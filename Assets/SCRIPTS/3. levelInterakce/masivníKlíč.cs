using UnityEngine;

public class masivníKlíč : MonoBehaviour
{
    [SerializeField]float speedx = 1f;
    [SerializeField]float speedy = -5f;

    void Update()
    {
        if (destrukčníSkript.ustrihnuto == true)
        {
            transform.position = new Vector3(transform.position.x+Time.deltaTime*speedx, transform.position.y+Time.deltaTime*speedy, 0);
        }
    }
}