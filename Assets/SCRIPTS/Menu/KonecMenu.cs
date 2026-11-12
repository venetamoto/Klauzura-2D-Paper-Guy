using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Menu po konci hry: hra znovu, ovladani, nastaveni, autori
public class KonecMenu : MonoBehaviour
{
    public GameObject hlavni;
    public GameObject ovladani;
    public GameObject nastaveni;
    public GameObject autori;

    [Header("Nastaveni")]
    public Image fullscreenObr;
    public Sprite fullscreenOn;
    public Sprite fullscreenOff;
    public Image zvukObr;
    public Sprite zvukOn;
    public Sprite zvukOff;

    bool full;

    void Start()
    {
        full = Screen.fullScreen;
        AudioListener.volume = PlayerPrefs.GetInt("zvuk", 1);
        Obnovit();
        Ukaz(hlavni);
    }

    void Update()
    {
        // Esc = zpet do hlavniho menu
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && !hlavni.activeSelf) Zpet();
    }

    public void ZnovuHrat()
    {
        Debug.Log("Hra znovu od zacatku");
        SceneManager.LoadScene(0);
    }

    public void Ukaz(GameObject panel)
    {
        hlavni.SetActive(panel == hlavni);
        ovladani.SetActive(panel == ovladani);
        nastaveni.SetActive(panel == nastaveni);
        autori.SetActive(panel == autori);

        // at jde menu ovladat i sipkami a Enterem
        Button prvni = panel.GetComponentInChildren<Button>();
        if (prvni != null && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(prvni.gameObject);
    }

    public void Zpet()
    {
        Ukaz(hlavni);
    }

    public void PrepnoutFullscreen()
    {
        full = !full;
        Screen.fullScreen = full;
        Obnovit();
    }

    public void PrepnoutZvuk()
    {
        int z = PlayerPrefs.GetInt("zvuk", 1) == 1 ? 0 : 1;
        PlayerPrefs.SetInt("zvuk", z);
        AudioListener.volume = z;
        Obnovit();
    }

    void Obnovit()
    {
        fullscreenObr.sprite = full ? fullscreenOn : fullscreenOff;
        zvukObr.sprite = PlayerPrefs.GetInt("zvuk", 1) == 1 ? zvukOn : zvukOff;
    }
}
