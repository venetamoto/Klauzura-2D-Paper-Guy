using PaperGuy.Player;

namespace PaperGuy.Interactions
{
    /// <summary>Všechno, co postava může „sníst“.</summary>
    public interface ICollectable
    {
        /// <summary>Dá se to sníst právě teď (není snědeno, není zablokováno)?</summary>
        bool CanBeCollected { get; }

        /// <summary>Volá hráč v okamžiku snědení.</summary>
        void Collect(PlayerController2D eater);
    }
}
