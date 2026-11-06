using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BossTrigger : MonoBehaviour
{
    public HandChaseController hand;
    [Tooltip("Kolik casu ma hrac na reakci, nez guma vyrazi.")]
    public float reactionTime = 1.5f;

    bool done;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        hand.gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (done || other.GetComponentInParent<PlayerController2D>() == null) return;
        done = true;
        Debug.Log("Guma prichazi");
        hand.gameObject.SetActive(true);
        Invoke(nameof(Pust), reactionTime);
    }

    void Pust()
    {
        hand.StartChase();
    }
}
