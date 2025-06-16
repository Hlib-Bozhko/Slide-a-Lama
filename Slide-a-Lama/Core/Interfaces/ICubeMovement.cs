namespace Slide_a_Lama.Core
{
    public interface ICubeMovement
    {
        void MoveLeft(IGameBoard board, Position currentPosition);
        void MoveRight(IGameBoard board, Position currentPosition);
        void MoveDown(IGameBoard board, Position currentPosition);
        void MoveUp(IGameBoard board, Position currentPosition);
    }
}

