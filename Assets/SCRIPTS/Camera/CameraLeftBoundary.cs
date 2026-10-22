using UnityEngine;

namespace PaperGuy.Cameras
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class CameraLeftBoundary : MonoBehaviour
    {
        [SerializeField] private CameraController2D cameraController;
        [SerializeField] private float wallThickness = 1f;
        [Tooltip("S jakými vrstvami stěna koliduje (obvykle jen Player).")]
        [SerializeField] private LayerMask collideWith = ~0;

        private Rigidbody2D _rb;
        private BoxCollider2D _box;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.interpolation = RigidbodyInterpolation2D.None;

            _box = GetComponent<BoxCollider2D>();
            _box.isTrigger = false;
            _box.excludeLayers = ~collideWith;

            if (cameraController == null)
                cameraController = FindAnyObjectByType<CameraController2D>();
        }

        private void Start() => Place(teleport: true);

        private void FixedUpdate() => Place(teleport: false);

        private void Place(bool teleport)
        {
            if (cameraController == null || cameraController.Camera == null) return;

            // Výška s rezervou, aby hráč stěnu nepřeskočil.
            _box.size = new Vector2(wallThickness, cameraController.HalfHeight * 4f);

            Vector2 pos = new Vector2(
                cameraController.LeftEdgeX - wallThickness * 0.5f,
                cameraController.BasePosition.y);

            if (teleport) _rb.position = pos;
            else _rb.MovePosition(pos);
        }
    }
}
