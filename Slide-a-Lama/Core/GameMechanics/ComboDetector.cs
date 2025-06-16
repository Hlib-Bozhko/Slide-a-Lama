using System;

namespace Slide_a_Lama.Core
{
    [Serializable]
    public class ComboDetector : IComboDetector
    {
        private readonly GameSettings _settings;
        
        public ComboDetector(GameSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }
        
        public ComboResult FindAndRemoveCombo(IGameBoard board)
        {
            var combo = FindCombo(board);
            if (combo.Found)
            {
                RemoveCombo(board, combo);
            }
            return combo;
        }
        
        public ComboResult FindCombo(IGameBoard board)
        {
            // Vertical combinations first
            var verticalCombo = FindVerticalCombo(board);
            if (verticalCombo.Found)
                return verticalCombo;
                
            // Horizontal combinations
            var horizontalCombo = FindHorizontalCombo(board);
            if (horizontalCombo.Found)
                return horizontalCombo;
                
            return ComboResult.NotFound;
        }
        
        public void RemoveCombo(IGameBoard board, ComboResult combo)
        {
            if (!combo.Found)
                return;
                
            foreach (var position in combo.Positions)
            {
                var cube = board.GetCube(position);
                if (cube != null)
                    cube.Value = 0;
            }
        }
        
        public bool HasPotentialCombos(IGameBoard board)
        {
            return FindCombo(board).Found;
        }
        
        #region Search of vertical combinations
        
        private ComboResult FindVerticalCombo(IGameBoard board)
        {
            for (int row = _settings.PlayableRowStart; row <= _settings.RowCount - 3; row++)
            {
                for (int column = _settings.PlayableColumnStart; column < _settings.PlayableColumnEnd; column++)
                {
                    var pos1 = new Position(row, column);
                    var pos2 = new Position(row + 1, column);
                    var pos3 = new Position(row + 2, column);
                    
                    var cube1 = board.GetCube(pos1);
                    var cube2 = board.GetCube(pos2);
                    var cube3 = board.GetCube(pos3);
                    
                    if (IsValidCombo(cube1, cube2, cube3))
                    {
                        return ComboResult.Create(cube1.Value, ComboType.Vertical, pos1, pos2, pos3);
                    }
                }
            }
            
            return ComboResult.NotFound;
        }
        
        #endregion
        
        #region Search of horizontal combinations
        
        private ComboResult FindHorizontalCombo(IGameBoard board)
        {
            for (int row = _settings.PlayableRowStart; row < _settings.PlayableRowEnd; row++)
            {
                for (int column = _settings.PlayableColumnStart; column <= _settings.PlayableColumnEnd - 3; column++)
                {
                    var pos1 = new Position(row, column);
                    var pos2 = new Position(row, column + 1);
                    var pos3 = new Position(row, column + 2);
                    
                    var cube1 = board.GetCube(pos1);
                    var cube2 = board.GetCube(pos2);
                    var cube3 = board.GetCube(pos3);
                    
                    if (IsValidCombo(cube1, cube2, cube3))
                    {
                        return ComboResult.Create(cube1.Value, ComboType.Horizontal, pos1, pos2, pos3);
                    }
                }
            }
            
            return ComboResult.NotFound;
        }
        
        #endregion
        
        #region Auxiliary methods
        
        private static bool IsValidCombo(Cube cube1, Cube cube2, Cube cube3)
        {
            if (cube1 == null || cube2 == null || cube3 == null)
                return false;
                
            return cube1.Value != 0 && 
                   cube1.Value == cube2.Value && 
                   cube2.Value == cube3.Value;
        }
        
        #endregion
    }
}

