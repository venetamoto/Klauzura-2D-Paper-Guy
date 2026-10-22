using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EdibleCollectible : MonoBehaviour
{
    public float hideDelay = 0.15f;

    bool eaten;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (eaten || !enabled) return;

        PlayerController2D player = other.GetComponentInParent<PlayerController2D>();
        if (player == null) return;

        eaten = true;
        player.Eat();
        Debug.Log("Snedeno: " + name);
        Invoke(nameof(Hide), hideDelay);
    }

    void Hide()
    {
        gameObject.SetActive(false);
    }
}
