using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class LevelExit : MonoBehaviour
{
    public string nextScene = "VarvaraKonec";

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerController2D>() != null)
            SceneManager.LoadScene(nextScene);
    }
}
