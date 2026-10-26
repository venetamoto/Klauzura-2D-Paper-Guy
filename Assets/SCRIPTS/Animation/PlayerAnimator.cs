using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public PlayerController2D player;

    [Header("Casti postavy")]
    public Transform body;
    public SpriteRenderer head;
    public SpriteRenderer cape;
    public LineRenderer legLeft;
    public LineRenderer legRight;

    [Header("Obrazky")]
    public Sprite headFront;
    public Sprite headFrontBlink;
    public Sprite headSide;
    public Sprite headSideBlink;
    public Sprite[] headEat;
    public Sprite capeFront;
    public Sprite capeSide;
    public Sprite[] capeFall;
    public Sprite[] capeEat;

    [Header("Nastaveni")]
    public float stepRate = 2.4f;
    public float stepLength = 0.2f;
    public float blinkEvery = 3f;
    public float headSwing = 6f;
    public float tumbleSpeed = 720f;

    // jidlo: jak dlouho je videt kazdy obrazek plastiku (soucet = 1.2 s)
    readonly float[] eatTimes = { 0.15f, 0.2f, 0.3f, 0.2f, 0.2f, 0.15f };
    readonly int[] eatHead = { 0, 1, 1, 1, 2, 2 };

    float phase;
    float spin;
    float blinkTimer;
    float eatClock;
    bool wasEating;

    void LateUpdate()
    {
        if (player == null || body == null) return;
        float dt = Time.deltaTime;

        blinkTimer += dt;
        bool blink = blinkTimer > blinkEvery;
        if (blinkTimer > blinkEvery + 0.15f) blinkTimer = Random.Range(0f, 1.5f);

        head.transform.localRotation = Quaternion.identity;
        cape.transform.localScale = Vector3.one;

        // kotrmelce po grafu
        if (player.IsSliding)
        {
            spin -= tumbleSpeed * dt * player.Facing;
            body.localScale = new Vector3(player.Facing, 1f, 1f);
            body.localRotation = Quaternion.Euler(0f, 0f, spin);
            Vector3 center = new Vector3(0f, 0.6f, 0f);
            body.localPosition = center - body.localRotation * Vector3.Scale(body.localScale, center);
            head.sprite = headSide;
            cape.sprite = capeSide;
            Tucked();
            return;
        }
        spin = 0f;
        body.localPosition = Vector3.zero;

        bool standing = player.IsGrounded && !player.IsEating && Mathf.Abs(player.Velocity.x) < 0.1f;

        // stoji: diva se na nas, mrka a pokyvuje hlavou
        if (standing)
        {
            body.localScale = Vector3.one;
            body.localRotation = Quaternion.identity;
            head.sprite = blink ? headFrontBlink : headFront;
            cape.sprite = capeFront;
            head.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 1.6f) * headSwing);
            cape.transform.localScale = new Vector3(1f, 1f + Mathf.Sin(Time.time * 2.5f) * 0.02f, 1f);
            Leg(legLeft, new Vector2(-0.16f, 0f), new Vector2(-0.18f, -0.5f), 0f, -1f);
            Leg(legRight, new Vector2(0.16f, 0f), new Vector2(0.18f, -0.5f), 0f, 1f);
            wasEating = false;
            return;
        }

        body.localScale = new Vector3(player.Facing, 1f, 1f);
        body.localRotation = Quaternion.identity;

        // jidlo: plastik se rozevre, z bricha vyleze tlama s jazykem (1.2 s)
        if (player.IsEating)
        {
            if (!wasEating) eatClock = 0f;
            wasEating = true;
            eatClock += dt;

            int i = 0;
            float t = eatClock;
            while (i < eatTimes.Length - 1 && t > eatTimes[i]) { t -= eatTimes[i]; i++; }

            cape.sprite = i < capeEat.Length ? capeEat[i] : capeSide;
            head.sprite = headEat[eatHead[i]];
            Leg(legLeft, new Vector2(-0.04f, 0f), new Vector2(-0.12f, -0.5f), 0.03f, 1f);
            Leg(legRight, new Vector2(0.04f, 0f), new Vector2(0.12f, -0.5f), 0.03f, 1f);
            return;
        }
        wasEating = false;
        head.sprite = blink ? headSideBlink : headSide;

        // ve vzduchu: nahoru skrcene nohy, dolu se plast rozevre jako padak
        if (!player.IsGrounded)
        {
            if (player.Velocity.y > -2f)
            {
                cape.sprite = capeSide;
                Tucked();
            }
            else
            {
                int flutter = (int)(Time.time * 8f) % 2;
                cape.sprite = capeFall != null && capeFall.Length > 1 ? capeFall[flutter] : capeSide;
                Leg(legLeft, new Vector2(-0.04f, 0f), new Vector2(-0.08f, -0.48f), 0.04f, 1f);
                Leg(legRight, new Vector2(0.04f, 0f), new Vector2(0.1f, -0.45f), 0.06f, 1f);
            }
            return;
        }

        // chuze
        cape.sprite = capeSide;
        float speed = Mathf.Abs(player.Velocity.x);
        phase += speed * stepRate * dt;
        body.localPosition = new Vector3(0f, Mathf.Abs(Mathf.Sin(phase)) * 0.06f, 0f);
        body.localRotation = Quaternion.Euler(0f, 0f, -4f);
        WalkLeg(legLeft, -0.04f, phase);
        WalkLeg(legRight, 0.04f, phase + Mathf.PI);
    }

    void WalkLeg(LineRenderer lr, float hipX, float a)
    {
        float footX = hipX + Mathf.Sin(a) * stepLength;
        float lift = Mathf.Max(0f, Mathf.Cos(a)) * 0.13f;
        Leg(lr, new Vector2(hipX, 0f), new Vector2(footX, -0.5f + lift - body.localPosition.y), 0.07f, 1f);
    }

    void Tucked()
    {
        Leg(legLeft, new Vector2(-0.04f, 0f), new Vector2(0.0f, -0.28f), 0.16f, 1f);
        Leg(legRight, new Vector2(0.04f, 0f), new Vector2(0.06f, -0.3f), 0.14f, 1f);
    }

    // noha: kycel - koleno - chodidlo - spicka
    void Leg(LineRenderer lr, Vector2 hip, Vector2 foot, float bend, float toe)
    {
        Vector2 mid = (hip + foot) / 2f;
        Vector2 dir = (foot - hip).normalized;
        Vector2 knee = mid + new Vector2(-dir.y, dir.x) * bend;
        lr.positionCount = 4;
        lr.SetPosition(0, hip);
        lr.SetPosition(1, knee);
        lr.SetPosition(2, foot);
        lr.SetPosition(3, foot + new Vector2(0.09f * toe, 0f));
    }
}
