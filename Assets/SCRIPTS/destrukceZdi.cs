using UnityEngine;

public class destrukceZdi : MonoBehaviour
{
    void Update()
    {
        if (proměnaNaMrak.znicZed == true)
        {
            Destroy(gameObject);
        }
    }
}
