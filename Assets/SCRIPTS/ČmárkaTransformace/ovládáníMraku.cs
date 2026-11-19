using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Callbacks;
using UnityEngine;

public class ovládáníMraku : MonoBehaviour
{
    [SerializeField] float speedX = 1f;
    [SerializeField] float speedY = 1f;
    [SerializeField]float speedX2 = 1f;
    [SerializeField]float speedY2 = 0f;
    [SerializeField]float speedX3 = -1f;
    [SerializeField]float speedY3 = 0f;
    bool dash = false;
    bool cas = false;
    bool coolDown = false;
    bool dash2 = false;
    bool cas2 = false;
    bool coolDown2 = false;
    public SpriteRenderer spriteRenderer;
    public proměnaNaMrak promena;
    private Rigidbody2D gravitace;

    void Start()
    {
        coolDown = false;
        cas = false;
        dash = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
        gravitace = GetComponent<Rigidbody2D>();
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

        if (filipLevelSmrt.padni == true)
        {
            gravitace.gravityScale = 0f;
        }

        if (coolDown == true)
        {
            if (Input.GetKeyUp(KeyCode.D) || cas == true)
            {
                if (Input.GetKeyDown(KeyCode.D))
                {
                    dash = true;
                }
            }
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            StartCoroutine(Počkej());
        }
        if (dash == true)
        {
            transform.position = new Vector3(transform.position.x+Time.deltaTime*speedX2, transform.position.y+Time.deltaTime*speedY2, 0);
        }

        if (coolDown2 == true)
        {
            if (Input.GetKeyUp(KeyCode.A) || cas2 == true)
            {
                if (Input.GetKeyDown(KeyCode.A))
                {
                    dash2 = true;
                }
            }
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            StartCoroutine(Počkej2());
        }
        if (dash2 == true)
        {
            transform.position = new Vector3(transform.position.x+Time.deltaTime*speedX3, transform.position.y+Time.deltaTime*speedY3, 0);
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
    public IEnumerator Počkej2()
    {
        cas2 = true;
        coolDown2 = true;
        yield return new WaitForSeconds(0.2f);
        coolDown2 = false;
        cas2 = false;
        dash2 = false;
    }
}