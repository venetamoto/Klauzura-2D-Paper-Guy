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

    [Header("Varvara2: lek a utek (zapina SmazanaChocholka)")]
    public bool lek;
    public bool utika;
    public bool krci;
    [Header("VarvaraKonec: panika na ostrove a mazani gumou (zapina EraseEffect)")]
    public bool panika;
    public bool mazany;

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
        // pri uteku (start v menu, utek ve Varvara2) bezi vzdy doprava - nesmi bezet pozadu
        if (utika) kresba.flipX = false;

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

        // mazany: roztazeny plast, hrozne se trese a krci pod gumou
        if (mazany)
        {
            float p = Time.time;
            kresba.sprite = pad;
            kresba.flipX = false;
            float rub = Mathf.Abs(Mathf.Sin(p * 16f));
            t.localPosition = new Vector3(Random.Range(-0.09f, 0.09f), Random.Range(-0.04f, 0.04f), 0f);
            t.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(p * 23f) * 9f);
            t.localScale = new Vector3(1f + rub * 0.1f, 1f - rub * 0.14f, 1f);
            return;
        }

        // krci se: guma mu drhne hlavu, on se prikrci a trese
        if (krci)
        {
            float p = Time.time;
            float rub = Mathf.Abs(Mathf.Sin(p * 16f));
            kresba.sprite = stoji;
            kresba.flipX = false;
            t.localPosition = new Vector3(Random.Range(-0.04f, 0.04f), 0f, 0f);
            t.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(p * 21f) * 3f);
            t.localScale = new Vector3(1.05f + rub * 0.06f, 0.9f - rub * 0.1f, 1f);
            return;
        }

        // lek: vyskoci a trese se
        if (lek)
        {
            kresba.sprite = skok;
            t.localPosition = new Vector3(Random.Range(-0.06f, 0.06f), 0.2f, 0f);
            return;
        }

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
        if (utika || (Mathf.Abs(input.MoveX) > 0.1f && Mathf.Abs(player.Velocity.x) > 0.1f))
        {
            stepClock += dt * stepRate * (utika ? 1.8f : 1f);
            kresba.sprite = chuze != null && chuze.Length > 0 ? chuze[(int)stepClock % chuze.Length] : bok;
            // poskakovani v kroku, kdyz je kresba chuze jen jedna
            t.localPosition = new Vector3(0f, Mathf.Abs(Mathf.Sin(stepClock * 0.8f)) * 0.1f, 0f);
            t.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(stepClock * 0.8f) * 4f);
            return;
        }

        // panika (kdyz stoji): rozhlizi se sem a tam, nervozne poskakuje a trese se
        if (panika)
        {
            float p = Time.time;
            bool otoceny = Mathf.Repeat(p, 0.8f) < 0.4f;
            kresba.sprite = Mathf.Repeat(p, 2.4f) < 1.6f ? (chuze != null && chuze.Length > 0 ? chuze[0] : bok) : stoji;
            kresba.flipX = otoceny;
            float hop = Mathf.Abs(Mathf.Sin(p * 7f)) * 0.18f;
            t.localPosition = new Vector3(Random.Range(-0.03f, 0.03f), hop, 0f);
            t.localScale = new Vector3(1f + hop * 0.3f, 1f - hop * 0.3f, 1f);
            return;
        }

        // stoji: diva se na nas, mrka a dycha
        kresba.sprite = blink ? stojiMrk : stoji;
        kresba.flipX = false;
        t.localScale = new Vector3(1f, 1f + Mathf.Sin(Time.time * 2.5f) * 0.015f, 1f);
    }
}
