using UnityEngine;

public class masivníKlíč : MonoBehaviour
{
    [SerializeField]float speedx = 1f;
    [SerializeField]float speedy = -5f;
    void Start()
    {
        
    }

    void Update()
    {
        if (ustřich.ustrihnuto == true)
        {
            transform.position = new Vector3(transform.position.x+Time.deltaTime*speedx, transform.position.y+Time.deltaTime*speedy, 0);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "zdivo")
        {
            proměnaNaMrak.znicZed2 = true;
        }
    }
}
