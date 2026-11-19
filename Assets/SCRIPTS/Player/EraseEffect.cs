using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EraseEffect : MonoBehaviour
{
    public float eraseTime = 1.5f;
    [Tooltip("Bily ctverec - maska, za kterou se odkryva psany text.")]
    public Sprite bila;
    [Tooltip("Napis \"Kazdy sesit jednou konci...\" krive psany tuzkou.")]
    public Sprite epilog;
    public float epilogWidth = 0.6f;
    [Tooltip("Vykricnik nad hlavou, kdyz je hrac v panice.")]
    public Sprite lek;
    [Tooltip("Ruka s tuzkou, ktera napise epilog.")]
    public Sprite tuzka;

    [Header("Konec")]
    [Tooltip("Jak dlouho je videt prazdny sesit, nez prijde ruka psat.")]
    public float prazdno = 2.5f;
    public float writeTime = 3.5f;
    [Tooltip("Za kolik sekund po dopsani napisu se ukaze menu konce hry.")]
    public float menuPo = 3f;

    [Tooltip("Dobry konec: hrac se zachranil, guma uz ho nesmaze.")]
    public bool zachranen;

    bool stopped;
    bool erasing;
    public bool IsErasing => erasing;
    PlayerKresba kresba;
    SpriteRenderer znak;

    // na poslednim ostrove: hrac porad ovlada postavu, ale kdyz stoji, panikari
    public void Panika()
    {
        if (stopped || zachranen) return;
        stopped = true;
        Debug.Log("Hrac je v pasti");

        kresba = GetComponent<PlayerKresba>();
        if (kresba != null) kresba.panika = true;
        if (lek != null)
        {
            znak = new GameObject("Panika_Znak").AddComponent<SpriteRenderer>();
            znak.sprite = lek;
            znak.sortingOrder = 40;
            StartCoroutine(Znak());
        }
    }

    // vykricnik nad hlavou poskakuje a blika
    IEnumerator Znak()
    {
        while (znak != null)
        {
            float s = 0.55f + Mathf.Abs(Mathf.Sin(Time.time * 9f)) * 0.15f;
            znak.transform.position = transform.position + new Vector3(0.7f, 2.4f + Mathf.Sin(Time.time * 12f) * 0.08f, 0f);
            znak.transform.localScale = new Vector3(s, s, 1f);
            znak.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 20f) * 10f);
            yield return null;
        }
    }

    // dobry konec: konec paniky
    public void Uklidnit()
    {
        if (kresba == null) kresba = GetComponent<PlayerKresba>();
        if (kresba != null) kresba.panika = false;
        if (znak != null) Destroy(znak.gameObject);
    }

    public void StartErasing()
    {
        if (erasing || zachranen) return;
        erasing = true;
        Panika();
        // ted uz se nesmi restartovat level, kdyz spadne
        MimoSesit ms = GetComponent<MimoSesit>();
        if (ms != null) ms.enabled = false;
        StartCoroutine(Erase());
    }

    // 100 % -> 75 % -> 35 % -> 0 %, hrdina se trese a lita z nej drobky gumy
    IEnumerator Erase()
    {
        Debug.Log("Guma maze hrace");
        if (kresba != null)
        {
            kresba.panika = false;
            kresba.mazany = true;
        }

        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();
        float[] keys = { 1f, 0.75f, 0.35f, 0f };
        float crumb = 0f;
        for (float t = 0f; t < eraseTime; t += Time.deltaTime)
        {
            float k = t / eraseTime * 3f;
            int i = Mathf.Min(2, (int)k);
            float a = Mathf.Lerp(keys[i], keys[i + 1], k - i);
            SetAlpha(srs, a);
            if (znak != null) znak.color = new Color(1f, 1f, 1f, a);

            crumb += Time.deltaTime;
            if (crumb > 0.05f)
            {
                crumb = 0f;
                Drobek();
            }
            yield return null;
        }
        SetAlpha(srs, 0f);
        if (znak != null) Destroy(znak.gameObject);

        // korutina musi bezet na kamere, hrac se ted vypne
        Camera cam = Camera.main;
        cam.GetComponent<MonoBehaviour>().StartCoroutine(Konec(cam));
        gameObject.SetActive(false);
    }

    // drobek gumy odleti a spadne
    void Drobek()
    {
        if (bila == null) return;
        GameObject go = new GameObject("Drobek");
        go.layer = 2;
        go.transform.position = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0f, 1.8f), 0f);
        go.transform.localScale = Vector3.one * Random.Range(0.07f, 0.16f);
        go.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 90f));
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = bila;
        sr.color = new Color(0.55f, 0.5f, 0.52f);
        sr.sortingOrder = 25;
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1.5f;
        rb.linearVelocity = new Vector2(Random.Range(-3f, 3f), Random.Range(1f, 4f));
        rb.angularVelocity = Random.Range(-400f, 400f);
        Destroy(go, 1.5f);
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

    // prazdny sesit, pak ruka krive napise epilog
    IEnumerator Konec(Camera cam)
    {
        yield return new WaitForSeconds(prazdno);
        if (epilog == null) yield break;
        Debug.Log("Ruka pise: Kazdy sesit jednou konci...");
        yield return PisiciRuka.Napis(cam, epilog, 1, tuzka, bila, writeTime, epilogWidth);
        yield return new WaitForSeconds(menuPo);
        SceneManager.LoadScene("KonecMenu");
    }
}
