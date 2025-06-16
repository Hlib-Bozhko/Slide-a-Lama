using System;

namespace Slide_a_Lama.Core
{
    [Serializable]
    public class CubeMovement : ICubeMovement
    {
        private readonly GameSettings _settings;
        
        public CubeMovement(GameSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }
        
        public void MoveLeft(IGameBoard board, Position currentPosition)
        {
            if (!CanMoveLeft(currentPosition)) 
                return;

            ShiftRowLeft(board, currentPosition.Row);
        }

        public void MoveRight(IGameBoard board, Position currentPosition)
        {
            if (!CanMoveRight(currentPosition)) 
                return;

            ShiftRowRight(board, currentPosition.Row);
        }

        public void MoveDown(IGameBoard board, Position currentPosition)
        {
            if (!CanMoveDown(currentPosition)) 
                return;

            ShiftColumnDown(board, currentPosition.Column);
        }

        public void MoveUp(IGameBoard board, Position currentPosition)
        {
            if (!CanMoveUp(currentPosition)) 
                return;
                
            ShiftColumnUp(board, currentPosition.Column);
        }
        
        #region Movement capability checks
        
        private bool CanMoveLeft(Position position) => position.Row != -1 && position.Column > 0;
        private bool CanMoveRight(Position position) => position.Row != -1 && position.Column < _settings.ColumnCount - 1;
        private bool CanMoveDown(Position position) => position.Column != -1 && position.Row < _settings.RowCount - 1;
        private bool CanMoveUp(Position position) => position.Column != -1 && position.Row > 0;
        
        #endregion
        
        #region Row and column shifts
        
        private void ShiftRowLeft(IGameBoard board, int row)
        {
            if (row == -1) return;

            int firstColumn = 0;
            if (row != 0)
            {
                for (int i = _settings.ColumnCount - 1; i >= 1; i--)
                {
                    if (board.GetCube(new Position(row, i)).Value == 0)
                    {
                        firstColumn = i;
                    }
                }
            }

            for (int i = firstColumn; i < _settings.ColumnCount - 1; i++)
            {
                var currentCube = board.GetCube(new Position(row, i));
                var nextCube = board.GetCube(new Position(row, i + 1));
                currentCube.Value = nextCube.Value;
            }

            board.GetCube(new Position(row, _settings.ColumnCount - 1)).Value = 0;
            if (row != 0)
            {
                board.GetCube(new Position(row, 0)).Value = 0;
            }
        }

        private void ShiftRowRight(IGameBoard board, int row)
        {
            if (row == -1) return;

            int firstColumn = _settings.ColumnCount - 2;
            if (row != 0)
            {
                for (int i = 1; i < _settings.ColumnCount - 2; i++)
                {
                    if (board.GetCube(new Position(row, i)).Value == 0)
                    {
                        firstColumn = i - 1;
                    }
                }
            }

            for (int i = firstColumn; i >= 0; i--)
            {
                var targetCube = board.GetCube(new Position(row, i + 1));
                var sourceCube = board.GetCube(new Position(row, i));
                targetCube.Value = sourceCube.Value;
            }

            board.GetCube(new Position(row, 0)).Value = 0;
            if (row != 0)
            {
                board.GetCube(new Position(row, _settings.ColumnCount - 1)).Value = 0;
            }
        }

        private void ShiftColumnDown(IGameBoard board, int column)
        {
            if (column == -1) return;

            int firstRow = _settings.RowCount - 1;
            if (column != 0 && column != _settings.ColumnCount - 1)
            {
                for (int i = 1; i <= _settings.RowCount - 1; i++)
                {
                    if (board.GetCube(new Position(i, column)).Value == 0)
                    {
                        firstRow = i;
                    }
                }
            }

            for (int i = firstRow - 1; i >= 0; i--)
            {
                var targetCube = board.GetCube(new Position(i + 1, column));
                var sourceCube = board.GetCube(new Position(i, column));
                targetCube.Value = sourceCube.Value;
            }
            board.GetCube(new Position(0, column)).Value = 0;
        }

        private void ShiftColumnUp(IGameBoard board, int column)
        {
            if (column == -1) return;

            for (int i = 0; i < _settings.RowCount - 1; i++)
            {
                var targetCube = board.GetCube(new Position(i, column));
                var sourceCube = board.GetCube(new Position(i + 1, column));
                targetCube.Value = sourceCube.Value;
            }

            board.GetCube(new Position(_settings.RowCount - 1, column)).Value = 0;
        }
        
        #endregion
    }
}

