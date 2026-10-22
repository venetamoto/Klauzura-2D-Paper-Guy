using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    public float MoveX { get; private set; }
    public bool JumpHeld { get; private set; }
    public float LastJumpTime { get; set; } = -10f;

    void Update()
    {
        float x = 0f;
        bool jumpDown = false;
        bool jumpHeld = false;

        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;

            jumpDown = kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame;
            jumpHeld = kb.spaceKey.isPressed || kb.wKey.isPressed || kb.upArrowKey.isPressed;
        }

        Gamepad pad = Gamepad.current;
        if (pad != null)
        {
            x += pad.leftStick.x.ReadValue();
            jumpDown |= pad.buttonSouth.wasPressedThisFrame;
            jumpHeld |= pad.buttonSouth.isPressed;
        }

        MoveX = Mathf.Clamp(x, -1f, 1f);
        JumpHeld = jumpHeld;
        if (jumpDown) LastJumpTime = Time.time;
    }
}
