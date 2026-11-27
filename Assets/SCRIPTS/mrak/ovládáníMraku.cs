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
    bool POCKEJ = false;
    public bool canDash = true;
    public SpriteRenderer spriteRenderer;
    public proměnaNaMrak promena;
    private Rigidbody2D gravitace;

    void Start()
    {
        coolDown = false;
        cas = false;
        dash = false;
        POCKEJ = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
        gravitace = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        transform.position = transform.position + new Vector3(Input.GetAxis("Horizontal")*speedX*Time.deltaTime,Input.GetAxis("Vertical")*speedY*Time.deltaTime,0);
        if (Input.GetKeyDown(KeyCode.D))
        {
            spriteRenderer.flipX = false;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            spriteRenderer.flipX = true;
        }

    //dash pro pravou stranu
    if (canDash == false) return;
        

        if (POCKEJ == false)
        {
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
                StartCoroutine(CoolDown());
            }
        }

    //dash pro levou stranu
        if (POCKEJ == false)
        {
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
                StartCoroutine(CoolDown());
            }
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
    public IEnumerator CoolDown()
    {
        yield return new WaitForSeconds(0.2f);
        POCKEJ = true;
        dashUI.spustSlider = true;
        yield return new WaitForSeconds(2.0f);
        dashUI.zmensise = true;
        dashUI.spustSlider = false;
        POCKEJ = false;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "omítka")
        {
            canDash = false;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "omítka")
        {
            canDash = true;
        }
    }
}