using Unity.VisualScripting;
using UnityEngine;

public class ovládáníMraku : MonoBehaviour
{
    [SerializeField] float speedX = 1f;
    [SerializeField] float speedY = 1f;
    public SpriteRenderer spriteRenderer;

    public proměnaNaMrak promena;

    void Start()
    {
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
    }
}