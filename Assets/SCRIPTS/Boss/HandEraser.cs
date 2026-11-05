using System.Collections;
using UnityEngine;

public class HandEraser : MonoBehaviour
{
    public Transform hand;
    public float eraseTime = 1.6f;
    public float flyTime = 0.5f;
    public Vector2 offscreenOffset = new Vector2(5f, 9f);

    [Header("Smaze se az po snedeni")]
    public EdibleCollectible needEaten;

    [Header("Potom ruka nakresli malou plosinu")]
    public GameObject drawAfter;
    public float drawTime = 1f;

    SpriteRenderer sr;
    Collider2D col;
    bool started;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        hand.gameObject.SetActive(false);
        if (drawAfter != null) drawAfter.SetActive(false);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (started) return;
        if (needEaten != null && !needEaten.IsEaten) return;

        PlayerController2D player = collision.collider.GetComponentInParent<PlayerController2D>();
        if (player == null || !player.IsGrounded) return;
        if (player.transform.position.y < col.bounds.max.y) return;

        started = true;
        StartCoroutine(Erase());
    }

    IEnumerator Erase()
    {
        Bounds b = col.bounds;
        Vector3 top = new Vector3(b.center.x, b.max.y, 0f);
        Vector3 away = top + (Vector3)offscreenOffset;

        hand.gameObject.SetActive(true);
        yield return Fly(away, top);

        Color c = sr.color;
        for (float t = 0f; t < eraseTime; t += Time.deltaTime)
        {
            float x = Mathf.Sin(t * 18f) * b.extents.x * 0.9f;
            float y = Mathf.Abs(Mathf.Cos(t * 18f)) * 0.1f;
            hand.position = top + new Vector3(x, y, 0f);

            c.a = 1f - t / eraseTime;
            sr.color = c;
            yield return null;
        }

        sr.enabled = false;
        col.enabled = false;

        if (drawAfter != null) yield return DrawPlatform();

        yield return Fly(hand.position, away);
        hand.gameObject.SetActive(false);
    }

    // ruka jede zleva doprava a plosina za ni roste
    IEnumerator DrawPlatform()
    {
        Transform p = drawAfter.transform;
        Vector3 full = p.localScale;
        Bounds pb = drawAfter.GetComponent<Collider2D>().bounds;
        Vector3 left = new Vector3(pb.min.x, pb.max.y, 0f);
        Vector3 right = new Vector3(pb.max.x, pb.max.y, 0f);
        Vector3 center = p.position;

        yield return Fly(hand.position, left);

        drawAfter.SetActive(true);
        for (float t = 0f; t < drawTime; t += Time.deltaTime)
        {
            float k = t / drawTime;
            hand.position = Vector3.Lerp(left, right, k);
            p.localScale = new Vector3(full.x * k, full.y, full.z);
            p.position = new Vector3(Mathf.Lerp(left.x, center.x, k), center.y, center.z);
            yield return null;
        }
        p.localScale = full;
        p.position = center;
    }

    IEnumerator Fly(Vector3 from, Vector3 to)
    {
        for (float t = 0f; t < flyTime; t += Time.deltaTime)
        {
            hand.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / flyTime));
            yield return null;
        }
        hand.position = to;
    }
}
