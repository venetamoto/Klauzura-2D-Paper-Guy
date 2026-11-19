using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// Startovni menu: Start = hrdina uteče za okraj listu a zacne prvni level, Quit = konec
public class StartMenu : MonoBehaviour
{
    public PlayerController2D hrdina;
    public PlayerKresba kresba;
    [Tooltip("Prava stena hriste - pri startu zmizi, at hrdina muze utect.")]
    public Collider2D pravaStena;
    public float runSpeed = 11f;
    public float zaKolik = 1.3f;

    bool jde;
    Rigidbody2D rb;

    public void ZacitHru()
    {
        if (jde) return;
        jde = true;
        Debug.Log("Start hry");
        hrdina.enabled = false;
        rb = hrdina.GetComponent<Rigidbody2D>();
        kresba.utika = true;
        if (pravaStena != null) pravaStena.enabled = false;
        Invoke(nameof(Nahrat), zaKolik);
    }

    // tlacitko kliknute mysi nesmi zustat vybrane - mezernik (skok) by ho zmackl znovu
    void Update()
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    void FixedUpdate()
    {
        if (jde) rb.linearVelocity = new Vector2(runSpeed, rb.linearVelocity.y);
    }

    void Nahrat()
    {
        // 0 = tohle menu, 1 = prvni level
        SceneManager.LoadScene(1);
    }

    public void Odejit()
    {
        Debug.Log("Konec hry");
        Application.Quit();
    }
}
