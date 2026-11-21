using UnityEngine;
using UnityEngine.SceneManagement;

public class třetíleveldveře : MonoBehaviour
{

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "čmárkomrakzákladní_0(Clone)" && destrukčníSkript.ustrihnuto == true) SceneManager.LoadScene("Varvara");
    }
}
