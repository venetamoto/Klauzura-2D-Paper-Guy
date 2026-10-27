using UnityEngine;

public class proměnaNaMrak : MonoBehaviour
{
    public GameObject mrakTransformatorPrefab;
    bool mrakTransformace = false;
    void Start()
    {
        mrakTransformace = false;
    }

    void Update()
    {
        if (mrakTransformace == true)
        {
            Instantiate(mrakTransformatorPrefab, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "mrakTransformator")
        {
            mrakTransformace = true;
        }
    }
}
