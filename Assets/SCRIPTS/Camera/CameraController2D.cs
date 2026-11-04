using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController2D : MonoBehaviour
{
    public Transform target;
    public Vector2 offset = new Vector2(0f, 1f);
    public float smoothTime = 0.2f;
    public BoxCollider2D levelBounds;
    public CameraShake shake;

    [Header("Na zacatku ukaze cely level")]
    public float introWait = 1.5f;
    public float introTime = 1.5f;

    Camera cam;
    Vector3 basePosition;
    Vector3 velocity;
    float normalSize;
    float timer;

    void Start()
    {
        cam = GetComponent<Camera>();
        normalSize = cam.orthographicSize;
        basePosition = target != null ? GetGoal() : transform.position;
    }

    void LateUpdate()
    {
        if (target != null)
            basePosition = Vector3.SmoothDamp(basePosition, GetGoal(), ref velocity, smoothTime);

        Vector3 pos = basePosition;

        timer += Time.deltaTime;
        if (levelBounds != null && timer < introWait + introTime)
        {
            Bounds b = levelBounds.bounds;
            float wholeSize = Mathf.Max(b.extents.y, b.extents.x / cam.aspect);
            Vector3 whole = new Vector3(b.center.x, b.center.y, transform.position.z);
            float k = Mathf.SmoothStep(0f, 1f, (timer - introWait) / introTime);
            cam.orthographicSize = Mathf.Lerp(wholeSize, normalSize, k);
            pos = Vector3.Lerp(whole, basePosition, k);
        }
        else
        {
            cam.orthographicSize = normalSize;
        }

        if (shake != null) pos += (Vector3)shake.Offset;
        transform.position = pos;
    }

    Vector3 GetGoal()
    {
        Vector3 goal = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);

        if (levelBounds != null)
        {
            Bounds b = levelBounds.bounds;
            float h = normalSize;
            float w = h * cam.aspect;

            if (b.size.x > w * 2f) goal.x = Mathf.Clamp(goal.x, b.min.x + w, b.max.x - w);
            else goal.x = b.center.x;

            if (b.size.y > h * 2f) goal.y = Mathf.Clamp(goal.y, b.min.y + h, b.max.y - h);
            else goal.y = b.center.y;
        }

        return goal;
    }
}
