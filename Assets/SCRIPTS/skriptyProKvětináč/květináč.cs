using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class květináč : MonoBehaviour
{
    public static bool seminko = false;
    [SerializeField]public bool zasazeno = false;
    [SerializeField]public bool vyrostlo = false;
    [SerializeField]public int prijateJidlo = 10;
    public static bool kytka3prefab = false;
    public static bool kytka4prefab = false;
    public GameObject kytka1prefab;
    public GameObject kytka2prefab;
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
        prijateJidlo = 10;
    }

    void Update()
    {
        vzdalenost = Vector2.Distance(transform.position, player.position);
        if (vzdalenost <= okruh && gameObject.name != "květináč" && Input.GetKeyDown(KeyCode.F) && EdibleCollectible.zkonzumovano >= 1)
        {
            EdibleCollectible.zkonzumovano -= 1;
            prijateJidlo += 1;
            Debug.Log("přijto jídla: " + prijateJidlo);
            Debug.Log("jídlo: " + EdibleCollectible.zkonzumovano);
        }
        if (prijateJidlo == 11 && zasazeno == true && vyrostlo == false)
        {
            vyrostlo = true;
            kytka1prefab.SetActive(false);
            kytka3prefab = false;
            kytka4prefab = true;
            Debug.Log("kape!");
            Instantiate(kytka2prefab, transform.position, transform.rotation);
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