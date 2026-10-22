using System.Collections;
using UnityEngine;

public class HandEraser : MonoBehaviour
{
    public Transform hand;
    public float eraseTime = 1.6f;
    public float flyTime = 0.5f;
    public Vector2 offscreenOffset = new Vector2(5f, 9f);

    SpriteRenderer sr;
    Collider2D col;
    bool started;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        hand.gameObject.SetActive(false);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (started) return;

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

        yield return Fly(hand.position, away);
        hand.gameObject.SetActive(false);
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
