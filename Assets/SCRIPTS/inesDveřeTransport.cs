using UnityEngine;
using UnityEngine.SceneManagement;

public class inesDveřeTransport : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (EdibleCollectible.bod >= 33)
        {
            SceneManager.LoadScene("Filip");
        }
    }
}
