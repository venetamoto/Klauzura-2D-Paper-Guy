using PaperGuy.Cameras;
using PaperGuy.Player;
using UnityEngine;

namespace PaperGuy.Boss
{
    [RequireComponent(typeof(Collider2D))]
    [DefaultExecutionOrder(110)] // po CameraController2D
    public class EraserHazard : MonoBehaviour
    {
        [Tooltip("Posun středu gumy od levého okraje záběru (doprava +).")]
        [SerializeField] private float offsetFromLeftEdge = 0.3f;
        [SerializeField] private float bobAmplitude = 0.25f;
        [SerializeField] private float bobFrequency = 1.5f;

        private CameraController2D _camera;
        private bool _active;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            if (!_active) gameObject.SetActive(false);
        }

        public void Activate(CameraController2D cameraController)
        {
            _camera = cameraController;
            _active = true;
            gameObject.SetActive(true);
            LateUpdate();
        }

        public void Deactivate()
        {
            _active = false;
            gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (!_active || _camera == null) return;
            float bob = Mathf.Sin(Time.time * bobFrequency * Mathf.PI * 2f) * bobAmplitude;
            transform.position = new Vector3(
                _camera.LeftEdgeX + offsetFromLeftEdge,
                _camera.BasePosition.y + bob,
                transform.position.z);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_active) return;
            Rigidbody2D body = other.attachedRigidbody;
            if (body != null && body.TryGetComponent(out PlayerController2D player))
                player.Kill();
        }
    }
}
