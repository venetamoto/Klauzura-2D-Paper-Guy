using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public PlayerController2D player;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    void LateUpdate()
    {
        if (player == null) return;

        if (spriteRenderer != null)
            spriteRenderer.flipX = player.Facing < 0;

        if (animator == null || animator.runtimeAnimatorController == null) return;

        animator.SetFloat("Speed", Mathf.Abs(player.Velocity.x));
        animator.SetFloat("VerticalSpeed", player.Velocity.y);
        animator.SetBool("IsGrounded", player.IsGrounded);
        animator.SetBool("IsSliding", player.IsSliding);
        animator.SetBool("IsEating", player.IsEating);
    }
}
