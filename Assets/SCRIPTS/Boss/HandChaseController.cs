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
        if (erasing)
        {
            wiggle += Time.deltaTime;
            transform.position = start + Vector3.right * Mathf.Sin(wiggle * 20f) * 0.3f;
            return;
        }

        if (!chasing)
        {
            flyClock += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, flyClock / flyInTime);
            transform.position = start + Vector3.left * flyInFrom * (1f - k);
            return;
        }

        speed = Mathf.Min(maxSpeed, speed + zrychleni * Time.deltaTime);
        transform.position += Vector3.right * speed * Time.deltaTime;

        Bounds b = col.bounds;
        foreach (Collider2D h in Physics2D.OverlapBoxAll(b.center, b.size, 0f, groundLayers))
            Smaz(h, b.max.x);

        // na poslednim ostrove uz hrac nemuze nikam
        if (player != null && player.enabled && finalIsland != null && player.IsGrounded)
        {
            float x = player.transform.position.x;
            if (x > finalIsland.bounds.min.x && x < finalIsland.bounds.max.x)
                player.GetComponent<EraseEffect>().Zastav();
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
        if (erasing) return;
        PlayerController2D pc = other.GetComponentInParent<PlayerController2D>();
        if (pc == null) return;

        erasing = true;
        start = transform.position;
        pc.GetComponent<EraseEffect>().StartErasing();
    }
}
