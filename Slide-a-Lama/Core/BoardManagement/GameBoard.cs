
using System;
using System.Collections.Generic;

namespace Slide_a_Lama.Core
{
    [Serializable]
    public class GameBoard : IGameBoard
    {
        private readonly Cube[,] _cubes;
        
        public GameSettings Settings { get; }
        
        public GameBoard(GameSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _cubes = new Cube[Settings.RowCount, Settings.ColumnCount];
        }
        
        #region Base operations with cubes
        
        public Cube GetCube(Position position)
        {
            if (!IsValidPosition(position))
                return null;
                
            if (_cubes[position.Row, position.Column] == null)
                _cubes[position.Row, position.Column] = new Cube(0);
                
            return _cubes[position.Row, position.Column];
        }
        
        public void SetCube(Position position, Cube cube)
        {
            if (IsValidPosition(position))
                _cubes[position.Row, position.Column] = cube;
        }
        
        public bool IsEmptyCell(Position position)
        {
            if (!IsValidPosition(position))
                return false;
                
            var cube = GetCube(position);
            return cube?.Value == 0;
        }
        
        #endregion
        
        #region Position validation
        
        public bool IsValidPosition(Position position)
        {
            return Settings.IsValidPosition(position);
        }
        
        public bool IsPlayablePosition(Position position)
        {
            return Settings.IsPlayablePosition(position);
        }
        
        public bool IsCornerPosition(Position position)
        {
            return Settings.IsCornerPosition(position);
        }
        
        public bool IsBorderPosition(Position position)
        {
            return Settings.IsBorderPosition(position);
        }
        
        public bool CanMoveTo(Position position)
        {
            if (!IsValidPosition(position))
                return false;
        
            if (!IsEmptyCell(position))
                return false;

            if (IsCornerPosition(position))
                return false;

            if (IsPlayablePosition(position))
                return true;
        
            if (IsBorderPosition(position))
                return true;
        
            return false;
        }
        
        #endregion
        
        #region Getting allowable moves
        
        public List<Position> GetValidMoves()
        {
            var validMoves = new List<Position>();
            
            for (int row = 0; row < Settings.RowCount; row++)
            {
                for (int column = 0; column < Settings.ColumnCount; column++)
                {
                    var position = new Position(row, column);
                    if (CanMoveTo(position))
                    {
                        validMoves.Add(position);
                    }
                }
            }
            
            return validMoves;
        }
        
        #endregion
        
        #region Utilities
        
        public void FillEmptyCells(IRandomGenerator randomGenerator)
        {
            for (int row = Settings.PlayableRowStart; row < Settings.PlayableRowEnd; row++)
            {
                for (int column = Settings.PlayableColumnStart; column < Settings.PlayableColumnEnd; column++)
                {
                    var position = new Position(row, column);
                    var cube = GetCube(position);
                    if (cube.Value == 0)
                    {
                        cube.Value = randomGenerator.Next(GameSettings.MinCubeValue, GameSettings.MaxCubeValue + 1);
                    }
                }
            }
        }
        
        public bool ValidateIntegrity()
        {
            for (int column = Settings.PlayableColumnStart; column < Settings.PlayableColumnEnd; column++)
            {
                bool foundEmpty = false;
                for (int row = Settings.RowCount - 1; row >= Settings.PlayableRowStart; row--)
                {
                    var cube = GetCube(new Position(row, column));
                    if (cube.Value == 0)
                    {
                        foundEmpty = true;
                    }
                    else if (foundEmpty)
                    {
                        // Found a cube over an empty space - violation of integrity
                        return false;
                    }
                }
            }
            return true;
        }
        
        public void DebugPrint()
        {
            Console.WriteLine("=== Game Board ===");
            for (int row = 0; row < Settings.RowCount; row++)
            {
                for (int column = 0; column < Settings.ColumnCount; column++)
                {
                    Console.Write(GetCube(new Position(row, column))?.Value ?? 0);
                    Console.Write(" ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("==================");
        }
        
        #endregion
        
        #region Initialization
        
        public void Initialize(IRandomGenerator randomGenerator)
        {
            InitializeBorders();
            InitializePlayField(randomGenerator);
        }
        
        private void InitializeBorders()
        {
            for (int row = 0; row < Settings.RowCount; row++)
            {
                _cubes[row, 0] = new Cube(0);
                _cubes[row, Settings.ColumnCount - 1] = new Cube(0);
            }
            
            for (int column = 0; column < Settings.ColumnCount; column++)
            {
                _cubes[0, column] = new Cube(0);
            }
        }
        
        private void InitializePlayField(IRandomGenerator randomGenerator)
        {
            for (int row = Settings.PlayableRowStart; row < Settings.PlayableRowEnd; row++)
            {
                for (int column = Settings.PlayableColumnStart; column < Settings.PlayableColumnEnd; column++)
                {
                     var position = new Position(row, column);
                    _cubes[position.Row, position.Column] = new Cube(
                        randomGenerator.Next(GameSettings.MinCubeValue, GameSettings.MaxCubeValue + 1));
                }
            }
        }
        
        #endregion
    }
}

