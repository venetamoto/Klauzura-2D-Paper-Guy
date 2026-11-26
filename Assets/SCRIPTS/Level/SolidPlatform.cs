using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SolidPlatform : MonoBehaviour
{
    public bool oneWay = true;
    [Tooltip("Kresba autorky: nikdy nebrat jako zed, i kdyz je vysoka (jinak by byla pevna ze vsech stran).")]
    public bool nikdyZed = false;

    void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = false;

        bool wall = !nikdyZed && col.bounds.size.y > col.bounds.size.x * 1.5f;
        if (!oneWay || wall) return;

        PlatformEffector2D effector = GetComponent<PlatformEffector2D>();
        if (effector == null) effector = gameObject.AddComponent<PlatformEffector2D>();
        effector.useOneWay = true;
        effector.surfaceArc = 170f;
        col.usedByEffector = true;
    }
}
