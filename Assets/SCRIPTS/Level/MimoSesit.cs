using UnityEngine;
using UnityEngine.SceneManagement;

public class MimoSesit : MonoBehaviour
{
    [Tooltip("Obdelnik sesitu (Camera_Bounds).")]
    public BoxCollider2D sesit;
    [Tooltip("Jak daleko za okrajem uz je hrac pryc.")]
    public float okraj = 0.5f;
    public float delay = 0.6f;

    bool pryc;

    void Update()
    {
        if (pryc || sesit == null) return;

        Bounds b = sesit.bounds;
        Vector3 p = transform.position;
        if (p.x > b.min.x - okraj && p.x < b.max.x + okraj && p.y > b.min.y - okraj) return;

        // hrac odesel ze sesitu -> zmizi a level jede znovu
        pryc = true;
        Debug.Log("Hrac odesel ze sesitu, level znovu od zacatku");
        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>()) sr.enabled = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;
        Invoke(nameof(Znovu), delay);
    }

    void Znovu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
