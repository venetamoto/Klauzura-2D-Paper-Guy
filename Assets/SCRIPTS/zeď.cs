using UnityEngine;

public class zeď : MonoBehaviour
{
    void Update()
    {
        if (proměnaNaMrak.znicZed == true)
        {
            Destroy(gameObject);
        }
    }
}
