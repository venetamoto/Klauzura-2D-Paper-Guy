using PaperGuy.Interactions;
using PaperGuy.Player;
using UnityEngine;
using UnityEngine.Events;

namespace PaperGuy.Level
{
    /// <summary>
    /// Logická překážka s rovnicí. Otevře se, když hráč sní správnou odpověď
    /// se stejným groupId. Špatná odpověď se vrátí na své místo.
    /// </summary>
    public class EquationGate : MonoBehaviour
    {
        [SerializeField] private string groupId = "Equation_01";
        [Tooltip("Collider blokující cestu. Při otevření se vypne.")]
        [SerializeField] private Collider2D blocker;
        [SerializeField] private float wrongAnswerRespawnDelay = 1f;

        [SerializeField] private UnityEvent onOpened;
        [SerializeField] private UnityEvent onWrongAnswer;

        public bool IsOpen { get; private set; }

        private void OnEnable() => CollectibleBase.OnItemEaten += HandleItemEaten;
        private void OnDisable() => CollectibleBase.OnItemEaten -= HandleItemEaten;

        private void HandleItemEaten(CollectibleBase item, PlayerController2D _)
        {
            if (IsOpen || item.GroupId != groupId || item is not NumberCollectible number) return;

            if (number.IsCorrectAnswer)
            {
                Open();
            }
            else
            {
                onWrongAnswer?.Invoke();
                StartCoroutine(RespawnLater(number));
            }
        }

        public void Open()
        {
            IsOpen = true;
            if (blocker != null) blocker.enabled = false;
            onOpened?.Invoke();
        }

        private System.Collections.IEnumerator RespawnLater(CollectibleBase item)
        {
            yield return new WaitForSeconds(wrongAnswerRespawnDelay);
            if (!IsOpen && item != null) item.ResetCollectible();
        }
    }
}
