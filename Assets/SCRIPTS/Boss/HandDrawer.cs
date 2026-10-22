using System.Collections;
using System.Collections.Generic;
using PaperGuy.Interactions;
using PaperGuy.Player;
using UnityEngine;

namespace PaperGuy.Boss
{
    // Ruka autora přiletí, nakreslí tvar a odletí. Nakreslený tvar je jedlý.
    [RequireComponent(typeof(Collider2D))]
    public class HandDrawer : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private Transform hand;
        [Tooltip("Jedlý objekt s LineRendererem — do něj se kreslí.")]
        [SerializeField] private EdibleCollectible drawing;
        [SerializeField] private LineRenderer line;

        [Header("Tvar (lokální body vůči Drawing)")]
        [SerializeField] private Vector2[] shape =
        {
            new Vector2(0f, 0.8f), new Vector2(0.19f, 0.26f), new Vector2(0.76f, 0.25f),
            new Vector2(0.3f, -0.1f), new Vector2(0.47f, -0.65f), new Vector2(0f, -0.32f),
            new Vector2(-0.47f, -0.65f), new Vector2(-0.3f, -0.1f), new Vector2(-0.76f, 0.25f),
            new Vector2(-0.19f, 0.26f), new Vector2(0f, 0.8f)
        };

        [Header("Časování")]
        [SerializeField, Min(0.1f)] private float drawSpeed = 2.5f;
        [SerializeField, Min(0f)] private float flyTime = 0.6f;
        [Tooltip("Odkud ruka přiletí a kam odletí (vůči tvaru).")]
        [SerializeField] private Vector2 offscreenOffset = new Vector2(5f, 10f);
        [Tooltip("Posun ruky od hrotu tužky (sprite ruky má pivot přímo na hrotu → 0,0).")]
        [SerializeField] private Vector2 handOffset = Vector2.zero;

        private bool _started;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            if (hand != null) hand.gameObject.SetActive(false);
            if (line != null) line.positionCount = 0;
            SetDrawingEdible(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_started) return;
            Rigidbody2D body = other.attachedRigidbody;
            if (body == null || !body.TryGetComponent(out PlayerController2D _)) return;

            _started = true;
            StartCoroutine(DrawRoutine());
        }

        private IEnumerator DrawRoutine()
        {
            Transform origin = drawing.transform;
            var points = new List<Vector3>(shape.Length);
            foreach (Vector2 p in shape) points.Add(origin.TransformPoint(p));

            Vector3 away = points[0] + (Vector3)offscreenOffset;
            hand.gameObject.SetActive(true);
            yield return Fly(away, points[0]);

            line.useWorldSpace = true;
            line.positionCount = 1;
            line.SetPosition(0, points[0]);

            for (int i = 1; i < points.Count; i++)
            {
                Vector3 from = points[i - 1], to = points[i];
                int index = line.positionCount;
                line.positionCount = index + 1;

                float length = Vector3.Distance(from, to);
                for (float t = 0f; t < length; t += drawSpeed * Time.deltaTime)
                {
                    Vector3 tip = Vector3.Lerp(from, to, t / length);
                    line.SetPosition(index, tip);
                    hand.position = tip + (Vector3)handOffset;
                    yield return null;
                }
                line.SetPosition(index, to);
            }

            yield return Fly(points[points.Count - 1], away);
            hand.gameObject.SetActive(false);
            SetDrawingEdible(true);
        }

        private IEnumerator Fly(Vector3 from, Vector3 to)
        {
            for (float t = 0f; t < flyTime; t += Time.deltaTime)
            {
                hand.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / flyTime)) + (Vector3)handOffset;
                yield return null;
            }
            hand.position = to + (Vector3)handOffset;
        }

        // Dokud se kreslí, nejde tvar sníst.
        private void SetDrawingEdible(bool edible)
        {
            if (drawing == null) return;
            drawing.enabled = edible;
            foreach (Collider2D c in drawing.GetComponents<Collider2D>()) c.enabled = edible;
        }
    }
}
