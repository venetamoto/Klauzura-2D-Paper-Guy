using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public PlayerController2D player;
    public SpriteRenderer spriteRenderer;

    [Header("Z boku")]
    public Sprite[] walkFrames;
    public Sprite[] runFrames;
    public Sprite[] eatFrames;
    public Sprite tumbleFrame;

    [Header("Zepredu (kdyz stoji)")]
    public GameObject front;
    public SpriteRenderer frontHead;
    public SpriteRenderer frontBody;
    public Sprite[] headFrames;

    [Header("Rychlost")]
    public float walkFps = 10f;
    public float runFps = 9f;
    public float tumbleSpeed = 720f;
    public float blinkEvery = 3f;
    public float headSwing = 6f;
    public float bobHeight = 0.06f;

    Sprite[] current;
    int frame;
    float timer;
    float blinkTimer;
    float walkPhase;
    float spin;

    void LateUpdate()
    {
        if (player == null || spriteRenderer == null) return;

        Transform visual = spriteRenderer.transform;
        spriteRenderer.flipX = player.Facing < 0;

        bool standing = player.IsGrounded && !player.IsSliding && !player.IsEating && Mathf.Abs(player.Velocity.x) < 0.1f;
        ShowFront(standing);
        if (standing)
        {
            current = null;
            Idle();
            return;
        }

        visual.localScale = Vector3.one;

        if (player.IsSliding)
        {
            spin -= tumbleSpeed * Time.deltaTime * player.Facing;
            visual.localRotation = Quaternion.Euler(0f, 0f, spin);
            visual.localPosition = Vector3.zero;
            if (tumbleFrame != null) spriteRenderer.sprite = tumbleFrame;
            current = null;
            return;
        }
        spin = 0f;

        if (player.IsEating)
        {
            Play(eatFrames, eatFrames.Length / player.eatDuration, false);
            visual.localPosition = Vector3.zero;
            visual.localRotation = Quaternion.identity;
            return;
        }

        if (!player.IsGrounded)
        {
            Play(runFrames, runFps, true);
            visual.localPosition = Vector3.zero;
            visual.localRotation = Quaternion.identity;
            return;
        }

        float speed = Mathf.Clamp(Mathf.Abs(player.Velocity.x) / 7f, 0.5f, 1.2f);
        Play(walkFrames, walkFps * speed, true);
        walkPhase += Time.deltaTime * walkFps * speed * Mathf.PI / 2f;
        visual.localPosition = new Vector3(0f, Mathf.Abs(Mathf.Sin(walkPhase)) * bobHeight, 0f);
        visual.localRotation = Quaternion.Euler(0f, 0f, -4f * player.Facing);
    }

    void ShowFront(bool show)
    {
        if (front == null) return;
        if (front.activeSelf != show) front.SetActive(show);
        spriteRenderer.enabled = !show;
    }

    // stoji: diva se na nas, mrka a pokyvuje hlavou
    void Idle()
    {
        if (frontHead == null || headFrames == null || headFrames.Length == 0) return;

        blinkTimer += Time.deltaTime;
        bool blink = blinkTimer > blinkEvery;
        if (blinkTimer > blinkEvery + 0.15f) blinkTimer = Random.Range(0f, 1.5f);
        frontHead.sprite = headFrames[blink && headFrames.Length > 1 ? 1 : 0];

        frontHead.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 1.6f) * headSwing);

        if (frontBody != null)
        {
            float breathe = Mathf.Sin(Time.time * 2.5f) * 0.02f;
            frontBody.transform.localScale = new Vector3(1f - breathe * 0.5f, 1f + breathe, 1f);
        }
    }

    void Play(Sprite[] frames, float fps, bool loop)
    {
        if (frames == null || frames.Length == 0) return;

        if (frames != current)
        {
            current = frames;
            frame = 0;
            timer = 0f;
        }

        timer += Time.deltaTime;
        if (timer >= 1f / fps)
        {
            timer -= 1f / fps;
            frame++;
            if (frame >= frames.Length) frame = loop ? 0 : frames.Length - 1;
        }
        spriteRenderer.sprite = frames[frame];
    }
}
