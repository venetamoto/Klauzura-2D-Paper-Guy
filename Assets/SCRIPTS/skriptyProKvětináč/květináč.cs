using UnityEngine;

public class květináč : MonoBehaviour
{
    public static bool seminko = false;
    bool zasazeno = false;
    public GameObject kytka1prefab;
    void Start()
    {
        seminko = false;
        zasazeno = false;
    }

    void Update()
    {
        
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (seminko == true && zasazeno == false)
            {
                Instantiate(kytka1prefab, transform.position, transform.rotation);
                zasazeno = true;
            }
        }
    }
}