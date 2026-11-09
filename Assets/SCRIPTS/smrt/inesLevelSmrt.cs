using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class inesLevelSmrt : MonoBehaviour
{
    bool cekani = false;
    public static bool padni = false;
    void Start()
    {
        cekani = false;
        padni = false;
    }
    void Update()
    {
        if (cekani == true)
        {
            SceneManager.LoadScene("Ines");
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            padni = true;
            StartCoroutine(Počkej());
        }
    }
    public IEnumerator Počkej()
    {
        yield return new WaitForSeconds(1.0f);
        cekani = true;
    }
}
