using UnityEngine;

public class bomba : MonoBehaviour
{
    public static bool bombaSebrano = false;

    void Start()
    {
        bombaSebrano = false;
    }
    void Update()
    {
        if (bombaSebrano == true)
        {
            Destroy(gameObject);
        }
    }
}
