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
    [Tooltip("Animace jidla od autorky - snimky jdou po rade behem jidla (jazyk).")]
    public Sprite[] jidloSnimky;

    [Header("Nastaveni")]
    public float blinkEvery = 3f;
    public float stepRate = 8f;
    public float tumbleSpeed = 720f;
    [Tooltip("Kde jsou chodidla vuci stredu postavy (spodek kolideru).")]
    public float feetY = -0.5f;

    float blinkTimer;
    float stepClock;
    float spin;
    PlayerInputReader input;
    float eatClock;
    bool wasEating;
    void LateUpdate()
    {
        if (player == null || kresba == null) return;
        Pose();
        // kresba stoji chodidly na spodku kolideru, ne ve stredu postavy
        kresba.transform.localPosition += new Vector3(0f, feetY, 0f);
    }

    void Pose()
    {
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

        // jidlo: snimky autorky po rade, nic se s nimi nekrouti
        if (player.IsEating)
        {
            if (!wasEating) eatClock = 0f;
            wasEating = true;
            eatClock += dt;
            if (jidloSnimky != null && jidloSnimky.Length > 0)
            {
                int i = (int)(eatClock / player.eatDuration * jidloSnimky.Length);
                kresba.sprite = jidloSnimky[Mathf.Clamp(i, 0, jidloSnimky.Length - 1)];
            }
            else kresba.sprite = jidlo != null ? jidlo : bok;
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

        // chuze jen kdyz hrac opravdu macka smer (na sikmych kresbach postava trochu klouze, i kdyz stoji)
        if (input == null) input = player.GetComponent<PlayerInputReader>();
        if (Mathf.Abs(input.MoveX) > 0.1f && Mathf.Abs(player.Velocity.x) > 0.1f)
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
