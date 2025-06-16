
using System;

namespace Slide_a_Lama.Core
{
    [Serializable]
    public class CubePhysics : ICubePhysics
    {
        private readonly GameSettings _settings;
        
        public CubePhysics(GameSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }
        
        public virtual bool DropAllCubes(IGameBoard board) 
        {
            bool anyMoved = false;

            for (int column = _settings.PlayableColumnStart; column < _settings.PlayableColumnEnd; column++)
            {
                if (DropCubesInColumn(board, column))
                    anyMoved = true;
            }

            return anyMoved;
        }
        
        public bool DropCubesInColumn(IGameBoard board, int column)
        {
            bool hasMoved = false;
            
            int writePos = _settings.RowCount - 1; 
            
            for (int readPos = _settings.RowCount - 1; readPos >= _settings.PlayableRowStart; readPos--)
            {
                var readPosition = new Position(readPos, column);
                var cube = board.GetCube(readPosition);
                
                if (cube.Value != 0)
                {
                    if (readPos != writePos)
                    {
                        var writePosition = new Position(writePos, column);
                        var targetCube = board.GetCube(writePosition);
                        targetCube.Value = cube.Value;
                        cube.Value = 0;
                        hasMoved = true;
                    }
                    writePos--; 
                }
            }
            
            return hasMoved;
        }
        
        public void ForceDropAllCubes(IGameBoard board)
        {
            bool changed;
            int iterations = 0;
            const int maxIterations = 20;
            
            do
            {
                changed = DropAllCubes(board);
                iterations++;
            } while (changed && iterations < maxIterations);
        }
    }
}

