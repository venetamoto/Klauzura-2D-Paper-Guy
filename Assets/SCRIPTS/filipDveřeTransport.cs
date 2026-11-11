using UnityEngine;
using UnityEngine.SceneManagement;

public class filipDveřeTransport : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            SceneManager.LoadScene("Varvara");
        }
    }
}