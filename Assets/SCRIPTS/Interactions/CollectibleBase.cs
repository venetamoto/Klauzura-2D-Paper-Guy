using System;
using System.Collections;
using PaperGuy.Player;
using UnityEngine;
using UnityEngine.Events;

namespace PaperGuy.Interactions
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class CollectibleBase : MonoBehaviour, ICollectable
    {
        public static event Action<CollectibleBase, PlayerController2D> OnItemEaten;

        // Vynulování odběratelů při vypnutém Domain Reload (Enter Play Mode Options).
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => OnItemEaten = null;

        [Tooltip("Propojuje předmět s hádankou/bránou (např. \"Equation_01\").")]
        [SerializeField] private string groupId;
        [Tooltip("Za kolik sekund po snědení objekt skrýt (čas na animaci).")]
        [SerializeField] private float hideDelay = 0.15f;
        [SerializeField] private bool destroyOnEaten = false;
        [SerializeField] private UnityEvent onEaten;

        private Collider2D[] _colliders;
        private bool _collected;

        public string GroupId => groupId;
        public virtual bool CanBeCollected => !_collected && isActiveAndEnabled;

        protected virtual void Awake()
        {
            _colliders = GetComponents<Collider2D>();
            foreach (Collider2D c in _colliders) c.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!CanBeCollected) return;

            Rigidbody2D body = other.attachedRigidbody;
            if (body != null && body.TryGetComponent(out PlayerController2D player))
                player.TryEat(this);
        }

        public void Collect(PlayerController2D eater)
        {
            if (_collected) return;
            _collected = true;
            SetCollidersEnabled(false);

            OnCollected(eater);
            onEaten?.Invoke();
            OnItemEaten?.Invoke(this, eater);

            StartCoroutine(HideRoutine());
        }

        public virtual void ResetCollectible()
        {
            StopAllCoroutines();
            _collected = false;
            gameObject.SetActive(true);
            SetCollidersEnabled(true);
        }

        protected virtual void OnCollected(PlayerController2D eater) { }

        private IEnumerator HideRoutine()
        {
            if (hideDelay > 0f) yield return new WaitForSeconds(hideDelay);
            if (destroyOnEaten) Destroy(gameObject);
            else gameObject.SetActive(false);
        }

        private void SetCollidersEnabled(bool value)
        {
            foreach (Collider2D c in _colliders) c.enabled = value;
        }
    }
}
