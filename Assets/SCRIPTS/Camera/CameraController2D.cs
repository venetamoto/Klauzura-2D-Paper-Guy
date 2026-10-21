using UnityEngine;

namespace PaperGuy.Cameras
{
    /// <summary>
    /// Ortografická kamera plošinovky:
    /// • mrtvá zóna + předstih ve směru běhu (look-ahead);
    /// • oddělené vyhlazování X/Y;
    /// • „západka“ — kamera nikdy nejede doleva;
    /// • automatický posun (honička) — spodní mez pozice kamery, která se sama posouvá doprava;
    /// • omezení hranicemi úrovně (confiner) podle BoxCollider2D;
    /// • přičítání posunu z CameraShake.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(100)] // po všech LateUpdate herní logiky
    public class CameraController2D : MonoBehaviour
    {
        [Header("Cíl")]
        [SerializeField] private Transform target;
        [Tooltip("Pokud je zadáno, look-ahead bere rychlost odsud, jinak ji počítá z posunu Transformu.")]
        [SerializeField] private Rigidbody2D targetBody;
        [SerializeField] private Vector2 offset = new Vector2(0f, 1f);

        [Header("Mrtvá zóna (poloviční rozměry, v jednotkách)")]
        [SerializeField] private Vector2 deadZone = new Vector2(0.4f, 0.8f);

        [Header("Předstih (look-ahead)")]
        [SerializeField] private float lookAheadDistance = 2.5f;
        [SerializeField] private float lookAheadSmoothTime = 0.5f;
        [Tooltip("Pod touto rychlostí předstih nemění směr.")]
        [SerializeField] private float lookAheadMinSpeed = 0.5f;

        [Header("Tlumení")]
        [SerializeField] private float smoothTimeX = 0.15f;
        [SerializeField] private float smoothTimeY = 0.35f;

        [Header("Jednosměrnost")]
        [Tooltip("Kamera se pohybuje jen doprava.")]
        [SerializeField] private bool oneWay = true;

        [Header("Hranice úrovně")]
        [Tooltip("Hranice úrovně. Kamera neukáže nic mimo ně.")]
        [SerializeField] private BoxCollider2D levelBounds;

        [Header("Efekty")]
        [SerializeField] private CameraShake shake;

        private Camera _camera;
        private Vector2 _basePosition;      // pozice bez třesu
        private Vector2 _trackPoint;        // bod sledovaný mrtvou zónou
        private float _lookAhead;
        private float _lookAheadVelocity;
        private float _velocityX;
        private float _velocityY;
        private Vector3 _lastTargetPosition;

        private float _autoScrollSpeed;
        private float _autoScrollTargetSpeed;
        private float _autoScrollAcceleration;
        private float _autoScrollFloorX = float.NegativeInfinity;

        public Camera Camera => _camera;
        public float HalfHeight => _camera.orthographicSize;
        public float HalfWidth => _camera.orthographicSize * _camera.aspect;

        /// <summary>Levý okraj viditelné oblasti (bez třesu). Používá ho omezovač hráče.</summary>
        public float LeftEdgeX => _basePosition.x - HalfWidth;
        public float RightEdgeX => _basePosition.x + HalfWidth;
        public Vector2 BasePosition => _basePosition;
        public bool IsAutoScrolling => _autoScrollTargetSpeed > 0f || _autoScrollSpeed > 0f;
        public float AutoScrollSpeed => _autoScrollSpeed;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (shake == null) shake = GetComponent<CameraShake>();
            if (targetBody == null && target != null) targetBody = target.GetComponent<Rigidbody2D>();
            SnapToTarget();
        }

        public void SetTarget(Transform newTarget, bool snap = true)
        {
            target = newTarget;
            targetBody = target != null ? target.GetComponent<Rigidbody2D>() : null;
            if (snap) SnapToTarget();
        }

        /// <summary>Okamžitě postavit kameru na cíl (start úrovně, respawn).</summary>
        public void SnapToTarget()
        {
            if (target == null)
            {
                _basePosition = transform.position;
                return;
            }

            _lastTargetPosition = target.position;
            _trackPoint = (Vector2)target.position + offset;
            _lookAhead = 0f;
            _velocityX = _velocityY = _lookAheadVelocity = 0f;
            _basePosition = Confine(_trackPoint);
            ApplyTransform();
        }

        /// <summary>
        /// Zapnout automatický posun doprava (honička). Kamera zrychlí na <paramref name="speed"/>
        /// za <paramref name="accelerationTime"/> sekund. Hráč může kameru předběhnout, ale nesmí zaostat.
        /// </summary>
        public void StartAutoScroll(float speed, float accelerationTime = 1f)
        {
            _autoScrollTargetSpeed = Mathf.Max(0f, speed);
            _autoScrollAcceleration = accelerationTime > 0f ? _autoScrollTargetSpeed / accelerationTime : float.PositiveInfinity;
            _autoScrollFloorX = Mathf.Max(_autoScrollFloorX, _basePosition.x);
        }

        public void StopAutoScroll()
        {
            _autoScrollTargetSpeed = 0f;
            _autoScrollSpeed = 0f;
        }

        public void Shake(float amplitude, float duration)
        {
            if (shake != null) shake.Shake(amplitude, duration);
        }

        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            Vector2 goal = _basePosition;

            if (target != null)
            {
                Vector2 targetPoint = (Vector2)target.position + offset;

                // Mrtvá zóna: sledovaný bod se hýbe, jen když cíl vyjde z rámečku.
                float dx = targetPoint.x - _trackPoint.x;
                float dy = targetPoint.y - _trackPoint.y;
                if (dx > deadZone.x) _trackPoint.x = targetPoint.x - deadZone.x;
                else if (dx < -deadZone.x) _trackPoint.x = targetPoint.x + deadZone.x;
                if (dy > deadZone.y) _trackPoint.y = targetPoint.y - deadZone.y;
                else if (dy < -deadZone.y) _trackPoint.y = targetPoint.y + deadZone.y;

                // Předstih ve směru horizontální rychlosti.
                float speedX = targetBody != null
                    ? targetBody.linearVelocity.x
                    : (target.position.x - _lastTargetPosition.x) / dt;
                _lastTargetPosition = target.position;

                float lookAheadGoal = _lookAhead;
                if (Mathf.Abs(speedX) > lookAheadMinSpeed)
                    lookAheadGoal = Mathf.Sign(speedX) * lookAheadDistance;
                _lookAhead = Mathf.SmoothDamp(_lookAhead, lookAheadGoal, ref _lookAheadVelocity, lookAheadSmoothTime);

                goal = new Vector2(_trackPoint.x + _lookAhead, _trackPoint.y);
            }

            Vector2 next;
            next.x = Mathf.SmoothDamp(_basePosition.x, goal.x, ref _velocityX, smoothTimeX, Mathf.Infinity, dt);
            next.y = Mathf.SmoothDamp(_basePosition.y, goal.y, ref _velocityY, smoothTimeY, Mathf.Infinity, dt);

            // Západka: zpátky nejedeme.
            if (oneWay && next.x < _basePosition.x)
            {
                next.x = _basePosition.x;
                _velocityX = 0f;
            }

            // Automatický posun: spodní mez pozice, která se sama plazí doprava.
            if (IsAutoScrolling)
            {
                _autoScrollSpeed = Mathf.MoveTowards(_autoScrollSpeed, _autoScrollTargetSpeed, _autoScrollAcceleration * dt);
                _autoScrollFloorX = Mathf.Max(_autoScrollFloorX, _basePosition.x) + _autoScrollSpeed * dt;
                next.x = Mathf.Max(next.x, _autoScrollFloorX);
            }

            _basePosition = Confine(next);
            ApplyTransform();
        }

        private void ApplyTransform()
        {
            Vector2 shakeOffset = shake != null ? shake.CurrentOffset : Vector2.zero;
            transform.position = new Vector3(
                _basePosition.x + shakeOffset.x,
                _basePosition.y + shakeOffset.y,
                transform.position.z);
        }

        private Vector2 Confine(Vector2 position)
        {
            if (levelBounds == null || _camera == null) return position;

            Bounds b = levelBounds.bounds;
            float hw = HalfWidth, hh = HalfHeight;

            position.x = b.size.x > hw * 2f ? Mathf.Clamp(position.x, b.min.x + hw, b.max.x - hw) : b.center.x;
            position.y = b.size.y > hh * 2f ? Mathf.Clamp(position.y, b.min.y + hh, b.max.y - hh) : b.center.y;
            return position;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.6f, 0f, 0.8f);
            Vector3 c = Application.isPlaying ? (Vector3)_trackPoint : transform.position;
            Gizmos.DrawWireCube(c, new Vector3(deadZone.x * 2f, deadZone.y * 2f, 0f));
        }
#endif
    }
}
