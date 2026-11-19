using UnityEngine;
using UnityEngine.EventSystems;

// nakreslene tlacitko: kdyz je na nem mys (nebo je vybrane sipkami), zvetsi se a trochu se klati
public class TlacitkoKresba : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public float zvetseni = 1.1f;

    bool nad;
    bool vybrane;
    Vector3 zaklad;

    void Awake()
    {
        zaklad = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData e) { nad = true; }
    public void OnPointerExit(PointerEventData e) { nad = false; }
    public void OnSelect(BaseEventData e) { vybrane = true; }
    public void OnDeselect(BaseEventData e) { vybrane = false; }

    void Update()
    {
        bool aktivni = nad || vybrane;
        transform.localScale = Vector3.Lerp(transform.localScale, zaklad * (aktivni ? zvetseni : 1f), Time.unscaledDeltaTime * 12f);
        transform.localRotation = Quaternion.Euler(0f, 0f, aktivni ? Mathf.Sin(Time.unscaledTime * 14f) * 2.5f : 0f);
    }

    void OnDisable()
    {
        nad = false;
        vybrane = false;
        transform.localScale = zaklad;
        transform.localRotation = Quaternion.identity;
    }
}
