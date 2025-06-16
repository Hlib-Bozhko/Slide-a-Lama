namespace Slide_a_Lama.Core
{
    public interface IComboDetector
    {
        ComboResult FindAndRemoveCombo(IGameBoard board);
        ComboResult FindCombo(IGameBoard board);
        void RemoveCombo(IGameBoard board, ComboResult combo);
        bool HasPotentialCombos(IGameBoard board);
    }
}

