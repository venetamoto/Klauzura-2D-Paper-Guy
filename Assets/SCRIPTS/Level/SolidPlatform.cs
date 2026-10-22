using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SolidPlatform : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = false;
    }
}
