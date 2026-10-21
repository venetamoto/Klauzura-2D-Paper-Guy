using PaperGuy.Player;
using UnityEngine;
using UnityEngine.Events;

namespace PaperGuy.Level
{
    /// <summary>Neviditelná čára na stránce: hráč ji překročí — úroveň přejde do další etapy.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class LevelZoneTrigger : MonoBehaviour
    {
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private LevelPhase phase;
        [SerializeField] private UnityEvent onEntered;

        private bool _fired;

        private void Awake() => GetComponent<Collider2D>().isTrigger = true;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_fired) return;
            Rigidbody2D body = other.attachedRigidbody;
            if (body == null || !body.TryGetComponent(out PlayerController2D _)) return;

            _fired = true;
            if (levelManager != null) levelManager.SetPhase(phase);
            onEntered?.Invoke();
        }
    }
}
