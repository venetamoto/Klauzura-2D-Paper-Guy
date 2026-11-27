using UnityEngine;

public class dashUI : MonoBehaviour
{
    public static bool spustSlider = false;
    public static bool zmensise = false;
    void Start()
    {
        transform.localScale = new Vector3(5.312984f, 0.3f, 0f);
        spustSlider = false;
        zmensise = false;
    }

    void Update()
    {
        if (spustSlider == true)
        {
            Reset();
            transform.localScale = new Vector3(transform.localScale.x + 0.007f, 0.3f, 0f);
        }
        if (spustSlider == false) transform.localScale = new Vector3(5.312984f, 0.3f, 0f);
    }
    
    public void Reset()
    {
        if (zmensise == true)
        {
            transform.localScale = new Vector3(0f, 0.3f, 0f);
            zmensise = false;
        }
    }
}
