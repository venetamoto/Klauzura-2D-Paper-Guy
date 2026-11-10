using UnityEngine;
using System.Collections;

public class destrukceZdi2 : MonoBehaviour
{
    bool POCKEJ = false;
    void Update()
    {
        if (proměnaNaMrak.znicZed2 == true)
        {
            StartCoroutine(ČAS());
        }
        if (POCKEJ == true)
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator ČAS()
    {
        yield return new WaitForSeconds(1f);
        POCKEJ = true;
    }
}
