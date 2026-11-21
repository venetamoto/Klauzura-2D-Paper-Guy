using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Smrt : MonoBehaviour
{
    public GameObject mrtvaCmarkaPrefab;
    bool cekani = false;
    public static bool padni = false;
    string scena;
    void Start()
    {
        cekani = false;
        padni = false;
    }
    void Update()
    {
        if (cekani == true)
        {
            scena = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(scena);
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            padni = true;
            GameObject SMAZTO = GameObject.Find("čmárkomrakzákladní_0(Clone)");
            Destroy(SMAZTO);
            Instantiate(mrtvaCmarkaPrefab, transform.position, transform.rotation);
            StartCoroutine(Počkej());
        }
    }
    public IEnumerator Počkej()
    {
        yield return new WaitForSeconds(1.0f);
        cekani = true;
    }
}
