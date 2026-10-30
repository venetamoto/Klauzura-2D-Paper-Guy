using UnityEngine;

public class proměnaNaMrak : MonoBehaviour
{
    public GameObject mrakTransformatorPrefab;
    public GameObject cmarkaTransformatorPrefab;
    bool mrakTransformace = false;
    bool cmarkaTransformace = false;
    public static bool znicCmarku = false;
    public static bool znicMrak = false;
    public static bool znicZed = false;
    void Start()
    {
        znicCmarku = false;
        znicMrak = false;
        znicZed = false;
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
        if (collision.gameObject.tag == "bomba")
        {
            bomba.bombaSebrano = true;
        }
        if (collision.gameObject.tag == "zdivo")
        {
            if (bomba.bombaSebrano == true)
            {
                znicZed = true;
                mrakTransformace = true;
                znicMrak = true;
                transform.position = new Vector3(4f, -10f, 0f);
            }
        }
        if (collision.gameObject.tag == "čmárkaTransformator")
        {
            cmarkaTransformace = true;
            znicCmarku = true;
        }
    }
}
