using Unity.VisualScripting;
using UnityEngine;

public class pohyb : MonoBehaviour
{
    [SerializeField] float speedX = 1f;
    [SerializeField] float speedY = 1f;

    void FixedUpdate()
    {
        transform.position = transform.position + new Vector3(Input.GetAxis("Horizontal")*speedX*Time.deltaTime,Input.GetAxis("Vertical")*speedY*Time.deltaTime,0);
    }
}