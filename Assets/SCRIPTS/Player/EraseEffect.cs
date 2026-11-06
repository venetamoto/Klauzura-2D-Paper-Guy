using System.Collections;
using UnityEngine;

public class EraseEffect : MonoBehaviour
{
    public float eraseTime = 1.5f;
    public float fadeTime = 1.5f;
    [Tooltip("Bily ctverec pres celou obrazovku.")]
    public Sprite bila;
    [Tooltip("Napis \"Kazdy sesit jednou konci...\" nakresleny tuzkou.")]
    public Sprite epilog;
    public float epilogWidth = 0.6f;

    bool stopped;
    bool erasing;

    // hrac ztrati ovladani a zustane stat
    public void Zastav()
    {
        if (stopped) return;
        stopped = true;
        GetComponent<PlayerController2D>().enabled = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        Debug.Log("Hrac ztratil ovladani");
    }

    public void StartErasing()
    {
        if (erasing) return;
        erasing = true;
        Zastav();
        StartCoroutine(Erase());
    }

    // 100 % -> 75 % -> 35 % -> 0 %
    IEnumerator Erase()
    {
        Debug.Log("Guma maze hrace");
        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();
        float[] keys = { 1f, 0.75f, 0.35f, 0f };
        for (float t = 0f; t < eraseTime; t += Time.deltaTime)
        {
            float k = t / eraseTime * 3f;
            int i = Mathf.Min(2, (int)k);
            SetAlpha(srs, Mathf.Lerp(keys[i], keys[i + 1], k - i));
            yield return null;
        }
        SetAlpha(srs, 0f);

        // korutina musi bezet na kamere, hrac se ted vypne
        Camera cam = Camera.main;
        cam.GetComponent<MonoBehaviour>().StartCoroutine(FadeToWhite(cam));
        gameObject.SetActive(false);
    }

    void SetAlpha(SpriteRenderer[] srs, float a)
    {
        foreach (SpriteRenderer sr in srs)
        {
            Color c = sr.color;
            c.a = a;
            sr.color = c;
        }
    }

    IEnumerator FadeToWhite(Camera cam)
    {
        float h = cam.orthographicSize * 2f;
        float w = h * cam.aspect;

        SpriteRenderer white = Overlay(cam, "Bila_Obrazovka", bila, 1000);
        white.transform.localScale = new Vector3(w + 2f, h + 2f, 1f);
        for (float t = 0f; t < fadeTime; t += Time.deltaTime)
        {
            white.color = new Color(1f, 1f, 1f, t / fadeTime);
            yield return null;
        }
        white.color = Color.white;

        if (epilog == null) yield break;
        SpriteRenderer text = Overlay(cam, "Epilog", epilog, 1001);
        float s = w * epilogWidth / epilog.bounds.size.x;
        text.transform.localScale = new Vector3(s, s, 1f);
        Debug.Log("Kazdy sesit jednou konci...");
        for (float t = 0f; t < fadeTime; t += Time.deltaTime)
        {
            text.color = new Color(1f, 1f, 1f, t / fadeTime);
            yield return null;
        }
        text.color = Color.white;
    }

    SpriteRenderer Overlay(Camera cam, string name, Sprite sprite, int order)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(cam.transform, false);
        go.transform.localPosition = new Vector3(0f, 0f, 1f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        sr.color = new Color(1f, 1f, 1f, 0f);
        return sr;
    }
}
