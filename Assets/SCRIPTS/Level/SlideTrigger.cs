using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SlideTrigger : MonoBehaviour
{
    public FunctionGraphPath pathSource;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController2D player = other.GetComponentInParent<PlayerController2D>();
        if (player != null) player.StartSlide(pathSource);
    }
}
