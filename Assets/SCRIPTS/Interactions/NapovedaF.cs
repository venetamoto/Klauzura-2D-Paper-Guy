using UnityEngine;

public class NapovedaF : MonoBehaviour
{
    [Tooltip("Kolikrat za sekundu se kresba prekresli (roztreseni jako u hrdiny).")]
    public float fps = 8f;

    Sprite[] snimky;
    SpriteRenderer sr;
    float clock;

    // maly kresleny napis "Stiskni F" vedle klice nebo dveri
    public static NapovedaF Vytvor(Vector3 pos)
    {
        GameObject go = new GameObject("Napoveda_F");
        go.transform.position = pos;
        NapovedaF n = go.AddComponent<NapovedaF>();
        go.SetActive(false);
        return n;
    }

    void Awake()
    {
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 50;
        Texture2D[] tex = Resources.LoadAll<Texture2D>("Napoveda");
        snimky = new Sprite[tex.Length];
        for (int i = 0; i < tex.Length; i++)
            snimky[i] = Sprite.Create(tex[i], new Rect(0, 0, tex[i].width, tex[i].height), new Vector2(0.5f, 0.5f), 150f);
    }

    void Update()
    {
        if (snimky.Length == 0) return;
        clock += Time.deltaTime * fps;
        sr.sprite = snimky[(int)clock % snimky.Length];
    }
}
