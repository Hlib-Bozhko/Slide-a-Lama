namespace Slide_a_Lama.Core
{
    public interface ICubePhysics
    {
        bool DropAllCubes(IGameBoard board);
        bool DropCubesInColumn(IGameBoard board, int column);
        void ForceDropAllCubes(IGameBoard board);
    }
}



