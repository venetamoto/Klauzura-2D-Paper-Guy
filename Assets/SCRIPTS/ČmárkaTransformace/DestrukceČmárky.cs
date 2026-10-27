using UnityEngine;

public class DestrukceČmárky : MonoBehaviour
{
    void Update()
    {
        if (proměnaNaMrak.znicCmarku == true)
        {
            Destroy(gameObject);
        }
    }
}
