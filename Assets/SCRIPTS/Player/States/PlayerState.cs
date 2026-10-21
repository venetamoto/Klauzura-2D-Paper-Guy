namespace PaperGuy.Player
{
    /// <summary>Vizuální stavy postavy — jedna ku jedné se stavy v Animatoru.</summary>
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

    /// <summary>
    /// Základní logický stav hráče. Stavy jsou obyčejné C# třídy (ne MonoBehaviour):
    /// všechen „hardware“ (Rigidbody2D, vstup, nastavení) žije v <see cref="PlayerController2D"/>.
    /// </summary>
    public abstract class PlayerState
    {
        protected readonly PlayerController2D Player;

        protected PlayerState(PlayerController2D player) => Player = player;

        /// <summary>Jakou animaci v tomto stavu právě ukazovat.</summary>
        public abstract PlayerAnimState AnimState { get; }

        /// <summary>Uplatňovat omezení „ne vlevo od okraje kamery“?</summary>
        public virtual bool UsesBackLimit => true;

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Tick(float deltaTime) { }
        public virtual void FixedTick(float fixedDeltaTime) { }
    }
}
