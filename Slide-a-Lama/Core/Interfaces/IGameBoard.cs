using System.Collections.Generic;

namespace Slide_a_Lama.Core
{
    public interface IGameBoard
    {
        GameSettings Settings { get; }
        
        // Basic operations with cubes
        Cube GetCube(Position position);
        void SetCube(Position position, Cube cube);
        bool IsEmptyCell(Position position);
        
        // Position validation
        bool IsValidPosition(Position position);
        bool IsPlayablePosition(Position position);
        bool IsCornerPosition(Position position);
        bool IsBorderPosition(Position position);
        
        // Obtaining permissible moves
        List<Position> GetValidMoves();
        bool CanMoveTo(Position position);
        
        // Utilities
        void FillEmptyCells(IRandomGenerator randomGenerator);
        bool ValidateIntegrity();
        void DebugPrint();
        
        // Initialization
        void Initialize(IRandomGenerator randomGenerator);
    }
}

