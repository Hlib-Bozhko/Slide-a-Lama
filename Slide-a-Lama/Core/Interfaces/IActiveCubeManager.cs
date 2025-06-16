
namespace Slide_a_Lama.Core
{
    public interface IActiveCubeManager
    {
        Position CurrentPosition { get; }
        bool HasActiveCube { get; }
        // int CurrentCubeValue { get; }
        
        void AddNewCube(IGameBoard board, IRandomGenerator randomGenerator);
        bool MoveTo(IGameBoard board, Position newPosition);
        bool CanPutCube(IGameBoard board);
        void PutCube(IGameBoard board, ICubeMovement cubeMovement);
        void ResetPosition();
    }
}





