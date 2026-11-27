using UnityEngine;

public class DestrukceMraku : MonoBehaviour
{
    void Update()
    {
        if (proměnaNaMrak.znicMrak == true)
        {
            Destroy(gameObject);
        }
    }
}
