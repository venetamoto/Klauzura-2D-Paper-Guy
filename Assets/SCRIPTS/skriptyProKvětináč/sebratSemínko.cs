using UnityEngine;

public class sebratSemínko : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            květináč.seminko = true;
            Destroy(gameObject);
        }
    }
}
