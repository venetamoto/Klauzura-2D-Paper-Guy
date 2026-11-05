using UnityEngine;

public class PlayerKresba : MonoBehaviour
{
    public PlayerController2D player;
    public SpriteRenderer kresba;

    [Header("Kresby od autorky")]
    public Sprite stoji;
    public Sprite stojiMrk;
    public Sprite[] chuze;
    public Sprite bok;
    public Sprite skok;
    public Sprite pad;
    public Sprite jidlo;

    [Header("Nastaveni")]
    public float blinkEvery = 3f;
    public float stepRate = 8f;
    public float tumbleSpeed = 720f;

    float blinkTimer;
    float stepClock;
    float spin;
    float eatClock;
    bool wasEating;

    void LateUpdate()
    {
        if (player == null || kresba == null) return;
        float dt = Time.deltaTime;
        Transform t = kresba.transform;

        blinkTimer += dt;
        bool blink = blinkTimer > blinkEvery;
        if (blinkTimer > blinkEvery + 0.15f) blinkTimer = Random.Range(0f, 1.5f);

        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;
        kresba.flipX = player.Facing < 0;

        // kotrmelce po grafu
        if (player.IsSliding)
        {
            spin -= tumbleSpeed * dt * player.Facing;
            kresba.sprite = skok;
            t.localPosition = new Vector3(0f, 0.8f, 0f);
            t.localRotation = Quaternion.Euler(0f, 0f, spin);
            t.localPosition -= t.localRotation * new Vector3(0f, 0.8f, 0f);
            return;
        }
        spin = 0f;

        // jidlo: napred se zakloni, pak chnapne dopredu a trikrat zprudka zvykne
        if (player.IsEating)
        {
            if (!wasEating) eatClock = 0f;
            wasEating = true;
            eatClock += dt;
            kresba.sprite = jidlo != null ? jidlo : bok;
            float f = player.Facing;

            if (eatClock < 0.2f)
            {
                float k = eatClock / 0.2f;
                t.localRotation = Quaternion.Euler(0f, 0f, 14f * k * f);
                t.localScale = new Vector3(1f - 0.1f * k, 1f + 0.12f * k, 1f);
            }
            else if (eatClock < 0.35f)
            {
                float k = (eatClock - 0.2f) / 0.15f;
                t.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(14f, -18f, k) * f);
                t.localPosition = new Vector3(0.2f * k * f, 0f, 0f);
                t.localScale = new Vector3(1f + 0.12f * k, 1f - 0.1f * k, 1f);
            }
            else
            {
                float k = (eatClock - 0.35f) / 0.85f;
                float chomp = Mathf.Abs(Mathf.Sin(k * Mathf.PI * 3f)) * (1f - k * 0.6f);
                t.localRotation = Quaternion.Euler(0f, 0f, -6f * (1f - k) * f);
                t.localPosition = new Vector3(0.2f * (1f - k) * f, chomp * 0.12f, 0f);
                t.localScale = new Vector3(1f + chomp * 0.22f, 1f - chomp * 0.18f, 1f);
            }
            return;
        }
        wasEating = false;

        // ve vzduchu: nahoru skok, dolu padak
        if (!player.IsGrounded)
        {
            if (player.Velocity.y > -2f)
            {
                kresba.sprite = skok;
            }
            else
            {
                kresba.sprite = pad;
                kresba.flipX = false;
                t.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 9f) * 4f);
            }
            return;
        }

        // chuze
        if (Mathf.Abs(player.Velocity.x) > 0.1f)
        {
            stepClock += dt * stepRate;
            kresba.sprite = chuze != null && chuze.Length > 0 ? chuze[(int)stepClock % chuze.Length] : bok;
            // poskakovani v kroku, kdyz je kresba chuze jen jedna
            t.localPosition = new Vector3(0f, Mathf.Abs(Mathf.Sin(stepClock * 0.8f)) * 0.1f, 0f);
            t.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(stepClock * 0.8f) * 4f);
            return;
        }

        // stoji: diva se na nas, mrka a dycha
        kresba.sprite = blink ? stojiMrk : stoji;
        kresba.flipX = false;
        t.localScale = new Vector3(1f, 1f + Mathf.Sin(Time.time * 2.5f) * 0.015f, 1f);
    }
}
