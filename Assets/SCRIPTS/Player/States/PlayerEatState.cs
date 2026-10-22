namespace PaperGuy.Player
{
    public class PlayerEatState : PlayerState
    {
        private float _timer;

        public PlayerEatState(PlayerController2D player) : base(player) { }

        public override PlayerAnimState AnimState => PlayerAnimState.Eat;

        public override void Enter() => _timer = Player.EatDuration;

        public override void FixedTick(float dt)
        {
            Player.ApplyHorizontalMovement(0f, dt);
            Player.ApplyGravityModifiers();

            _timer -= dt;
            if (_timer <= 0f) Player.ReturnToLocomotion();
        }
    }
}
