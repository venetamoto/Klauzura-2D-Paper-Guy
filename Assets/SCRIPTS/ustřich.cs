using UnityEngine;

public class ustřich : MonoBehaviour
{
    public static bool ustrihnuto = false;
    void Start()
    {
        ustrihnuto = false;
    }
    void Update()
    {
        if (ustrihnuto == true)
        {
            Destroy(gameObject);
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (zničNůžky.nuzky == true)
            {
                ustrihnuto = true;
            }
        }
    } 
}
