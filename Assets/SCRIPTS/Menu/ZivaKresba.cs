using UnityEngine;

// kresba v menu se jemne "trese" jako rucne kreslena animace
public class ZivaKresba : MonoBehaviour
{
    public float uhel = 0.7f;
    public float posun = 2f;
    public float kazdych = 0.12f;

    RectTransform rt;
    Vector2 zaklad;
    float cas;

    void Awake()
    {
        rt = (RectTransform)transform;
        zaklad = rt.anchoredPosition;
    }

    void Update()
    {
        cas += Time.unscaledDeltaTime;
        if (cas < kazdych) return;
        cas = 0f;
        rt.anchoredPosition = zaklad + new Vector2(Random.Range(-posun, posun), Random.Range(-posun, posun));
        rt.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-uhel, uhel));
    }
}
