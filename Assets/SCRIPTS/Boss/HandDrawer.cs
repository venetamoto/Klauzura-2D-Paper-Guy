using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HandDrawer : MonoBehaviour
{
    public Transform hand;
    public EdibleCollectible drawing;
    public LineRenderer line;

    public Vector2[] shape =
    {
        new Vector2(0f, 0.8f), new Vector2(0.19f, 0.26f), new Vector2(0.76f, 0.25f),
        new Vector2(0.3f, -0.1f), new Vector2(0.47f, -0.65f), new Vector2(0f, -0.32f),
        new Vector2(-0.47f, -0.65f), new Vector2(-0.3f, -0.1f), new Vector2(-0.76f, 0.25f),
        new Vector2(-0.19f, 0.26f), new Vector2(0f, 0.8f)
    };

    public float drawSpeed = 2.5f;
    public float flyTime = 0.6f;
    public Vector2 offscreenOffset = new Vector2(5f, 10f);
    public Vector2 handOffset = Vector2.zero;

    bool started;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        hand.gameObject.SetActive(false);
        line.positionCount = 0;
        SetEdible(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (started) return;
        if (other.GetComponentInParent<PlayerController2D>() == null) return;

        started = true;
        StartCoroutine(Draw());
    }

    IEnumerator Draw()
    {
        Vector3[] points = new Vector3[shape.Length];
        for (int i = 0; i < shape.Length; i++)
            points[i] = drawing.transform.TransformPoint(shape[i]);

        Vector3 away = points[0] + (Vector3)offscreenOffset;

        hand.gameObject.SetActive(true);
        yield return Fly(away, points[0]);

        line.useWorldSpace = true;
        line.positionCount = 1;
        line.SetPosition(0, points[0]);

        for (int i = 1; i < points.Length; i++)
        {
            line.positionCount = i + 1;
            float length = Vector3.Distance(points[i - 1], points[i]);

            for (float t = 0f; t < length; t += drawSpeed * Time.deltaTime)
            {
                Vector3 tip = Vector3.Lerp(points[i - 1], points[i], t / length);
                line.SetPosition(i, tip);
                hand.position = tip + (Vector3)handOffset;
                yield return null;
            }
            line.SetPosition(i, points[i]);
        }

        yield return Fly(points[points.Length - 1], away);
        hand.gameObject.SetActive(false);
        SetEdible(true);
    }

    IEnumerator Fly(Vector3 from, Vector3 to)
    {
        for (float t = 0f; t < flyTime; t += Time.deltaTime)
        {
            hand.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / flyTime)) + (Vector3)handOffset;
            yield return null;
        }
        hand.position = to + (Vector3)handOffset;
    }

    void SetEdible(bool value)
    {
        drawing.enabled = value;
        foreach (Collider2D c in drawing.GetComponents<Collider2D>())
            c.enabled = value;
    }
}
