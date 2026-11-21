using UnityEngine;
using UnityEngine.SceneManagement;

public class druhýleveldveře : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "player") SceneManager.LoadScene("3. level Filip");
    }
}
