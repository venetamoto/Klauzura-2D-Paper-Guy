using System.Collections;
using UnityEngine;

// Varvara2: na konci prileti ruka s gumou, smaze hrdinovi chocholku, on se lekne a utece dal
[RequireComponent(typeof(BoxCollider2D))]
public class SmazanaChocholka : MonoBehaviour
{
    public PlayerController2D player;
    public PlayerKresba kresba;
    public Transform hand;
    [Tooltip("Vykricnik nad hlavou, kdyz se lekne.")]
    public SpriteRenderer lekZnak;

    [Header("Kresby bez chocholky")]
    public Sprite stoji;
    public Sprite stojiMrk;
    public Sprite chuze1;
    public Sprite chuze2;
    public Sprite skok;
    public Sprite pad;

    [Header("Casy")]
    public float ticho = 0.8f;
    public float flyTime = 1.1f;
    public float eraseTime = 1.4f;
    public float lekTime = 0.8f;
    public float runSpeed = 11f;

    bool started;
    bool run;
    Rigidbody2D rb;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        hand.gameObject.SetActive(false);
        if (lekZnak != null) lekZnak.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (started || other.GetComponentInParent<PlayerController2D>() == null) return;
        started = true;
        StartCoroutine(Scena());
    }

    IEnumerator Scena()
    {
        // hrdina se zastavi a chvili je ticho
        player.enabled = false;
        rb = player.GetComponent<Rigidbody2D>();
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        yield return new WaitForSeconds(ticho);

        Bounds b = kresba.kresba.bounds;
        Vector3 top = new Vector3(b.center.x, b.max.y - 0.2f, 0f);
        Vector3 away = top + new Vector3(4f, 8f, 0f);

        kresba.krci = true;
        top.y -= 0.3f;
        hand.gameObject.SetActive(true);
        yield return Fly(away, top);

        bool swapped = false;
        for (float t = 0f; t < eraseTime; t += Time.deltaTime)
        {
            hand.position = top + new Vector3(Mathf.Sin(t * 16f) * 0.35f, Mathf.Abs(Mathf.Cos(t * 16f)) * 0.08f, 0f);
            if (!swapped && t > eraseTime * 0.5f)
            {
                swapped = true;
                Smaz();
            }
            yield return null;
        }

        kresba.krci = false;
        yield return Fly(hand.position, away);
        hand.gameObject.SetActive(false);

        // lek
        Debug.Log("Hrdina se lekl");
        kresba.lek = true;
        if (lekZnak != null)
        {
            lekZnak.enabled = true;
            lekZnak.transform.position = new Vector3(b.center.x + 0.6f, b.max.y + 0.4f, 0f);
        }
        yield return new WaitForSeconds(lekTime);
        kresba.lek = false;
        if (lekZnak != null) lekZnak.enabled = false;

        // a pryc
        kresba.utika = true;
        run = true;
    }

    void Smaz()
    {
        Debug.Log("Chocholka smazana");
        kresba.stoji = stoji;
        kresba.stojiMrk = stojiMrk;
        kresba.chuze = new Sprite[] { chuze1, chuze2 };
        kresba.skok = skok;
        kresba.pad = pad;
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

    void FixedUpdate()
    {
        if (run) rb.linearVelocity = new Vector2(runSpeed, rb.linearVelocity.y);
    }
}
