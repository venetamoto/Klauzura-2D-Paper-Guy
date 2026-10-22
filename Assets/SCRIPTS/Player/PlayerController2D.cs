using System;
using System.Collections.Generic;
using PaperGuy.Cameras;
using PaperGuy.Interactions;
using PaperGuy.Level;
using UnityEngine;

namespace PaperGuy.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerInputReader))]
    public class PlayerController2D : MonoBehaviour
    {
        // =====================================================================
        //  Inspektor
        // =====================================================================

        [Header("Nastavení pohybu (Movement Settings)")]
        [Tooltip("Rychlost chůze v jednotkách za sekundu.")]
        [SerializeField, Min(0f)] private float walkSpeed = 7f;
        [Tooltip("Plynulost rozjezdu: o kolik jednotek/s se rychlost zvýší za sekundu.")]
        [SerializeField, Min(0f)] private float acceleration = 60f;
        [Tooltip("Plynulost dojezdu: jak rychle postava zastaví po puštění ovládání.")]
        [SerializeField, Min(0f)] private float deceleration = 70f;
        [Tooltip("Kolik kontroly má hráč ve vzduchu (0 = žádná, 1 = jako na zemi).")]
        [SerializeField, Range(0f, 1f)] private float airControl = 0.65f;

        [Header("Nastavení skoku (Jump Settings)")]
        [Tooltip("Síla skoku = počáteční vertikální rychlost. Výška ≈ síla² / (2 · 9.81 · Gravity Scale).")]
        [SerializeField, Min(0f)] private float jumpForce = 13f;
        [Tooltip("Základní tíže postavy (zapisuje se do Rigidbody2D.gravityScale).")]
        [SerializeField, Min(0f)] private float gravityScale = 3f;
        [Tooltip("Násobitel tíže při pádu — postava padá rychleji, než stoupá.")]
        [SerializeField, Min(1f)] private float fallGravityMultiplier = 1.8f;
        [Tooltip("Násobitel tíže, když hráč pustí skok při stoupání (krátký skok).")]
        [SerializeField, Min(1f)] private float lowJumpGravityMultiplier = 2.5f;
        [Tooltip("Maximální rychlost pádu.")]
        [SerializeField, Min(0f)] private float maxFallSpeed = 22f;
        [Tooltip("Coyote Time: kolik sekund po sestupu z hrany ještě jde skočit.")]
        [SerializeField, Min(0f)] private float coyoteTime = 0.1f;
        [Tooltip("Jump Buffer: kolik sekund si hra pamatuje stisk skoku před dopadem.")]
        [SerializeField, Min(0f)] private float jumpBuffer = 0.12f;

        [Header("Kontrola země (Ground Check Settings)")]
        [Tooltip("Bod u nohou hráče. Když je prázdné, použije se spodní hrana collideru.")]
        [SerializeField] private Transform groundCheck;
        [Tooltip("Poloměr kruhu, kterým se hledá zem.")]
        [SerializeField, Min(0.01f)] private float groundCheckRadius = 0.15f;
        [Tooltip("Vrstvy, které se počítají jako zem (nastavte na Ground).")]
        [SerializeField] private LayerMask groundLayers = 1;

        [Header("Omezení zpět (Back Limit)")]
        [Tooltip("Kamera, jejíž levý okraj omezuje hráče. Když je prázdné, hledá se ve scéně.")]
        [SerializeField] private CameraController2D cameraController;
        [SerializeField] private bool useBackLimit = true;
        [Tooltip("Odsazení od levého okraje záběru (obvykle polovina šířky collideru).")]
        [SerializeField, Min(0f)] private float leftEdgePadding = 0.4f;

        [Header("Klouzání po grafu (Slide Settings)")]
        [Tooltip("Jak silně sklon křivky zrychluje klouzání.")]
        [SerializeField] private float slideGravity = 25f;
        [Tooltip("Stálé zrychlení podél křivky (i na rovině).")]
        [SerializeField] private float slideBaseAcceleration = 2f;
        [SerializeField, Min(0f)] private float slideMinEntrySpeed = 3f;
        [SerializeField, Min(0f)] private float slideMinSpeed = 2f;
        [SerializeField, Min(0f)] private float slideMaxSpeed = 22f;
        [Tooltip("Kolik rychlosti si postava odnese z konce křivky.")]
        [SerializeField, Range(0f, 1.5f)] private float slideExitMomentum = 1f;
        [Tooltip("Jak moc se sprite naklání podle křivky.")]
        [SerializeField, Range(0f, 1f)] private float slideTiltAmount = 0.6f;

        [Header("Jídlo (Eat Settings)")]
        [Tooltip("Jak dlouho trvá „snědení“ — po tuto dobu je zablokovaný horizontální pohyb.")]
        [SerializeField, Min(0f)] private float eatDuration = 0.35f;

        [Header("Vizuál")]
        [Tooltip("Podřízený objekt se spritem — naklání se při klouzání.")]
        [SerializeField] private Transform visualRoot;
        [Tooltip("Od jaké rychlosti se přehrává animace běhu.")]
        [SerializeField, Min(0f)] private float runAnimThreshold = 0.1f;

        // =====================================================================
        //  Vnitřní stav
        // =====================================================================

        private Rigidbody2D _rb;
        private Collider2D _collider;
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

        // ---------- Parametry pro stavy (jen ke čtení) ----------
        public float RunAnimThreshold => runAnimThreshold;
        public float SlideGravity => slideGravity;
        public float SlideBaseAcceleration => slideBaseAcceleration;
        public float SlideMinEntrySpeed => slideMinEntrySpeed;
        public float SlideMinSpeed => slideMinSpeed;
        public float SlideMaxSpeed => slideMaxSpeed;
        public float SlideExitMomentum => slideExitMomentum;
        public float SlideTiltAmount => slideTiltAmount;
        public float EatDuration => eatDuration;

        // ---------- Události ----------
        public event Action Jumped;
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

        private void Reset()
        {
            int ground = LayerMask.GetMask("Ground");
            if (ground != 0) groundLayers = ground;
        }

        private void OnValidate()
        {
            // Změna Gravity Scale v inspektoru se projeví hned i za běhu.
            if (_rb != null && !IsSliding) _rb.gravityScale = gravityScale;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            Input = GetComponent<PlayerInputReader>();

            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rb.freezeRotation = true;
            _rb.gravityScale = gravityScale;

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
            float target = inputX * walkSpeed;
            float rate = Mathf.Abs(target) > 0.01f ? acceleration : deceleration;
            if (!IsGrounded) rate *= airControl;

            v.x = Mathf.MoveTowards(v.x, target, rate * dt);
            _rb.linearVelocity = v;

            if (Mathf.Abs(inputX) > 0.01f) SetFacing(inputX > 0f ? 1 : -1);
        }

        public bool TryConsumeJump()
        {
            bool canJump = !_jumpedThisAir && Time.time - _lastGroundedTime <= coyoteTime;
            if (!canJump || !Input.HasBufferedJump(jumpBuffer)) return false;

            Input.ConsumeJump();
            _jumpedThisAir = true;
            IsGrounded = false;
            _lastGroundedTime = float.NegativeInfinity;

            Vector2 v = _rb.linearVelocity;
            v.y = jumpForce;
            _rb.linearVelocity = v;
            Jumped?.Invoke();
            return true;
        }

        public void ApplyGravityModifiers()
        {
            Vector2 v = _rb.linearVelocity;
            float multiplier = 1f;
            if (v.y < 0f) multiplier = fallGravityMultiplier;
            else if (v.y > 0f && !Input.JumpHeld && _jumpedThisAir) multiplier = lowJumpGravityMultiplier;
            _rb.gravityScale = gravityScale * multiplier;

            if (v.y < -maxFallSpeed)
            {
                v.y = -maxFallSpeed;
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
            _rb.gravityScale = gravityScale;
            _fsm.ChangeState(_locomotion);
        }

        // =====================================================================
        //  API pro vnější svět
        // =====================================================================

        public void StartSlide(ISlidePath path)
        {
            if (path == null || IsDead || IsSliding) return;
            _slide.Setup(path);
            _rb.gravityScale = gravityScale;
            _fsm.ChangeState(_slide);
            SlideStarted?.Invoke();
        }

        public void FinishSlide(Vector2 endPosition, Vector2 exitVelocity)
        {
            ReturnToLocomotion();         // Exit() vrátí tělo do Dynamic
            _rb.position = endPosition;
            _rb.linearVelocity = exitVelocity;
            _jumpedThisAir = false;
            SlideFinished?.Invoke();
        }

        public bool TryEat(ICollectable item)
        {
            if (item == null || IsDead || !item.CanBeCollected) return false;

            item.Collect(this);
            // Během klouzání křivku nepřerušujeme — jen vyvoláme událost.
            if (_fsm.Current == _locomotion) _fsm.ChangeState(_eat);
            ItemEaten?.Invoke(item);
            return true;
        }

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
            _groundFilter.SetLayerMask(groundLayers);
            _groundFilter.useTriggers = false;

            int count = Physics2D.OverlapCircle(point, groundCheckRadius, _groundFilter, _groundHits);
            for (int i = 0; i < count; i++)
                if (_groundHits[i].attachedRigidbody != _rb) // vlastní collider se za zem nepočítá
                    return true;
            return false;
        }

        private void EnforceBackLimit()
        {
            if (cameraController == null) return;

            float minX = cameraController.LeftEdgeX + leftEdgePadding;
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
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
