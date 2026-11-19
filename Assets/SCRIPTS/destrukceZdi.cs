using UnityEngine;

public class destrukceZdi : MonoBehaviour
{
    void Update()
    {
        if (proměnaNaMrak.znicZed == true)
        {
            Destroy(gameObject);
        }
        if (proměnaNaMrak.znicZed2 == true)
        {
            Destroy(gameObject);
        }
    }
}
