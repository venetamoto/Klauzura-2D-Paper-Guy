using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Pauza v kazdem levelu (sama se vytvori, scenu neni treba upravovat):
// Esc / gamepad Start -> hra stoji, obrazovka ztmavne a uprostred napis "Paused".
public class Pauza : MonoBehaviour
{
    public static bool pauznuto;

    GameObject vrstva;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Spustit()
    {
        // bez reloadu domeny se Spustit vola pri kazdem Play - nejdriv odhlasit, at nejsou dve pauzy
        SceneManager.sceneLoaded -= NaScenu;
        SceneManager.sceneLoaded += NaScenu;
        Vytvor(SceneManager.GetActiveScene());
    }

    static void NaScenu(Scene s, LoadSceneMode m)
    {
        Vytvor(s);
    }

    static void Vytvor(Scene s)
    {
        Time.timeScale = 1f;
        pauznuto = false;
        AudioListener.pause = false;
        if (s.name == "StartMenu" || s.name == "KonecMenu") return;
        GameObject go = new GameObject("Pauza");
        SceneManager.MoveGameObjectToScene(go, s);
        go.AddComponent<Pauza>();
    }

    void Start()
    {
        GameObject c = new GameObject("Pauza_Canvas");
        c.transform.SetParent(transform, false);
        Canvas canvas = c.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        CanvasScaler cs = c.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920f, 1080f);
        cs.matchWidthOrHeight = 0.5f;
        vrstva = c;

        // tmavsi pozadi pres celou obrazovku
        Image tma = new GameObject("Tma").AddComponent<Image>();
        tma.transform.SetParent(c.transform, false);
        tma.color = new Color(0.05f, 0.05f, 0.08f, 0.45f);
        tma.rectTransform.anchorMin = Vector2.zero;
        tma.rectTransform.anchorMax = Vector2.one;
        tma.rectTransform.offsetMin = Vector2.zero;
        tma.rectTransform.offsetMax = Vector2.zero;

        // napis
        Texture2D tex = Resources.Load<Texture2D>("Pauza/Pauza_Titul");
        if (tex != null)
        {
            Image napis = new GameObject("Paused").AddComponent<Image>();
            napis.transform.SetParent(c.transform, false);
            napis.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            napis.rectTransform.sizeDelta = new Vector2(tex.width, tex.height) * 1.3f;
            napis.gameObject.AddComponent<ZivaKresba>();
        }

        c.SetActive(false);
    }

    void Update()
    {
        bool stisk = (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                     || (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);
        if (!stisk || vrstva == null) return;

        pauznuto = !pauznuto;
        Time.timeScale = pauznuto ? 0f : 1f;
        AudioListener.pause = pauznuto;
        vrstva.SetActive(pauznuto);
        Debug.Log(pauznuto ? "Pauza" : "Hra pokracuje");
    }
}
