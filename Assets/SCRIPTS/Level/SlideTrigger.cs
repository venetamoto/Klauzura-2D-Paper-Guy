using PaperGuy.Player;
using UnityEngine;

namespace PaperGuy.Level
{
    [RequireComponent(typeof(Collider2D))]
    public class SlideTrigger : MonoBehaviour
    {
        [Tooltip("Komponenta implementující ISlidePath (např. FunctionGraphPath).")]
        [SerializeField] private MonoBehaviour pathSource;
        [SerializeField] private bool oneShot = true;

        private ISlidePath _path;
        private bool _used;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            _path = pathSource as ISlidePath;
            if (_path == null) Debug.LogError($"[SlideTrigger] {pathSource} neimplementuje ISlidePath", this);
        }

        private void OnValidate()
        {
            if (pathSource != null && pathSource is not ISlidePath)
            {
                Debug.LogWarning($"[SlideTrigger] {pathSource.GetType().Name} neimplementuje ISlidePath", this);
                pathSource = null;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_used || _path == null) return;
            Rigidbody2D body = other.attachedRigidbody;
            if (body == null || !body.TryGetComponent(out PlayerController2D player)) return;

            player.StartSlide(_path);
            if (oneShot) _used = true;
        }
    }
}
