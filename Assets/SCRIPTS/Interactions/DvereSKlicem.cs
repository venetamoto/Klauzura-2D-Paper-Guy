using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DvereSKlicem : MonoBehaviour
{
    public float fadeTime = 0.5f;

    bool opening;

    void Awake()
    {
        Klic.mam = false;
        GetComponent<Collider2D>().isTrigger = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        TryOpen(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        TryOpen(collision);
    }

    // bez klice se neda projit, s klicem dvere zmizi
    void TryOpen(Collision2D collision)
    {
        if (opening || !Klic.mam) return;
        if (collision.collider.GetComponentInParent<PlayerController2D>() == null) return;

        opening = true;
        Debug.Log("Dvere odemceny");
        StartCoroutine(Open());
    }

    IEnumerator Open()
    {
        GetComponent<Collider2D>().enabled = false;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color c = sr.color;
        for (float t = 0f; t < fadeTime; t += Time.deltaTime)
        {
            c.a = 1f - t / fadeTime;
            sr.color = c;
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
