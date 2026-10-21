using PaperGuy.Level;
using UnityEngine;

namespace PaperGuy.Player
{
    /// <summary>
    /// Klouzání po grafu. Tělo se stane kinematickým a „přilepí se“ k trajektorii,
    /// vstup se ignoruje. Rychlost roste úměrně strmosti (průmět gravitace do tečny).
    /// Na konci dráhy se hybnost předá zpět dynamickému tělu.
    /// </summary>
    public class PlayerSlideState : PlayerState
    {
        private ISlidePath _path;
        private float _distance;
        private float _speed;

        public PlayerSlideState(PlayerController2D player) : base(player) { }

        public override PlayerAnimState AnimState => PlayerAnimState.Slide;
        public override bool UsesBackLimit => false;

        public ISlidePath Path => _path;
        public float Speed => _speed;

        public void Setup(ISlidePath path) => _path = path;

        public override void Enter()
        {
            var s = Player.Settings;
            Rigidbody2D rb = Player.Body;

            _distance = _path.GetClosestDistance(rb.position - Vector2.up * Player.FootOffset);
            Vector2 tangent = _path.GetTangent(_distance);
            _speed = Mathf.Max(s.slideMinEntrySpeed, Vector2.Dot(rb.linearVelocity, tangent));

            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        public override void FixedTick(float dt)
        {
            var s = Player.Settings;
            Vector2 tangent = _path.GetTangent(_distance);

            // Zrychlení podél křivky: g * sin(úhlu sklonu) + konstantní „postrčení“.
            float slopeAccel = -tangent.y * s.slideGravity;
            _speed = Mathf.Clamp(_speed + (slopeAccel + s.slideBaseAcceleration) * dt, s.slideMinSpeed, s.slideMaxSpeed);
            _distance += _speed * dt;

            if (_distance >= _path.Length)
            {
                Vector2 end = _path.GetPoint(_path.Length) + Vector2.up * Player.FootOffset;
                Player.FinishSlide(end, _path.GetTangent(_path.Length) * _speed * s.slideExitMomentum);
                return;
            }

            Vector2 point = _path.GetPoint(_distance) + Vector2.up * Player.FootOffset;
            Player.Body.MovePosition(point);
            Player.SetFacing(tangent.x >= 0f ? 1 : -1);
            Player.SetVisualTilt(Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg * s.slideTiltAmount);
        }

        public override void Exit()
        {
            Player.Body.bodyType = RigidbodyType2D.Dynamic;
            Player.SetVisualTilt(0f);
        }
    }
}
