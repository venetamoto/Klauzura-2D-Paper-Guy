using UnityEngine;
using UnityEngine.InputSystem;

namespace PaperGuy.Player
{
    public class PlayerInputReader : MonoBehaviour
    {
        [Header("Akce (Player/Move, Player/Jump z InputSystem_Actions)")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference jumpAction;

        private InputAction _move;
        private InputAction _jump;
        private bool _ownsActions;
        private float _jumpPressedTime = float.NegativeInfinity;

        public float MoveX { get; private set; }

        public bool JumpHeld => _jump != null && _jump.IsPressed();

        public bool InputEnabled { get; set; } = true;

        private void Awake()
        {
            if (moveAction != null && jumpAction != null)
            {
                _move = moveAction.action;
                _jump = jumpAction.action;
            }
            else
            {
                _ownsActions = true;
                _move = new InputAction("Move", InputActionType.Value);
                _move.AddCompositeBinding("1DAxis")
                    .With("Negative", "<Keyboard>/a")
                    .With("Positive", "<Keyboard>/d");
                _move.AddCompositeBinding("1DAxis")
                    .With("Negative", "<Keyboard>/leftArrow")
                    .With("Positive", "<Keyboard>/rightArrow");
                _move.AddBinding("<Gamepad>/leftStick/x");

                _jump = new InputAction("Jump", InputActionType.Button);
                _jump.AddBinding("<Keyboard>/space");
                _jump.AddBinding("<Keyboard>/w");
                _jump.AddBinding("<Keyboard>/upArrow");
                _jump.AddBinding("<Gamepad>/buttonSouth");
            }
        }

        private void OnEnable()
        {
            _move.Enable();
            _jump.Enable();
            _jump.performed += OnJumpPerformed;
        }

        private void OnDisable()
        {
            _jump.performed -= OnJumpPerformed;
            if (_ownsActions)
            {
                _move.Disable();
                _jump.Disable();
            }
        }

        private void OnDestroy()
        {
            if (_ownsActions)
            {
                _move.Dispose();
                _jump.Dispose();
            }
        }

        private void Update()
        {
            if (!InputEnabled)
            {
                MoveX = 0f;
                return;
            }

            // Move z assetu je Vector2, záložní je float. Podporujeme obojí.
            MoveX = _move.expectedControlType == "Vector2"
                ? _move.ReadValue<Vector2>().x
                : _move.ReadValue<float>();
            MoveX = Mathf.Clamp(MoveX, -1f, 1f);
        }

        private void OnJumpPerformed(InputAction.CallbackContext _)
        {
            if (InputEnabled)
                _jumpPressedTime = Time.time;
        }

        public bool HasBufferedJump(float window) => Time.time - _jumpPressedTime <= window;

        public void ConsumeJump() => _jumpPressedTime = float.NegativeInfinity;
    }
}
