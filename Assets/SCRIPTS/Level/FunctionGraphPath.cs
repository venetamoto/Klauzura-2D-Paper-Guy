using System;
using UnityEngine;

namespace PaperGuy.Level
{
    [ExecuteAlways]
    public class FunctionGraphPath : MonoBehaviour, ISlidePath
    {
        public enum GraphFunction
        {
            NegativeParabola, // y = -x^2
            Parabola,         // y = x^2
            Linear,           // y = -x
            Sine,             // y = sin(x)
            Custom            // y = customCurve(x)
        }

        [SerializeField] private GraphFunction function = GraphFunction.NegativeParabola;
        [SerializeField] private AnimationCurve customCurve = AnimationCurve.Linear(0f, 0f, 1f, -1f);

        [Header("Definiční obor")]
        [SerializeField] private float xFrom = 0f;
        [SerializeField] private float xTo = 3f;
        [Tooltip("Měřítko os grafu (x — šířka, y — strmost).")]
        [SerializeField] private Vector2 scale = new Vector2(2f, 0.6f);
        [SerializeField, Range(8, 512)] private int samples = 96;

        [Header("Volitelné výstupy")]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private EdgeCollider2D edgeCollider;

        private Vector2[] _localPoints = Array.Empty<Vector2>();
        private float[] _cumulative = Array.Empty<float>();

        public float Length { get; private set; }

        private bool _dirty;

        private void Awake() => Rebuild();

        private void OnValidate()
        {
            if (xTo <= xFrom) xTo = xFrom + 0.01f;
            _dirty = true; // v OnValidate se nesmí měnit jiné komponenty — přestavíme v Update
        }

        private void Update()
        {
            if (!_dirty) return;
            _dirty = false;
            Rebuild();
        }

        public float Evaluate(float x) => function switch
        {
            GraphFunction.NegativeParabola => -x * x,
            GraphFunction.Parabola => x * x,
            GraphFunction.Linear => -x,
            GraphFunction.Sine => Mathf.Sin(x),
            GraphFunction.Custom => customCurve.Evaluate(x),
            _ => 0f
        };

        [ContextMenu("Rebuild")]
        public void Rebuild()
        {
            int count = Mathf.Max(2, samples + 1);
            if (_localPoints.Length != count)
            {
                _localPoints = new Vector2[count];
                _cumulative = new float[count];
            }

            float y0 = Evaluate(xFrom);
            for (int i = 0; i < count; i++)
            {
                float x = Mathf.Lerp(xFrom, xTo, i / (float)(count - 1));
                // Začátek grafu leží v pivotu objektu — snadno se umisťuje ve scéně.
                _localPoints[i] = new Vector2((x - xFrom) * scale.x, (Evaluate(x) - y0) * scale.y);
            }

            RecalculateLengths();
            WriteOutputs();
        }

        private void RecalculateLengths()
        {
            _cumulative[0] = 0f;
            for (int i = 1; i < _localPoints.Length; i++)
            {
                Vector2 a = transform.TransformPoint(_localPoints[i - 1]);
                Vector2 b = transform.TransformPoint(_localPoints[i]);
                _cumulative[i] = _cumulative[i - 1] + Vector2.Distance(a, b);
            }
            Length = _cumulative[_cumulative.Length - 1];
        }

        private void WriteOutputs()
        {
            if (lineRenderer != null)
            {
                lineRenderer.useWorldSpace = false;
                lineRenderer.positionCount = _localPoints.Length;
                for (int i = 0; i < _localPoints.Length; i++)
                    lineRenderer.SetPosition(i, _localPoints[i]);
            }

            if (edgeCollider != null && edgeCollider.transform == transform)
                edgeCollider.points = _localPoints;
        }

        public Vector2 GetPoint(float distance)
        {
            int i = FindSegment(distance, out float t);
            return transform.TransformPoint(Vector2.Lerp(_localPoints[i], _localPoints[i + 1], t));
        }

        public Vector2 GetTangent(float distance)
        {
            int i = FindSegment(distance, out _);
            Vector2 a = transform.TransformPoint(_localPoints[i]);
            Vector2 b = transform.TransformPoint(_localPoints[i + 1]);
            return (b - a).normalized;
        }

        public float GetClosestDistance(Vector2 worldPoint)
        {
            float best = float.MaxValue, bestDistance = 0f;
            for (int i = 0; i < _localPoints.Length - 1; i++)
            {
                Vector2 a = transform.TransformPoint(_localPoints[i]);
                Vector2 b = transform.TransformPoint(_localPoints[i + 1]);
                Vector2 ab = b - a;
                float t = Mathf.Clamp01(Vector2.Dot(worldPoint - a, ab) / Mathf.Max(ab.sqrMagnitude, 1e-6f));
                float sqr = (a + ab * t - worldPoint).sqrMagnitude;
                if (sqr < best)
                {
                    best = sqr;
                    bestDistance = Mathf.Lerp(_cumulative[i], _cumulative[i + 1], t);
                }
            }
            return bestDistance;
        }

        private int FindSegment(float distance, out float t)
        {
            if (_localPoints.Length < 2) Rebuild();
            if (transform.hasChanged) { RecalculateLengths(); transform.hasChanged = false; }

            distance = Mathf.Clamp(distance, 0f, Length);
            int lo = 0, hi = _cumulative.Length - 1;
            while (hi - lo > 1)
            {
                int mid = (lo + hi) >> 1;
                if (_cumulative[mid] <= distance) lo = mid; else hi = mid;
            }

            float segment = _cumulative[hi] - _cumulative[lo];
            t = segment > 1e-6f ? (distance - _cumulative[lo]) / segment : 0f;
            return lo;
        }

        private void OnDrawGizmos()
        {
            if (_localPoints.Length < 2) return;
            Gizmos.color = new Color(0.2f, 0.4f, 1f, 0.9f);
            for (int i = 0; i < _localPoints.Length - 1; i++)
                Gizmos.DrawLine(transform.TransformPoint(_localPoints[i]), transform.TransformPoint(_localPoints[i + 1]));
        }
    }
}
