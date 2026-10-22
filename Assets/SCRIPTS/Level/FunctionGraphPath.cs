using UnityEngine;

[ExecuteAlways]
public class FunctionGraphPath : MonoBehaviour
{
    public enum GraphFunction { NegativeParabola, Parabola, Linear, Sine }

    public GraphFunction function = GraphFunction.NegativeParabola;
    public float xFrom = 0f;
    public float xTo = 3f;
    public Vector2 scale = new Vector2(2f, 0.6f);
    public int samples = 96;

    public LineRenderer lineRenderer;
    public EdgeCollider2D edgeCollider;

    public float Length { get; private set; }

    Vector2[] points;
    float[] lengths;
    bool needsRebuild;

    void Awake()
    {
        Rebuild();
    }

    void OnValidate()
    {
        needsRebuild = true;
    }

    void Update()
    {
        if (needsRebuild)
        {
            needsRebuild = false;
            Rebuild();
        }
    }

    float F(float x)
    {
        switch (function)
        {
            case GraphFunction.Parabola: return x * x;
            case GraphFunction.Linear: return -x;
            case GraphFunction.Sine: return Mathf.Sin(x);
            default: return -x * x;
        }
    }

    public void Rebuild()
    {
        int count = Mathf.Max(2, samples + 1);
        points = new Vector2[count];
        lengths = new float[count];

        float y0 = F(xFrom);
        for (int i = 0; i < count; i++)
        {
            float x = Mathf.Lerp(xFrom, xTo, i / (float)(count - 1));
            points[i] = new Vector2((x - xFrom) * scale.x, (F(x) - y0) * scale.y);
        }

        for (int i = 1; i < count; i++)
            lengths[i] = lengths[i - 1] + Vector2.Distance(World(i - 1), World(i));
        Length = lengths[count - 1];

        if (lineRenderer != null)
        {
            lineRenderer.useWorldSpace = false;
            lineRenderer.positionCount = count;
            for (int i = 0; i < count; i++)
                lineRenderer.SetPosition(i, points[i]);
        }

        if (edgeCollider != null)
            edgeCollider.points = points;
    }

    Vector2 World(int i)
    {
        return transform.TransformPoint(points[i]);
    }

    int Segment(float distance)
    {
        if (points == null) Rebuild();

        for (int i = 1; i < lengths.Length; i++)
            if (lengths[i] >= distance) return i - 1;
        return lengths.Length - 2;
    }

    public Vector2 GetPoint(float distance)
    {
        distance = Mathf.Clamp(distance, 0f, Length);
        int i = Segment(distance);
        float t = Mathf.InverseLerp(lengths[i], lengths[i + 1], distance);
        return Vector2.Lerp(World(i), World(i + 1), t);
    }

    public Vector2 GetDirection(float distance)
    {
        int i = Segment(Mathf.Clamp(distance, 0f, Length));
        return (World(i + 1) - World(i)).normalized;
    }

    public float ClosestDistance(Vector2 p)
    {
        if (points == null) Rebuild();

        float best = float.MaxValue;
        float result = 0f;
        for (int i = 0; i < points.Length; i++)
        {
            float d = (World(i) - p).sqrMagnitude;
            if (d < best)
            {
                best = d;
                result = lengths[i];
            }
        }
        return result;
    }

    void OnDrawGizmos()
    {
        if (points == null) return;
        Gizmos.color = Color.blue;
        for (int i = 0; i < points.Length - 1; i++)
            Gizmos.DrawLine(World(i), World(i + 1));
    }
}
