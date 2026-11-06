using System.Collections;
using UnityEngine;

public class FadingPlatform : MonoBehaviour
{
    [Tooltip("Za jak dlouho se papir pod nohama rozdrobi.")]
    public float destroyDelay = 0.4f;

    bool started;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (started || collision.collider.GetComponentInParent<PlayerController2D>() == null) return;
        started = true;
        StartCoroutine(Drolit());
    }

    // papir se trese a bledne, pak zmizi
    IEnumerator Drolit()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Vector3 pos = transform.position;
        Color c = sr != null ? sr.color : Color.white;
        for (float t = 0f; t < destroyDelay; t += Time.deltaTime)
        {
            float k = t / destroyDelay;
            transform.position = pos + new Vector3(Random.Range(-0.04f, 0.04f), Random.Range(-0.04f, 0.04f), 0f);
            if (sr != null) sr.color = new Color(c.r, c.g, c.b, c.a * (1f - k * 0.7f));
            yield return null;
        }
        Destroy(gameObject);
    }
}
