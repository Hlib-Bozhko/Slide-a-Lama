using System.Collections.Generic;

namespace Slide_a_Lama.Core
{
    public interface IGame
    {
        // Game State Properties
        GameState GameState { get; }
        GameSettings Settings { get; }
        Position CurrentCubePosition { get; }
        Player CurrentPlayer { get; }
        IReadOnlyList<Player> Players { get; }
        bool ToMenu { get; set; }
        
        // Basic game actions
        void AddCube();
        bool MoveCurCube(Position newPosition);
        void PutCube();
        bool UpdateField();
        
        // Utilities
        Cube GetCube(Position position);
        int GetScore();
        bool HasActiveCube();
        int GetCurrentCubeValue();
        bool CanMoveTo(Position position);
        bool CanPutCube();
        List<Position> GetValidMoves();
        
        // Debugging and diagnostics
        void DebugPrintField();
        bool ValidateFieldIntegrity();
        void ForceDropAllCubes();
    }
}



