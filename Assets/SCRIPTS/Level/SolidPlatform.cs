using UnityEngine;

namespace PaperGuy.Level
{
    public class SolidPlatform : MonoBehaviour
    {
        private void Reset() => EnsureSolidCollider();
        private void Awake() => EnsureSolidCollider();

        private void EnsureSolidCollider()
        {
            if (!TryGetComponent(out Collider2D col))
                col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = false;
        }
    }
}
