using UnityEngine;

public class proměnaNaMrak : MonoBehaviour
{
    public GameObject mrakTransformatorPrefab;
    public GameObject cmarkaTransformatorPrefab;
    bool mrakTransformace = false;
    bool cmarkaTransformace = false;
    public static bool znicCmarku = false;
    public static bool znicMrak = false;
    void Start()
    {
        znicCmarku = false;
        znicMrak = false;
        mrakTransformace = false;
        cmarkaTransformace = false;
    }

    void Update()
    {
        if (mrakTransformace == true)
        {
            Instantiate(mrakTransformatorPrefab, transform.position, transform.rotation);
            Destroy(gameObject);
        }
        else if (cmarkaTransformace == true)
        {
            Instantiate(cmarkaTransformatorPrefab, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "mrakTransformator")
        {
            mrakTransformace = true;
            znicMrak = true;
        }
        if (collision.gameObject.tag == "čmárkaTransformator")
        {
            cmarkaTransformace = true;
            znicCmarku = true;
        }
    }
}
