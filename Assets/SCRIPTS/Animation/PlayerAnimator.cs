using System.Collections.Generic;
using PaperGuy.Interactions;
using PaperGuy.Player;
using UnityEngine;

namespace PaperGuy.Animations
{
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private PlayerController2D player;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [Tooltip("Dívá se sprite ve výchozím stavu doprava?")]
        [SerializeField] private bool spriteFacesRight = true;

        [Header("Přehrávání")]
        [Tooltip("Přehrávat stavy Idle/Run/Jump/Fall/Slide/Eat/Dead přímo přes CrossFade.")]
        [SerializeField] private bool driveStatesDirectly = true;
        [SerializeField] private float crossFadeDuration = 0.06f;
        [SerializeField] private int layer = 0;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int IsSlidingHash = Animator.StringToHash("IsSliding");
        private static readonly int StateHash = Animator.StringToHash("State");
        private static readonly int EatHash = Animator.StringToHash("Eat");

        private readonly Dictionary<PlayerAnimState, int> _stateHashes = new();
        private readonly HashSet<int> _parameters = new();
        private PlayerAnimState? _current;

        private void Reset()
        {
            player = GetComponentInParent<PlayerController2D>();
            animator = GetComponentInChildren<Animator>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Awake()
        {
            if (player == null) player = GetComponentInParent<PlayerController2D>();
            if (animator == null) animator = GetComponentInChildren<Animator>();

            foreach (PlayerAnimState s in System.Enum.GetValues(typeof(PlayerAnimState)))
                _stateHashes[s] = Animator.StringToHash(s.ToString());

            if (animator != null && animator.runtimeAnimatorController != null)
                foreach (AnimatorControllerParameter p in animator.parameters)
                    _parameters.Add(p.nameHash);
        }

        private void OnEnable()
        {
            if (player != null) player.ItemEaten += OnItemEaten;
        }

        private void OnDisable()
        {
            if (player != null) player.ItemEaten -= OnItemEaten;
        }

        private void LateUpdate()
        {
            if (player == null) return;

            if (spriteRenderer != null)
                spriteRenderer.flipX = spriteFacesRight ? player.FacingDirection < 0 : player.FacingDirection > 0;

            // Greybox bez Animator Controlleru — jen otáčíme sprite.
            if (animator == null || animator.runtimeAnimatorController == null) return;

            PlayerAnimState state = player.AnimState;
            Vector2 velocity = player.Velocity;

            SetFloat(SpeedHash, Mathf.Abs(velocity.x));
            SetFloat(VerticalSpeedHash, velocity.y);
            SetBool(IsGroundedHash, player.IsGrounded);
            SetBool(IsSlidingHash, player.IsSliding);
            SetInt(StateHash, (int)state);

            if (driveStatesDirectly && _current != state)
            {
                int hash = _stateHashes[state];
                if (animator.HasState(layer, hash))
                    animator.CrossFadeInFixedTime(hash, crossFadeDuration, layer);
            }
            _current = state;
        }

        private void OnItemEaten(ICollectable _)
        {
            // Trigger je potřeba jen v režimu „přes přechody“ (např. Eat přes klouzání na samostatné vrstvě).
            if (animator != null && _parameters.Contains(EatHash)) animator.SetTrigger(EatHash);
        }

        private void SetFloat(int hash, float value) { if (_parameters.Contains(hash)) animator.SetFloat(hash, value); }
        private void SetBool(int hash, bool value) { if (_parameters.Contains(hash)) animator.SetBool(hash, value); }
        private void SetInt(int hash, int value) { if (_parameters.Contains(hash)) animator.SetInteger(hash, value); }
    }
}
