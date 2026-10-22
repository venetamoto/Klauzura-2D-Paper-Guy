using PaperGuy.Player;
using UnityEngine;

namespace PaperGuy.Interactions
{
    public class NumberCollectible : CollectibleBase
    {
        [SerializeField] private int value;
        [Tooltip("Text pro UI, pokud to není číslo (\"+\", \"=\", \"x\").")]
        [SerializeField] private string symbol;
        [SerializeField] private bool isCorrectAnswer;

        public int Value => value;
        public string Symbol => string.IsNullOrEmpty(symbol) ? value.ToString() : symbol;
        public bool IsCorrectAnswer => isCorrectAnswer;

        protected override void OnCollected(PlayerController2D eater)
        {
            Debug.Log($"[NumberCollectible] Snědeno „{Symbol}“ (správně: {isCorrectAnswer})", this);
        }
    }
}
