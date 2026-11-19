using UnityEngine;
using UnityEngine.UI;

// hrdina v menu: mrka a dycha
public class MenuHrdina : MonoBehaviour
{
    public Image obr;
    public Sprite stoji;
    public Sprite mrk;
    public float mrkatKazdych = 3f;

    float cas;
    Vector3 zaklad;

    void Awake()
    {
        zaklad = transform.localScale;
    }

    void Update()
    {
        cas += Time.unscaledDeltaTime;
        obr.sprite = cas > mrkatKazdych ? mrk : stoji;
        if (cas > mrkatKazdych + 0.15f) cas = Random.Range(0f, 1.5f);
        transform.localScale = new Vector3(zaklad.x, zaklad.y * (1f + Mathf.Sin(Time.unscaledTime * 2.5f) * 0.015f), zaklad.z);
    }
}
