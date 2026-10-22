using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BoundaryWall : MonoBehaviour
{
    public bool hideInGame = true;

    void Start()
    {
        GetComponent<BoxCollider2D>().isTrigger = false;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (hideInGame && sr != null) sr.enabled = false;
    }
}
