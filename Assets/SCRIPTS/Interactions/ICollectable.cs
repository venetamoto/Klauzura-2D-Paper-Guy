using PaperGuy.Player;

namespace PaperGuy.Interactions
{
    public interface ICollectable
    {
        bool CanBeCollected { get; }

        void Collect(PlayerController2D eater);
    }
}
