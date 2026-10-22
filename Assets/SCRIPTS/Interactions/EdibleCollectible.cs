using PaperGuy.Player;
using UnityEngine;

namespace PaperGuy.Interactions
{
    public class EdibleCollectible : CollectibleBase
    {
        public enum EdibleKind
        {
            Text,
            Drawing
        }

        [SerializeField] private EdibleKind kind = EdibleKind.Text;
        [Tooltip("Popis pro UI/log (např. \"slovo Úkol\", \"nakreslený trojúhelník\").")]
        [SerializeField] private string description;

        public EdibleKind Kind => kind;
        public string Description => string.IsNullOrEmpty(description) ? name : description;

        private void Reset()
        {
            kind = name.ToLowerInvariant().Contains("draw") ? EdibleKind.Drawing : EdibleKind.Text;
            if (TryGetComponent(out Collider2D col)) col.isTrigger = true;
            else gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
        }

        protected override void OnCollected(PlayerController2D eater)
        {
            string what = kind == EdibleKind.Text ? "text" : "obrázek";
            Debug.Log($"[EdibleCollectible] Hráč snědl {what} „{Description}“", this);
        }
    }
}
