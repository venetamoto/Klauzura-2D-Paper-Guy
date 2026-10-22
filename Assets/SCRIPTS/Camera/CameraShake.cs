using UnityEngine;

namespace PaperGuy.Cameras
{
    public class CameraShake : MonoBehaviour
    {
        [SerializeField] private float frequency = 25f;
        [Tooltip("Křivka útlumu: 1 → 0 během trvání třesu.")]
        [SerializeField] private AnimationCurve falloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        [SerializeField] private float maxAmplitude = 0.6f;

        private float _amplitude;
        private float _duration;
        private float _elapsed;
        private float _seed;

        public Vector2 CurrentOffset { get; private set; }

        private void Awake() => _seed = Random.value * 100f;

        public void Shake(float amplitude, float duration)
        {
            amplitude = Mathf.Min(amplitude, maxAmplitude);
            float remaining = CurrentStrength();
            if (amplitude < remaining && _elapsed < _duration) return; // aktuální třes je silnější

            _amplitude = amplitude;
            _duration = Mathf.Max(0.01f, duration);
            _elapsed = 0f;
        }

        public void Stop()
        {
            _elapsed = _duration;
            CurrentOffset = Vector2.zero;
        }

        private float CurrentStrength() =>
            _elapsed >= _duration ? 0f : _amplitude * falloff.Evaluate(_elapsed / _duration);

        private void Update()
        {
            if (_elapsed >= _duration)
            {
                CurrentOffset = Vector2.zero;
                return;
            }

            _elapsed += Time.deltaTime;
            float strength = CurrentStrength();
            float t = Time.time * frequency;
            CurrentOffset = new Vector2(
                (Mathf.PerlinNoise(_seed, t) - 0.5f) * 2f,
                (Mathf.PerlinNoise(_seed + 37f, t) - 0.5f) * 2f) * strength;
        }
    }
}
