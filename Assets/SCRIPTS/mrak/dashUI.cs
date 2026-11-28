using UnityEngine;
using System.Collections;

public class dashUI : MonoBehaviour
{
    public static bool spustSlider = false;
    public static bool zmensise = false;
    void Start()
    {
        spustSlider = true;
        zmensise = false;
        transform.localScale = new Vector3(0f, 0.3f, 0f);
        StartCoroutine(CoolDown());
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

    public IEnumerator CoolDown()
    {
        ovládáníMraku.POCKEJ = true;
        spustSlider = true;
        yield return new WaitForSeconds(2.0f);
        zmensise = true;
        spustSlider = false;
        ovládáníMraku.POCKEJ = false;
    }
}
