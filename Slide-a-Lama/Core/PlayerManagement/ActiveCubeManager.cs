
using System;

namespace Slide_a_Lama.Core
{
    [Serializable]
    public class ActiveCubeManager : IActiveCubeManager
    {
        private readonly GameSettings _settings;
        private Position _currentPosition;
        
        public Position CurrentPosition => _currentPosition;
        public bool HasActiveCube => _currentPosition.IsValid;
        
        // public int CurrentCubeValue
        // {
        //     get
        //     {
        //         return 0;
        //     }
        // }
        
        public ActiveCubeManager(GameSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _currentPosition = Position.Invalid;
        }
        
        public void AddNewCube(IGameBoard board, IRandomGenerator randomGenerator)
        {
            var centerPosition = _settings.GetCenterPosition();
            
            var cube = board.GetCube(centerPosition);
            if (cube == null)
            {
                board.SetCube(centerPosition, new Cube(0));
                cube = board.GetCube(centerPosition);
            }
                
            cube.Value = randomGenerator.Next(GameSettings.MinCubeValue, GameSettings.MaxCubeValue + 1);
            _currentPosition = centerPosition;
        }
        
        public bool MoveTo(IGameBoard board, Position newPosition)
        {
            if (!HasActiveCube || !board.CanMoveTo(newPosition))
                return false;

            var currentCube = board.GetCube(_currentPosition);
            if (currentCube == null || currentCube.Value == 0) 
                return false;

            var targetCube = board.GetCube(newPosition);
            if (targetCube == null)
            {
                board.SetCube(newPosition, new Cube(0));
                targetCube = board.GetCube(newPosition);
            }

            targetCube.Value = currentCube.Value;
            currentCube.Value = 0;
            _currentPosition = newPosition;
            
            return true;
        }
        
        // Can be placed in borders (but not in corners)
        public bool CanPutCube(IGameBoard board)
        {
            if (!HasActiveCube)
                return false;
                
            var pos = _currentPosition;
            
            if (board.IsCornerPosition(pos))
                return false;
                
            return (pos.Row == 0) || 
                   (pos.Column == 0 && pos.Row != 0) || 
                   (pos.Column == _settings.ColumnCount - 1 && pos.Row != 0);
        }
        
        public void PutCube(IGameBoard board, ICubeMovement cubeMovement)
        {
            if (!CanPutCube(board))
                return;
                
            var pos = _currentPosition;
            
            if (board.IsCornerPosition(pos))
                return;
            
            switch ((pos.Row, pos.Column))
            {
                case (0, _):
                    cubeMovement.MoveDown(board, pos);
                    break;
                case (_, 0) when pos.Row != 0:
                    cubeMovement.MoveRight(board, pos);
                    break;
                case var (r, c) when c == _settings.ColumnCount - 1 && r != 0:
                    cubeMovement.MoveLeft(board, pos);
                    break;
            }
        }
        
        public void ResetPosition()
        {
            _currentPosition = Position.Invalid;
        }

        public void SetPositionForTesting(Position position)
        {
            _currentPosition = position;
        }
    }
}

