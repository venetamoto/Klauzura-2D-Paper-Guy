using System;
using System.Collections;
using PaperGuy.Interactions;
using PaperGuy.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PaperGuy.Level
{
    public enum LevelPhase
    {
        GraphSlide,        // sjezd po grafu
        EquationBarrier,   // překážka s rovnicí
        ShapePlatforming,  // skákání po geometrických tvarech
        FinalChase,        // honička Ruky/gumy
        Complete
    }

    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private PlayerController2D player;
        [SerializeField] private LevelPhase startPhase = LevelPhase.GraphSlide;
        [SerializeField] private float restartDelay = 1.5f;

        public LevelPhase Phase { get; private set; }

        public event Action<LevelPhase, LevelPhase> PhaseChanged;

        private void Awake() => Phase = startPhase;

        private void OnEnable()
        {
            if (player != null) player.Died += OnPlayerDied;
            CollectibleBase.OnItemEaten += OnItemEaten;
        }

        private void OnDisable()
        {
            if (player != null) player.Died -= OnPlayerDied;
            CollectibleBase.OnItemEaten -= OnItemEaten;
        }

        public void SetPhase(LevelPhase next)
        {
            if (next <= Phase) return;
            LevelPhase previous = Phase;
            Phase = next;
            PhaseChanged?.Invoke(previous, next);
        }

        private void OnItemEaten(CollectibleBase item, PlayerController2D _)
        {
            // Místo pro skóre/statistiky. Logika bran je v EquationGate.
        }

        private void OnPlayerDied() => StartCoroutine(RestartRoutine());

        private IEnumerator RestartRoutine()
        {
            yield return new WaitForSeconds(restartDelay);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
