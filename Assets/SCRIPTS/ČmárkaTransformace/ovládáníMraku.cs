using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ovládáníMraku : MonoBehaviour
{
    [SerializeField] float speedX = 1f;
    [SerializeField] float speedY = 1f;
    [SerializeField]float speedX2 = 300f;
    [SerializeField]float speedY2 = 0f;
    bool dash = false;
    bool cas = false;
    bool coolDown = false;
    public SpriteRenderer spriteRenderer;
    public proměnaNaMrak promena;

    void Start()
    {
        coolDown = false;
        cas = false;
        dash = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        transform.position = transform.position + new Vector3(Input.GetAxis("Horizontal")*speedX*Time.deltaTime,Input.GetAxis("Vertical")*speedY*Time.deltaTime,0);
        if (Input.GetKeyDown(KeyCode.D))
        {
            spriteRenderer.flipX = true;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            spriteRenderer.flipX = false;
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            StartCoroutine(Počkej());
        }
        if (coolDown == true)
        {
            if (Input.GetKeyDown(KeyCode.D) || cas == true)
            {
                dash = true;
            }
        }
        if (dash == true)
        {
            transform.position = new Vector3(transform.position.x+Time.deltaTime*speedX2*6f, transform.position.y+Time.deltaTime*speedY2, 0);
        }
    }
    public IEnumerator Počkej()
    {
        cas = true;
        coolDown = true;
        yield return new WaitForSeconds(0.2f);
        coolDown = false;
        cas = false;
        dash = false;
    }
}