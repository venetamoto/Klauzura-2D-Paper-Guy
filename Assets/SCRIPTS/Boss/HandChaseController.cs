using System.Collections;
using PaperGuy.Cameras;
using PaperGuy.Level;
using PaperGuy.Player;
using UnityEngine;
using UnityEngine.Events;

namespace PaperGuy.Boss
{
    public class HandChaseController : MonoBehaviour
    {
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private CameraController2D cameraController;
        [SerializeField] private PlayerController2D player;
        [SerializeField] private EraserHazard eraser;
        [Tooltip("Ruka autora (sprite/Animator). Zapne se během intra.")]
        [SerializeField] private GameObject hand;

        [Header("Intro")]
        [SerializeField] private float introDuration = 1.2f;
        [SerializeField] private bool freezePlayerDuringIntro = true;
        [SerializeField] private float introShakeAmplitude = 0.35f;
        [SerializeField] private float introShakeDuration = 0.8f;

        [Header("Posun")]
        [SerializeField] private float startSpeed = 3f;
        [SerializeField] private float maxSpeed = 6.5f;
        [Tooltip("Za kolik sekund rychlost dojde od start do max.")]
        [SerializeField] private float rampDuration = 20f;

        [SerializeField] private UnityEvent onChaseStarted;
        [SerializeField] private UnityEvent onChaseEnded;

        private float _chaseTime;
        private bool _running;

        private void OnEnable()
        {
            if (levelManager != null) levelManager.PhaseChanged += OnPhaseChanged;
        }

        private void OnDisable()
        {
            if (levelManager != null) levelManager.PhaseChanged -= OnPhaseChanged;
        }

        private void OnPhaseChanged(LevelPhase _, LevelPhase next)
        {
            if (next == LevelPhase.FinalChase) StartCoroutine(ChaseRoutine());
            else if (next == LevelPhase.Complete) StopChase();
        }

        private IEnumerator ChaseRoutine()
        {
            if (hand != null) hand.SetActive(true);
            cameraController.Shake(introShakeAmplitude, introShakeDuration);

            if (freezePlayerDuringIntro && player != null) player.SetControlEnabled(false);
            yield return new WaitForSeconds(introDuration);
            if (freezePlayerDuringIntro && player != null) player.SetControlEnabled(true);

            if (eraser != null) eraser.Activate(cameraController);
            cameraController.StartAutoScroll(startSpeed, 0.75f);
            _chaseTime = 0f;
            _running = true;
            onChaseStarted?.Invoke();
        }

        private void Update()
        {
            if (!_running) return;

            _chaseTime += Time.deltaTime;
            float t = rampDuration > 0f ? Mathf.Clamp01(_chaseTime / rampDuration) : 1f;
            cameraController.StartAutoScroll(Mathf.Lerp(startSpeed, maxSpeed, t), 0f);
        }

        public void StopChase()
        {
            if (!_running) return;
            _running = false;
            cameraController.StopAutoScroll();
            if (eraser != null) eraser.Deactivate();
            onChaseEnded?.Invoke();
        }
    }
}
