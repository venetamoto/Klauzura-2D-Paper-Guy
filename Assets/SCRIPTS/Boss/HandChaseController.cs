using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class HandChaseController : MonoBehaviour
{
    public float speed = 4.5f;
    [Tooltip("Guma postupne zrychluje az na maxSpeed.")]
    public float zrychleni = 0f;
    public float maxSpeed = 6f;
    public LayerMask groundLayers;
    public PlayerController2D player;
    [Tooltip("Na tomhle ostrove hrac ztrati ovladani a ceka na gumu.")]
    public Collider2D finalIsland;
    [Tooltip("Odkud guma prileti, nez se zastavi na startu.")]
    public float flyInFrom = 5f;
    public float flyInTime = 0.6f;

    BoxCollider2D col;
    Vector3 start;
    float flyClock;
    bool chasing;
    bool erasing;
    bool hotovo;
    bool odlet;
    [Tooltip("Dobry konec: guma ceka, nez hrac dokresli stenu.")]
    public bool pauza;
    [Tooltip("Nakreslena stena - guma do ni narazi a odleti.")]
    public Collider2D stena;
    [Tooltip("Jak dlouho guma maze hrace (stejne jako EraseEffect.eraseTime).")]
    public float eraseTime = 1.5f;
    float wiggle;

    void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
        start = transform.position;
    }

    // guma prileti zleva a ceka
    void OnEnable()
    {
        flyClock = 0f;
        transform.position = start + Vector3.left * flyInFrom;
    }

    public void StartChase()
    {
        chasing = true;
        Debug.Log("Guma jede");
    }

    void Update()
    {
        if (odlet) return;

        // guma drhne po hraci, i kdyz uteka; pak jede dal a smaze i ostrov
        if (erasing)
        {
            wiggle += Time.deltaTime;
            Vector3 p = player.transform.position;
            transform.position = new Vector3(p.x - 0.2f + Mathf.Sin(wiggle * 20f) * 0.35f, p.y + 0.4f + Mathf.Abs(Mathf.Cos(wiggle * 20f)) * 0.15f, 0f);
            if (wiggle >= eraseTime)
            {
                erasing = false;
                hotovo = true;
            }
            return;
        }

        if (!chasing)
        {
            flyClock += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, flyClock / flyInTime);
            transform.position = start + Vector3.left * flyInFrom * (1f - k);
            return;
        }

        if (pauza) return;

        if (stena != null && col.bounds.max.x >= stena.bounds.min.x - 0.05f)
        {
            StartCoroutine(Odraz());
            return;
        }

        speed = Mathf.Min(maxSpeed, speed + zrychleni * Time.deltaTime);
        transform.position += Vector3.right * speed * Time.deltaTime;

        Bounds b = col.bounds;
        foreach (Collider2D h in Physics2D.OverlapBoxAll(b.center, b.size, 0f, groundLayers))
            Smaz(h, b.max.x);

        // na poslednim ostrove uz hrac nemuze nikam
        if (player != null && player.gameObject.activeInHierarchy && finalIsland != null && player.IsGrounded)
        {
            float x = player.transform.position.x;
            if (x > finalIsland.bounds.min.x && x < finalIsland.bounds.max.x)
                player.GetComponent<EraseEffect>().Panika();
        }
    }

    // guma maze plosinu odzadu, sikmou smaze celou
    void Smaz(Collider2D h, float edge)
    {
        Transform p = h.transform;
        if (Mathf.Abs(Mathf.DeltaAngle(p.eulerAngles.z, 0f)) > 1f)
        {
            Destroy(h.gameObject);
            return;
        }

        float right = h.bounds.max.x;
        float w = right - edge;
        if (w < 0.15f)
        {
            Destroy(h.gameObject);
            return;
        }
        p.position = new Vector3(edge + w / 2f, p.position.y, p.position.z);
        p.localScale = new Vector3(w, p.localScale.y, p.localScale.z);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (erasing || hotovo || odlet) return;
        PlayerController2D pc = other.GetComponentInParent<PlayerController2D>();
        if (pc == null) return;

        erasing = true;
        pc.GetComponent<EraseEffect>().StartErasing();
    }

    // guma narazi do steny, odskoci a odleti pryc
    System.Collections.IEnumerator Odraz()
    {
        odlet = true;
        Debug.Log("Guma narazila do steny");
        Vector3 p = transform.position;
        for (float t = 0f; t < 0.3f; t += Time.deltaTime)
        {
            float k = t / 0.3f;
            transform.position = p + new Vector3(-1.5f * Mathf.Sin(k * Mathf.PI * 0.5f), 0.4f * Mathf.Sin(k * Mathf.PI), 0f);
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 60f) * 15f);
            yield return null;
        }
        p = transform.position;
        for (float t = 0f; t < 1.2f; t += Time.deltaTime)
        {
            float k = t / 1.2f;
            transform.position = p + new Vector3(-6f * k, 12f * k * k, 0f);
            transform.rotation = Quaternion.Euler(0f, 0f, 540f * k);
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
