using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class EdibleCollectible : MonoBehaviour
{
    public float waitForTongue = 0.35f;
    public float flyTime = 0.3f;
    bool eaten;
    public static int bod = 0;
    [SerializeField] int bod2 = 0;
    [SerializeField]int zkonzumovano = 0;

    public bool IsEaten => eaten;

    // kolik jidla je v levelu celkem
    static int celkem;

    void Start()
    {
        bod = 0;
        bod2 = 0;
        celkem = FindObjectsByType<EdibleCollectible>(FindObjectsSortMode.None).Length;
    }
    void Update()
    {
        bod2 = bod;
        bod = bod2;
    }

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (eaten || !enabled) return;

        PlayerController2D player = other.GetComponentInParent<PlayerController2D>();
        if(player == null)
        {
            Destroy(gameObject);
            zkonzumovano += 1;
        }

        if (player != null)
        {
            eaten = true;
            player.Eat();
            Debug.Log("Snedeno: " + name);
            StartCoroutine(FlyIntoMouth(player));
        }

    }

    // jazyk vec chnapne a ona zaleti do tlamy na brisku
    IEnumerator FlyIntoMouth(PlayerController2D player)
    {
        yield return new WaitForSeconds(waitForTongue);

        Vector3 start = transform.position;
        Vector3 startScale = transform.localScale;
        for (float t = 0f; t < flyTime; t += Time.deltaTime)
        {
            Vector3 mouth = player.transform.position + new Vector3(0.3f * player.Facing, 0.33f, 0f);
            float k = t / flyTime;
            transform.position = Vector3.Lerp(start, mouth, k);
            transform.localScale = startScale * (1f - k);
            yield return null;
        }
        bod += 1;

        // v levelu Ines: kdyz je snedeno vsechno, jde se na level Varvara
        if (SceneManager.GetActiveScene().name == "Ines" && bod >= celkem)
        {
            Debug.Log("Snedeno vsech " + celkem + ", jde se na Varvaru");
            player.StartCoroutine(NaVarvaru());
        }
        gameObject.SetActive(false);
    }

    static IEnumerator NaVarvaru()
    {
        yield return new WaitForSeconds(0.6f);
        SceneManager.LoadScene("Varvara");
    }
}
