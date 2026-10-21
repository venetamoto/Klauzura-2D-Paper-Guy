using PaperGuy.Player;
using UnityEngine;

namespace PaperGuy.Cameras
{
    /// <summary>Zatřese kamerou, když hráč dopadne po pádu z výšky.</summary>
    public class PlayerLandingShake : MonoBehaviour
    {
        [SerializeField] private PlayerController2D player;
        [SerializeField] private CameraShake shake;
        [Tooltip("Minimální rychlost pádu (jednotek/s), od které se kamera třese.")]
        [SerializeField] private float minImpactSpeed = 14f;
        [SerializeField] private float amplitudePerUnitSpeed = 0.015f;
        [SerializeField] private float duration = 0.25f;

        private void OnEnable()
        {
            if (player != null) player.Landed += OnLanded;
        }

        private void OnDisable()
        {
            if (player != null) player.Landed -= OnLanded;
        }

        private void OnLanded(float impactSpeed)
        {
            if (impactSpeed < minImpactSpeed || shake == null) return;
            shake.Shake((impactSpeed - minImpactSpeed * 0.5f) * amplitudePerUnitSpeed, duration);
        }
    }
}
