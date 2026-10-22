using UnityEngine;

namespace PaperGuy.Player
{
    public class PlayerLocomotionState : PlayerState
    {
        public PlayerLocomotionState(PlayerController2D player) : base(player) { }

        public override PlayerAnimState AnimState
        {
            get
            {
                if (Player.IsGrounded)
                    return Mathf.Abs(Player.Velocity.x) > Player.RunAnimThreshold
                        ? PlayerAnimState.Run
                        : PlayerAnimState.Idle;

                return Player.Velocity.y > 0.01f ? PlayerAnimState.Jump : PlayerAnimState.Fall;
            }
        }

        public override void FixedTick(float dt)
        {
            Player.ApplyHorizontalMovement(Player.Input.MoveX, dt);
            Player.TryConsumeJump();
            Player.ApplyGravityModifiers();
        }
    }
}
