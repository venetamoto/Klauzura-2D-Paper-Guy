using System;

namespace PaperGuy.Player
{
    public class PlayerStateMachine
    {
        public PlayerState Current { get; private set; }

        public event Action<PlayerState, PlayerState> StateChanged;

        public void ChangeState(PlayerState next)
        {
            if (next == null || next == Current) return;

            PlayerState previous = Current;
            previous?.Exit();
            Current = next;
            Current.Enter();
            StateChanged?.Invoke(previous, Current);
        }

        public void Tick(float dt) => Current?.Tick(dt);
        public void FixedTick(float dt) => Current?.FixedTick(dt);
    }
}
