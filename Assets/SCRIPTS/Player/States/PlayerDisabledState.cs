namespace PaperGuy.Player
{
    /// <summary>Ovládání vypnuté: smrt nebo cutscéna. Tělo dál padá vlivem gravitace.</summary>
    public class PlayerDisabledState : PlayerState
    {
        public PlayerDisabledState(PlayerController2D player) : base(player) { }

        public bool IsDead { get; set; }

        public override PlayerAnimState AnimState => IsDead ? PlayerAnimState.Dead : PlayerAnimState.Idle;
        public override bool UsesBackLimit => !IsDead;

        public override void FixedTick(float dt) => Player.ApplyHorizontalMovement(0f, dt);
    }
}
