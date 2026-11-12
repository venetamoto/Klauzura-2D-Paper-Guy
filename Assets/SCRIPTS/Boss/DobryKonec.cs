using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Dobry konec: kdo cestou snedl tajny modry ctverecek, najde na ostrove tuzku.
// Klavesou F ji vezme, dokresli si chocholku a pred gumu nakresli stenu. Guma narazi a odleti.
public class DobryKonec : MonoBehaviour
{
    public EdibleCollectible tajny;
    public PlayerController2D player;
    public PlayerKresba kresba;
    public HandChaseController guma;
    [Tooltip("Posledni ostrov - tuzka se objevi, az na nem hrac stoji.")]
    public Collider2D ostrov;
    [Tooltip("Tuzka lezici na ostrove (na zacatku vypnuta).")]
    public SpriteRenderer tuzka;
    public float dosah = 1.2f;

    [Header("Kresby s chocholkou (vrati se)")]
    public Sprite stoji;
    public Sprite stojiMrk;
    public Sprite chuze1;
    public Sprite chuze2;
    public Sprite skok;
    public Sprite pad;

    [Header("Stena a napis")]
    public Sprite bila;
    public Sprite rukaTuzka;
    public Sprite napis;
    public float stenaVyska = 7f;
    public float writeTime = 4.5f;
    public float menuPo = 3f;

    bool ukazana;
    bool vzata;
    NapovedaF napoveda;
    Collider2D playerCol;
    Collider2D tuzkaCol;

    void Awake()
    {
        tuzka.gameObject.SetActive(false);
    }

    void Update()
    {
        if (vzata || player == null || !player.gameObject.activeInHierarchy) return;
        if (tajny == null || !tajny.IsEaten) return;
        if (playerCol == null) playerCol = player.GetComponent<Collider2D>();

        EraseEffect ee = player.GetComponent<EraseEffect>();
        if (ee.IsErasing) return;

        // tuzka se objevi, kdyz hrac doskoci na ostrov
        if (!ukazana)
        {
            float x = player.transform.position.x;
            if (!player.IsGrounded || x < ostrov.bounds.min.x || x > ostrov.bounds.max.x) return;
            ukazana = true;
            tuzka.gameObject.SetActive(true);
            tuzkaCol = tuzka.GetComponent<Collider2D>();
            napoveda = NapovedaF.Vytvor(tuzka.transform.position + Vector3.up * 1.2f);
            Debug.Log("Na ostrove lezi tuzka");
        }

        float d = Physics2D.Distance(tuzkaCol, playerCol).distance;
        napoveda.gameObject.SetActive(d < 3f);
        if (d > dosah || !Klic.StisklF()) return;

        vzata = true;
        napoveda.gameObject.SetActive(false);
        StartCoroutine(Dokreslit());
    }

    IEnumerator Dokreslit()
    {
        Debug.Log("Hrdina vzal tuzku");
        EraseEffect ee = player.GetComponent<EraseEffect>();
        ee.zachranen = true;
        ee.Uklidnit();
        guma.pauza = true;
        player.enabled = false;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        tuzkaCol.enabled = false;

        // tuzka vyleti nad hlavu
        Transform tz = tuzka.transform;
        tz.localScale = tz.localScale * 0.6f;
        Bounds b = kresba.kresba.bounds;
        Vector3 hlava = new Vector3(b.center.x, b.max.y + 0.1f, 0f);
        yield return Let(tz, tz.position, hlava, 0.4f, -60f);

        // dokresli chocholku
        bool hotovo = false;
        for (float t = 0f; t < 1.2f; t += Time.deltaTime)
        {
            tz.position = hlava + new Vector3(Mathf.Sin(t * 22f) * 0.25f, Mathf.Abs(Mathf.Sin(t * 9f)) * 0.5f, 0f);
            if (!hotovo && t > 0.6f)
            {
                hotovo = true;
                kresba.stoji = stoji;
                kresba.stojiMrk = stojiMrk;
                kresba.chuze = new Sprite[] { chuze1, chuze2 };
                kresba.skok = skok;
                kresba.pad = pad;
                Debug.Log("Chocholka je zpatky");
            }
            yield return null;
        }

        // nakresli stenu mezi sebe a gumu
        float gx = guma.GetComponent<Collider2D>().bounds.max.x;
        float px = player.transform.position.x;
        float x = Mathf.Max(ostrov.bounds.min.x - 0.4f, gx + 0.3f);
        if (x > px - 0.7f) x = (gx + px) / 2f;
        float y0 = ostrov.bounds.max.y - 2f;

        GameObject stena = new GameObject("Nakreslena_Stena");
        stena.transform.position = new Vector3(x, y0, 0f);
        SpriteRenderer ss = stena.AddComponent<SpriteRenderer>();
        ss.sprite = bila;
        ss.color = new Color(0.14f, 0.12f, 0.12f);
        ss.sortingOrder = 5;
        BoxCollider2D sc = stena.AddComponent<BoxCollider2D>();
        sc.size = Vector2.one;

        yield return Let(tz, tz.position, new Vector3(x, y0, 0f), 0.35f, 0f);
        for (float t = 0f; t < 0.9f; t += Time.deltaTime)
        {
            float h = stenaVyska * t / 0.9f;
            stena.transform.position = new Vector3(x + Random.Range(-0.02f, 0.02f), y0 + h / 2f, 0f);
            stena.transform.localScale = new Vector3(0.35f, Mathf.Max(0.01f, h), 1f);
            tz.position = new Vector3(x + Mathf.Sin(t * 30f) * 0.12f, y0 + h, 0f);
            yield return null;
        }
        stena.transform.position = new Vector3(x, y0 + stenaVyska / 2f, 0f);
        stena.transform.localScale = new Vector3(0.35f, stenaVyska, 1f);
        tz.gameObject.SetActive(false);

        // guma pokracuje a narazi
        guma.stena = sc;
        guma.pauza = false;
        player.enabled = true;
        MimoSesit ms = player.GetComponent<MimoSesit>();
        if (ms != null) ms.enabled = false;

        yield return new WaitForSeconds(2.5f);
        Debug.Log("Ruka pise: Nekdy si svuj pribeh musis dokreslit sam...");
        Camera cam = Camera.main;
        yield return PisiciRuka.Napis(cam, napis, 2, rukaTuzka, bila, writeTime, 0.55f);
        yield return new WaitForSeconds(menuPo);
        SceneManager.LoadScene("KonecHry");
    }

    IEnumerator Let(Transform tr, Vector3 from, Vector3 to, float time, float angle)
    {
        Quaternion r0 = tr.rotation, r1 = Quaternion.Euler(0f, 0f, angle);
        for (float t = 0f; t < time; t += Time.deltaTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / time);
            tr.position = Vector3.Lerp(from, to, k);
            tr.rotation = Quaternion.Slerp(r0, r1, k);
            yield return null;
        }
        tr.position = to;
        tr.rotation = r1;
    }
}
