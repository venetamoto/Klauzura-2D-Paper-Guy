using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInputReader))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Pohyb")]
    public float walkSpeed = 7f;
    public float acceleration = 60f;
    public float deceleration = 70f;
    public float airControl = 0.65f;

    [Header("Skok")]
    public float jumpForce = 13f;
    public float gravityScale = 3f;
    public float fallGravityMultiplier = 1.8f;
    public float lowJumpGravityMultiplier = 2.5f;
    public float maxFallSpeed = 22f;
    public float coyoteTime = 0.1f;
    public float jumpBuffer = 0.12f;

    [Header("Zem")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayers;

    [Header("Klouzani po grafu")]
    public float slideGravity = 25f;
    public float slideMaxSpeed = 22f;
    public Vector2 slideLaunch = new Vector2(8f, 6f);

    [Header("Jidlo")]
    public float eatDuration = 0.35f;

    public bool IsGrounded { get; private set; }
    public bool IsSliding { get; private set; }
    public bool IsEating => eatTimer > 0f;
    public int Facing { get; private set; } = 1;
    public Vector2 Velocity => rb.linearVelocity;

    public event Action<float> Landed;

    Rigidbody2D rb;
    PlayerInputReader input;
    float footOffset;

    float lastGroundedTime = -10f;
    bool jumped;
    float fallSpeed;
    float eatTimer;

    FunctionGraphPath slidePath;
    float slideDistance;
    float slideSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<PlayerInputReader>();

        rb.gravityScale = gravityScale;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.useFullKinematicContacts = true;

        Collider2D col = GetComponent<Collider2D>();
        footOffset = col != null ? rb.position.y - col.bounds.min.y : 0.5f;
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        if (IsSliding)
        {
            Slide(dt);
            return;
        }

        CheckGround();

        if (eatTimer > 0f) eatTimer -= dt;
        float move = IsEating ? 0f : input.MoveX;

        Vector2 v = rb.linearVelocity;

        float targetSpeed = move * walkSpeed;
        float rate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        if (!IsGrounded) rate *= airControl;
        v.x = Mathf.MoveTowards(v.x, targetSpeed, rate * dt);

        if (move > 0.01f) Facing = 1;
        if (move < -0.01f) Facing = -1;

        bool canJump = !jumped && Time.time - lastGroundedTime <= coyoteTime;
        bool wantsJump = Time.time - input.LastJumpTime <= jumpBuffer;
        if (canJump && wantsJump)
        {
            v.y = jumpForce;
            jumped = true;
            IsGrounded = false;
            lastGroundedTime = -10f;
            input.LastJumpTime = -10f;
        }

        if (v.y < 0f) rb.gravityScale = gravityScale * fallGravityMultiplier;
        else if (v.y > 0f && jumped && !input.JumpHeld) rb.gravityScale = gravityScale * lowJumpGravityMultiplier;
        else rb.gravityScale = gravityScale;

        if (v.y < -maxFallSpeed) v.y = -maxFallSpeed;

        rb.linearVelocity = v;
    }

    void CheckGround()
    {
        bool wasGrounded = IsGrounded;
        Vector2 pos = groundCheck != null ? (Vector2)groundCheck.position : rb.position + Vector2.down * footOffset;

        bool goingUp = jumped && rb.linearVelocity.y > 0.1f;
        IsGrounded = !goingUp && Physics2D.OverlapCircle(pos, groundCheckRadius, groundLayers) != null;

        if (!IsGrounded)
        {
            fallSpeed = Mathf.Max(fallSpeed, -rb.linearVelocity.y);
            return;
        }

        lastGroundedTime = Time.time;
        jumped = false;

        if (!wasGrounded)
        {
            Landed?.Invoke(fallSpeed);
            fallSpeed = 0f;
        }
    }

    public void StartSlide(FunctionGraphPath path)
    {
        if (IsSliding || path == null) return;

        slidePath = path;
        slideDistance = path.ClosestDistance(rb.position + Vector2.down * footOffset);
        slideSpeed = Mathf.Max(3f, rb.linearVelocity.magnitude);

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = gravityScale;
        rb.bodyType = RigidbodyType2D.Kinematic;
        IsSliding = true;
    }

    void Slide(float dt)
    {
        Vector2 dir = slidePath.GetDirection(slideDistance);

        slideSpeed += -dir.y * slideGravity * dt;
        slideSpeed = Mathf.Clamp(slideSpeed, 2f, slideMaxSpeed);
        slideDistance += slideSpeed * dt;
        Facing = dir.x >= 0f ? 1 : -1;

        if (slideDistance >= slidePath.Length)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.position = slidePath.GetPoint(slidePath.Length) + Vector2.up * footOffset;
            float forward = Mathf.Max(Mathf.Abs(dir.x) * slideSpeed, slideLaunch.x);
            rb.linearVelocity = new Vector2(forward * Facing, slideLaunch.y);
            IsSliding = false;
            jumped = true;
            return;
        }

        rb.MovePosition(slidePath.GetPoint(slideDistance) + Vector2.up * footOffset);
    }

    public void Eat()
    {
        if (!IsSliding) eatTimer = eatDuration;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
