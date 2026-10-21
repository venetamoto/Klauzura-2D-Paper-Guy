using System;
using System.Collections.Generic;
using PaperGuy.Cameras;
using PaperGuy.Interactions;
using PaperGuy.Level;
using UnityEngine;

namespace PaperGuy.Player
{
    /// <summary>
    /// Vlastník „hardwaru“ postavy (Rigidbody2D, vstup, nastavení) a stavového automatu.
    /// Stavy volají jeho primitiva (ApplyHorizontalMovement, TryConsumeJump…),
    /// vnější svět s ním komunikuje přes veřejné metody a C# události.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerInputReader))]
    public class PlayerController2D : MonoBehaviour
    {
        [SerializeField] private PlayerMovementSettings settings = new PlayerMovementSettings();

        [Header("Reference")]
        [SerializeField] private Transform groundCheck;
        [Tooltip("Podřízený objekt se spritem — naklání se při klouzání.")]
        [SerializeField] private Transform visualRoot;
        [Tooltip("Kamera, jejíž levý okraj omezuje hráče. Když je prázdné, hledá se ve scéně.")]
        [SerializeField] private CameraController2D cameraController;
        [SerializeField] private bool useBackLimit = true;

        private Rigidbody2D _rb;
        private Collider2D _collider;
        private float _baseGravityScale;
        private float _lastGroundedTime = float.NegativeInfinity;
        private float _maxFallSpeedThisAir;
        private bool _jumpedThisAir;
        private ContactFilter2D _groundFilter;
        private readonly List<Collider2D> _groundHits = new List<Collider2D>(4);

        // Vytváří se hned, aby se šlo přihlásit k StateChanged ještě před naším Awake.
        private readonly PlayerStateMachine _fsm = new PlayerStateMachine();
        private PlayerLocomotionState _locomotion;
        private PlayerSlideState _slide;
        private PlayerEatState _eat;
        private PlayerDisabledState _disabled;

        // ---------- Veřejný stav ----------
        public PlayerMovementSettings Settings => settings;
        public PlayerInputReader Input { get; private set; }
        public Rigidbody2D Body => _rb;
        public Vector2 Velocity => _rb.linearVelocity;
        public bool IsGrounded { get; private set; }
        public int FacingDirection { get; private set; } = 1;
        public float FootOffset { get; private set; }
        public PlayerState CurrentState => _fsm.Current;
        public PlayerAnimState AnimState => _fsm.Current?.AnimState ?? PlayerAnimState.Idle;
        public bool IsSliding => _fsm.Current == _slide;
        public bool IsDead => _fsm.Current == _disabled && _disabled.IsDead;

        // ---------- Události ----------
        public event Action Jumped;
        /// <summary>Argument — rychlost pádu v okamžiku dopadu na zem (kladná).</summary>
        public event Action<float> Landed;
        public event Action SlideStarted;
        public event Action SlideFinished;
        public event Action<ICollectable> ItemEaten;
        public event Action Died;
        public event Action<PlayerState, PlayerState> StateChanged
        {
            add => _fsm.StateChanged += value;
            remove => _fsm.StateChanged -= value;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            Input = GetComponent<PlayerInputReader>();

            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rb.freezeRotation = true;
            _baseGravityScale = _rb.gravityScale;

            // Vzdálenost od pivotu těla k „chodidlům“ — aby při klouzání stály nohy na křivce.
            FootOffset = _collider != null ? _rb.position.y - _collider.bounds.min.y : 0.5f;

            if (cameraController == null)
                cameraController = FindAnyObjectByType<CameraController2D>();

            _locomotion = new PlayerLocomotionState(this);
            _slide = new PlayerSlideState(this);
            _eat = new PlayerEatState(this);
            _disabled = new PlayerDisabledState(this);
            _fsm.ChangeState(_locomotion);
        }

        private void Update() => _fsm.Tick(Time.deltaTime);

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;

            UpdateGrounded();
            _fsm.FixedTick(dt);

            if (useBackLimit && _fsm.Current != null && _fsm.Current.UsesBackLimit)
                EnforceBackLimit();
        }

        // =====================================================================
        //  Primitiva pro stavy
        // =====================================================================

        public void ApplyHorizontalMovement(float inputX, float dt)
        {
            Vector2 v = _rb.linearVelocity;
            float target = inputX * settings.moveSpeed;
            float rate = Mathf.Abs(target) > 0.01f ? settings.acceleration : settings.deceleration;
            if (!IsGrounded) rate *= settings.airControl;

            v.x = Mathf.MoveTowards(v.x, target, rate * dt);
            _rb.linearVelocity = v;

            if (Mathf.Abs(inputX) > 0.01f) SetFacing(inputX > 0f ? 1 : -1);
        }

        /// <summary>Skok s ohledem na buffer vstupu a „coyote time“.</summary>
        public bool TryConsumeJump()
        {
            bool canJump = !_jumpedThisAir && Time.time - _lastGroundedTime <= settings.coyoteTime;
            if (!canJump || !Input.HasBufferedJump) return false;

            Input.ConsumeJump();
            _jumpedThisAir = true;
            IsGrounded = false;
            _lastGroundedTime = float.NegativeInfinity;

            Vector2 v = _rb.linearVelocity;
            v.y = settings.jumpVelocity;
            _rb.linearVelocity = v;
            Jumped?.Invoke();
            return true;
        }

        /// <summary>Těžší pád a krátký skok při puštění tlačítka.</summary>
        public void ApplyGravityModifiers()
        {
            Vector2 v = _rb.linearVelocity;
            float multiplier = 1f;
            if (v.y < 0f) multiplier = settings.fallGravityMultiplier;
            else if (v.y > 0f && !Input.JumpHeld && _jumpedThisAir) multiplier = settings.lowJumpGravityMultiplier;
            _rb.gravityScale = _baseGravityScale * multiplier;

            if (v.y < -settings.maxFallSpeed)
            {
                v.y = -settings.maxFallSpeed;
                _rb.linearVelocity = v;
            }
        }

        public void SetFacing(int direction)
        {
            if (direction != 0) FacingDirection = direction > 0 ? 1 : -1;
        }

        public void SetVisualTilt(float degrees)
        {
            if (visualRoot != null) visualRoot.localRotation = Quaternion.Euler(0f, 0f, degrees);
        }

        public void ReturnToLocomotion()
        {
            _rb.gravityScale = _baseGravityScale;
            _fsm.ChangeState(_locomotion);
        }

        // =====================================================================
        //  API pro vnější svět
        // =====================================================================

        /// <summary>Začít klouzat po trajektorii (volá SlideTrigger).</summary>
        public void StartSlide(ISlidePath path)
        {
            if (path == null || IsDead || IsSliding) return;
            _slide.Setup(path);
            _rb.gravityScale = _baseGravityScale;
            _fsm.ChangeState(_slide);
            SlideStarted?.Invoke();
        }

        /// <summary>Volá stav klouzání na konci křivky.</summary>
        public void FinishSlide(Vector2 endPosition, Vector2 exitVelocity)
        {
            ReturnToLocomotion();         // Exit() vrátí tělo do Dynamic
            _rb.position = endPosition;
            _rb.linearVelocity = exitVelocity;
            _jumpedThisAir = false;
            SlideFinished?.Invoke();
        }

        /// <summary>Pokusit se sníst předmět. Vrací true, pokud byl předmět přijat.</summary>
        public bool TryEat(ICollectable item)
        {
            if (item == null || IsDead || !item.CanBeCollected) return false;

            item.Collect(this);
            // Během klouzání křivku nepřerušujeme — jen vyvoláme událost.
            if (_fsm.Current == _locomotion) _fsm.ChangeState(_eat);
            ItemEaten?.Invoke(item);
            return true;
        }

        /// <summary>Vypnout/zapnout ovládání (cutscény).</summary>
        public void SetControlEnabled(bool enabled)
        {
            if (IsDead) return;
            if (enabled) ReturnToLocomotion();
            else
            {
                _disabled.IsDead = false;
                _fsm.ChangeState(_disabled);
            }
        }

        public void Kill()
        {
            if (IsDead) return;
            _disabled.IsDead = true;
            _fsm.ChangeState(_disabled);
            _rb.linearVelocity = Vector2.zero;
            Died?.Invoke();
        }

        // =====================================================================
        //  Vnitřní logika
        // =====================================================================

        private void UpdateGrounded()
        {
            bool wasGrounded = IsGrounded;
            Vector2 checkPos = groundCheck != null ? (Vector2)groundCheck.position : _rb.position + Vector2.down * FootOffset;

            // Dokud po skoku stoupáme, nepovažujeme se za stojící na zemi (jinak „dvojitý“ skok u hrany).
            bool rising = _rb.linearVelocity.y > 0.1f && _jumpedThisAir;
            IsGrounded = !rising && HasGroundAt(checkPos);

            if (!IsGrounded)
            {
                _maxFallSpeedThisAir = Mathf.Max(_maxFallSpeedThisAir, -_rb.linearVelocity.y);
                return;
            }

            _lastGroundedTime = Time.time;
            _jumpedThisAir = false;

            if (!wasGrounded)
            {
                Landed?.Invoke(_maxFallSpeedThisAir);
                _maxFallSpeedThisAir = 0f;
            }
        }

        private bool HasGroundAt(Vector2 point)
        {
            _groundFilter.SetLayerMask(settings.groundLayers);
            _groundFilter.useTriggers = false;

            int count = Physics2D.OverlapCircle(point, settings.groundCheckRadius, _groundFilter, _groundHits);
            for (int i = 0; i < count; i++)
                if (_groundHits[i].attachedRigidbody != _rb) // vlastní collider se za zem nepočítá
                    return true;
            return false;
        }

        /// <summary>
        /// Pojistka nad fyzickou stěnou CameraLeftBoundary: nedovolíme odejít vlevo ze záběru
        /// ani při tunelování, ani když stěna ve scéně není.
        /// </summary>
        private void EnforceBackLimit()
        {
            if (cameraController == null) return;

            float minX = cameraController.LeftEdgeX + settings.leftEdgePadding;
            Vector2 pos = _rb.position;
            if (pos.x >= minX) return;

            _rb.position = new Vector2(minX, pos.y);
            Vector2 v = _rb.linearVelocity;
            if (v.x < 0f)
            {
                v.x = 0f;
                _rb.linearVelocity = v;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, settings.groundCheckRadius);
        }
    }
}
