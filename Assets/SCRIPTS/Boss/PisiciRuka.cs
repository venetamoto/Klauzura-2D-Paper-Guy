using System.Collections;
using UnityEngine;

// Ruka s tuzkou prileti doprostred obrazovky a radek po radku napise text (obrazek odkryvany maskou)
public static class PisiciRuka
{
    public static IEnumerator Napis(Camera cam, Sprite text, int radky, Sprite tuzka, Sprite bila, float writeTime, float widthFrac)
    {
        float w = cam.orthographicSize * 2f * cam.aspect;
        Vector3 c = cam.transform.position;
        float s = w * widthFrac / text.bounds.size.x;
        float tw = text.bounds.size.x * s, th = text.bounds.size.y * s;
        Vector3 center = new Vector3(c.x, c.y + 1f, 0f);
        float left = center.x - tw / 2f;
        float lineH = th / radky;

        SpriteRenderer sr = new GameObject("Epilog").AddComponent<SpriteRenderer>();
        sr.sprite = text;
        sr.sortingOrder = 50;
        sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        sr.transform.position = center;
        sr.transform.localScale = new Vector3(s, s, 1f);

        SpriteRenderer hand = new GameObject("Ruka_Tuzka").AddComponent<SpriteRenderer>();
        hand.sprite = tuzka;
        hand.sortingOrder = 60;

        Vector3 away = new Vector3(left + tw, center.y + 8f, 0f);
        float firstY = center.y + th / 2f - lineH / 2f;
        Vector3 start = new Vector3(left, firstY, 0f);
        for (float t = 0f; t < 0.8f; t += Time.deltaTime)
        {
            hand.transform.position = Vector3.Lerp(away, start, Mathf.SmoothStep(0f, 1f, t / 0.8f));
            yield return null;
        }

        float lineTime = writeTime / radky;
        for (int r = 0; r < radky; r++)
        {
            float y = center.y + th / 2f - lineH * (r + 0.5f);
            SpriteMask mask = new GameObject("Epilog_Maska_" + r).AddComponent<SpriteMask>();
            mask.sprite = bila;
            for (float t = 0f; t < lineTime; t += Time.deltaTime)
            {
                float k = t / lineTime;
                k += Mathf.Sin(k * 60f) * 0.004f;
                float x = tw * Mathf.Clamp01(k);
                hand.transform.position = new Vector3(left + x, y + Mathf.Sin(t * 28f) * lineH * 0.22f - lineH * 0.05f + (k - 0.5f) * lineH * 0.2f, 0f);
                Maska(mask, left, x, y, lineH);
                yield return null;
            }
            Maska(mask, left, tw, y, lineH);
        }

        Vector3 end = hand.transform.position;
        for (float t = 0f; t < 0.8f; t += Time.deltaTime)
        {
            hand.transform.position = Vector3.Lerp(end, end + new Vector3(3f, 8f, 0f), Mathf.SmoothStep(0f, 1f, t / 0.8f));
            yield return null;
        }
        Object.Destroy(hand.gameObject);
    }

    static void Maska(SpriteMask mask, float left, float width, float y, float h)
    {
        mask.transform.position = new Vector3(left + width / 2f, y, 0f);
        mask.transform.localScale = new Vector3(Mathf.Max(0.001f, width), h * 1.05f, 1f);
    }
}
