using UnityEngine;

namespace PaperGuy.Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class BoundaryWall : MonoBehaviour
    {
        [Tooltip("Skrýt sprite po spuštění hry (stěna je pak neviditelná, ale pevná).")]
        [SerializeField] private bool hideInGame = true;

        private void Reset() => MakeSolid();

        private void Awake()
        {
            MakeSolid();
            if (hideInGame && TryGetComponent(out SpriteRenderer sr)) sr.enabled = false;
        }

        private void MakeSolid() => GetComponent<BoxCollider2D>().isTrigger = false;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!TryGetComponent(out BoxCollider2D box)) return;
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.8f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.offset, box.size);
        }
#endif
    }
}
