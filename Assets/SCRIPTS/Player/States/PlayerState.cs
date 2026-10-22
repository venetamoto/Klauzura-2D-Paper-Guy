namespace PaperGuy.Player
{
    public enum PlayerAnimState
    {
        Idle,
        Run,
        Jump,
        Fall,
        Slide,
        Eat,
        Dead
    }

    public abstract class PlayerState
    {
        protected readonly PlayerController2D Player;

        protected PlayerState(PlayerController2D player) => Player = player;

        public abstract PlayerAnimState AnimState { get; }

        public virtual bool UsesBackLimit => true;

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Tick(float deltaTime) { }
        public virtual void FixedTick(float fixedDeltaTime) { }
    }
}
