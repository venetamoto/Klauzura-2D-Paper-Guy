using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class květináč : MonoBehaviour
{
    public static bool seminko = false;
    public static bool zasazeno = false;
    public static bool vyrostlo = false;
    public static int prijateJidlo = 0;
    public static bool kytka3prefab = false;
    public static bool kytka4prefab = false;
    public GameObject kytka1prefab;
    public GameObject kytka2prefab;
    public GameObject kapkopadprefab;
    public Transform player;
    public float okruh = 1.2f;
    float vzdalenost = 0f;
    void Awake()
    {
        seminko = false;
        zasazeno = false;
        vyrostlo = false;
        kytka3prefab = false;
        kytka4prefab = false;
        prijateJidlo = 0;
    }

    void Update()
    {
        GameObject SMAZTO = GameObject.Find("kytka1(Clone)");
        vzdalenost = Vector2.Distance(transform.position, player.position);
        if (vzdalenost <= okruh && gameObject.name != "květináč" && Input.GetKeyDown(KeyCode.F) && EdibleCollectible.zkonzumovano >= 1)
        {
            EdibleCollectible.zkonzumovano -= 1;
            prijateJidlo += 1;
            Debug.Log("přijato jídla: " + prijateJidlo);
            Debug.Log("jídlo: " + EdibleCollectible.zkonzumovano);
        }
        if (prijateJidlo >= 11 && zasazeno == true && vyrostlo == false)
        {
            Destroy(SMAZTO);
            kytka3prefab = false;
            kytka4prefab = true;
            Debug.Log("kape!");
            Instantiate(kapkopadprefab, new Vector3(14.89f, -2.08f, 0f), Quaternion.Euler(0f, 0f, -77.917f));
            Instantiate(kytka2prefab, transform.position, transform.rotation);
            vyrostlo = true;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "player" && seminko == true && zasazeno == false)
        {
            zasazeno = true;
            Instantiate(kytka1prefab, transform.position, transform.rotation);
            kytka3prefab = true;
            Debug.Log("zasazeno!");
        }
    }
}