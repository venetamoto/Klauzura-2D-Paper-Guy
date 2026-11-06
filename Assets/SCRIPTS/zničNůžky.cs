using UnityEngine;

public class zničNůžky : MonoBehaviour
{
    public static bool nuzky = false;
    void Start()
    {
        nuzky = false;
    }

    void Update()
    {
        
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            nuzky = true;
            bomba.bombaSebrano = false;
            Destroy(gameObject);
        }
    } 
}
