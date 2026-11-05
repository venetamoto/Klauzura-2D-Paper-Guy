using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class LevelExit : MonoBehaviour
{
    public string nextScene = "VarvaraKonec";

    [Header("Nebo az se sni tohle jidlo")]
    public EdibleCollectible afterEaten;
    public float delay = 1.3f;

    bool loading;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerController2D>() != null) Load();
    }

    // posledni jidlo snedeno -> chvili pockat (animace jidla) a dalsi level
    void Update()
    {
        if (loading || afterEaten == null || !afterEaten.IsEaten) return;
        loading = true;
        Invoke(nameof(Load), delay);
    }

    void Load()
    {
        loading = true;
        SceneManager.LoadScene(nextScene);
    }
}
