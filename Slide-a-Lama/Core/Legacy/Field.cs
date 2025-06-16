
using System;
using System.Collections.Generic;

namespace Slide_a_Lama.Core
{
    [Serializable]
    public class Field
    {
        private readonly Cube[,] _cubes;
        [NonSerialized]
        private Random _random;
        private readonly int _winScore;
        
        private readonly int _playableRowStart = 1;
        private readonly int _playableRowEnd;
        private readonly int _playableColumnStart = 1;
        private readonly int _playableColumnEnd;
        
        public readonly Player[] Players;
        public readonly int[] CurrentCube = {-1, -1};
        
        public bool ToMenu { get; set; }
        public int PlayersCount { get; }
        public Player CurrentPlayer { get; private set; }
        public GameState GameState { get; private set; }
        public int RowCount { get; }
        public int ColumnCount { get; }

        public Field(int rowCount, int columnCount, int playersCount, int winScore)
        {
            if (rowCount <= 0 || columnCount <= 0 || playersCount <= 0 || winScore <= 0)
                throw new ArgumentException("All parameters must be positive");

            ColumnCount = columnCount + 2; 
            RowCount = rowCount + 1; 
            PlayersCount = playersCount;
            _winScore = winScore;
            
            _playableRowEnd = RowCount;
            _playableColumnEnd = ColumnCount - 1;
            
            _cubes = new Cube[RowCount, ColumnCount];
            Players = new Player[playersCount];
            GameState = GameState.PLAYING;
            
            EnsureRandomInitialized();
            Initialize();
        }

        #region Public Methods

        public void MoveCurCube(int row, int column)
        {
            if (!IsValidPosition(row, column) || !IsEmptyCell(row, column))
                return;

            var currentCube = GetCurrentCube();
            if (currentCube == null || currentCube.Value == 0) 
                return;

            if (_cubes[row, column] == null)
                _cubes[row, column] = new Cube(0);

            _cubes[row, column].Value = currentCube.Value;
            currentCube.Value = 0;
            SetCurrentCubePosition(row, column);
        }

        public void PutCube()
        {
            var (row, column) = (CurrentCube[0], CurrentCube[1]);
            
            if (IsCornerPosition(row, column))
            {
                return; 
            }
            
            switch ((row, column))
            {
                case (0, _):
                    MoveDown(column);
                    break;
                case (_, 0) when row != 0:
                    MoveRight(row);
                    break;
                case var (r, c) when c == ColumnCount - 1 && r != 0:
                    MoveLeft(row);
                    break;
            }
        }

        public void AddCube()
        {
            EnsureRandomInitialized();
            
            if (CurrentCube[0] != -1)
            {
                SwitchToNextPlayer();
            }

            int centerColumn = (ColumnCount - 1) / 2;
            
            if (_cubes[0, centerColumn] == null)
                _cubes[0, centerColumn] = new Cube(0);
                
            _cubes[0, centerColumn].Value = _random.Next(1, 7);
            
            SetCurrentCubePosition(0, centerColumn);
            CurrentPlayer.Turns++;
        }

        public bool UpdateField()
        {
            const int maxIterations = 50;
            bool hasChanges = false;
            
            for (int i = 0; i < maxIterations; i++)
            {
                bool iterationHadChanges = false;
                
                bool cubesDropped = DropAllCubes();
                if (cubesDropped)
                {
                    iterationHadChanges = true;
                }
                
                bool comboFound = DelCombo();
                if (comboFound)
                {
                    iterationHadChanges = true;
                    hasChanges = true;
                }
                
                if (!iterationHadChanges)
                    break;
            }

            return hasChanges;
        }

        public Cube GetCube(int row, int column)
        {
            if (!IsValidPosition(row, column))
                return null;
                
            if (_cubes[row, column] == null)
                _cubes[row, column] = new Cube(0);
                
            return _cubes[row, column];
        }

        public int GetScore()
        {
            return Math.Max(0, CurrentPlayer.Score - CurrentPlayer.Turns * 5);
        }

        public bool HasActiveCube()
        {
            return CurrentCube[0] != -1 && CurrentCube[1] != -1 && 
                   IsValidPosition(CurrentCube[0], CurrentCube[1]) &&
                   GetCube(CurrentCube[0], CurrentCube[1])?.Value > 0;
        }

        public int GetCurrentCubeValue()
        {
            var cube = GetCurrentCube();
            return cube?.Value ?? 0;
        }

        public bool CanMoveTo(int row, int column)
        {
            if (!IsValidPosition(row, column) || !IsEmptyCell(row, column))
                return false;

            if (IsCornerPosition(row, column))
                return false;
            
            
            bool isInPlayableArea = IsValidPlayablePosition(row, column);
            bool isTopBorder = (row == 0 && column > 0 && column < ColumnCount - 1);
            bool isLeftBorder = (column == 0 && row > 0 && row < RowCount - 1);
            bool isRightBorder = (column == ColumnCount - 1 && row > 0 && row < RowCount - 1);
            
            return isInPlayableArea || isTopBorder || isLeftBorder || isRightBorder;
        }

        public bool CanPutCube()
        {
            var (row, column) = (CurrentCube[0], CurrentCube[1]);
            
            if (IsCornerPosition(row, column))
                return false;
                
            if (!HasActiveCube())
                return false;
                
            return (row == 0) || (column == 0 && row != 0) || (column == ColumnCount - 1 && row != 0);
        }

        public void DebugPrintField()
        {
            for (int row = 0; row < RowCount; row++)
            {
                for (int column = 0; column < ColumnCount; column++)
                {
                    Console.Write(GetCube(row, column)?.Value ?? 0);
                    Console.Write(" ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("Current Cube: [{0},{1}] = {2}", 
                CurrentCube[0], CurrentCube[1], GetCurrentCubeValue());
            Console.WriteLine("---");
        }

        public bool ValidateFieldIntegrity()
        {
            for (int column = _playableColumnStart; column < _playableColumnEnd; column++)
            {
                bool foundEmpty = false;
                for (int row = RowCount - 1; row >= _playableRowStart; row--)
                {
                    var cube = GetCube(row, column);
                    if (cube.Value == 0)
                    {
                        foundEmpty = true;
                    }
                    else if (foundEmpty)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void ForceDropAllCubes()
        {
            bool changed;
            do
            {
                changed = DropAllCubes();
            } while (changed);
        }

        public List<(int row, int column)> GetValidMoves()
        {
            var validMoves = new List<(int row, int column)>();
            
            for (int row = 0; row < RowCount; row++)
            {
                for (int column = 0; column < ColumnCount; column++)
                {
                    if (CanMoveTo(row, column))
                    {
                        validMoves.Add((row, column));
                    }
                }
            }
            
            return validMoves;
        }

        #endregion

        #region Movement Methods

        public void MoveLeft(int row)
        {
            if (!CanMoveLeft(row)) return;

            CurrentCube[1]--;
            ShiftRowLeft(row);
        }

        public void MoveRight(int row)
        {
            if (!CanMoveRight(row)) return;

            CurrentCube[1]++;
            ShiftRowRight(row);
        }

        public void MoveDown(int column)
        {
            if (!CanMoveDown(column)) return;

            CurrentCube[0]++;
            ShiftColumnDown(column);
        }

        public void MoveUp(int column)
        {
            if (!CanMoveUp(column)) return;
                
            CurrentCube[0]--;
            ShiftColumnUp(column);
        }

        #endregion

        #region Private Methods - Initialization

        private void EnsureRandomInitialized()
        {
            if (_random == null)
                _random = new Random();
        }

        private void Initialize()
        {
            InitializeBorders();
            InitializePlayField();
            InitializePlayers();
            SetInitialPlayer();
            
            StabilizeField();
            ResetCurrentPlayerStats();
        }

        private void InitializePlayField()
        {
            EnsureRandomInitialized();
            
            for (int row = _playableRowStart; row < _playableRowEnd; row++)
            {
                for (int column = _playableColumnStart; column < _playableColumnEnd; column++)
                {
                    _cubes[row, column] = new Cube(_random.Next(1, 7));
                }
            }
        }

        private void InitializeBorders()
        {
            for (int row = 0; row < RowCount; row++)
            {
                _cubes[row, 0] = new Cube(0);
                _cubes[row, ColumnCount - 1] = new Cube(0);
            }
            
            for (int column = 0; column < ColumnCount; column++)
            {
                _cubes[0, column] = new Cube(0);
            }
        }

        private void InitializePlayers()
        {
            for (int i = 0; i < PlayersCount; i++)
            {
                Players[i] = new Player(i + 1);
            }
        }

        private void SetInitialPlayer()
        {
            CurrentPlayer = Players[0];
        }

        private void StabilizeField()
        {
            while (UpdateField())
            {
                FillEmptyCells();
            }
        }

        private void ResetCurrentPlayerStats()
        {
            CurrentPlayer.Score = 0;
            CurrentPlayer.Turns = 0;
        }

        #endregion

        #region Private Methods - Game logic

        private void SwitchToNextPlayer()
        {
            int currentIndex = CurrentPlayer.Team - 1;
            int nextIndex = (currentIndex + 1) % PlayersCount;
            CurrentPlayer = Players[nextIndex];
        }

        private void SetCurrentCubePosition(int row, int column)
        {
            CurrentCube[0] = row;
            CurrentCube[1] = column;
        }

        private Cube GetCurrentCube()
        {
            var (row, column) = (CurrentCube[0], CurrentCube[1]);
            return IsValidPosition(row, column) ? GetCube(row, column) : null;
        }

        #endregion

        #region Private Methods - Combinations 

        private bool DelCombo()
        {
            for (int iRow = _playableRowStart; iRow <= RowCount - 3; iRow++)
            {
                for (int iColumn = _playableColumnStart; iColumn < _playableColumnEnd; iColumn++)
                {
                    var cube1 = GetCube(iRow, iColumn);
                    var cube2 = GetCube(iRow + 1, iColumn);
                    var cube3 = GetCube(iRow + 2, iColumn);
                    
                    if (cube1.Value != 0 && cube1.Value == cube2.Value && cube2.Value == cube3.Value)
                    {
                        CheckState(cube1.Value);

                        cube1.Value = 0;
                        cube2.Value = 0;
                        cube3.Value = 0;
                        return true;
                    }
                }
            }

            for (int iRow = _playableRowStart; iRow < _playableRowEnd; iRow++)
            {
                for (int iColumn = _playableColumnStart; iColumn <= _playableColumnEnd - 3; iColumn++)
                {
                    var cube1 = GetCube(iRow, iColumn);
                    var cube2 = GetCube(iRow, iColumn + 1);
                    var cube3 = GetCube(iRow, iColumn + 2);
                    
                    if (cube1.Value != 0 && cube1.Value == cube2.Value && cube2.Value == cube3.Value)
                    {
                        CheckState(cube1.Value);

                        cube1.Value = 0;
                        cube2.Value = 0;
                        cube3.Value = 0;
                        return true;
                    }
                }
            }

            return false;
        }

        private void CheckState(int cubeValue)
        {
            CurrentPlayer.Score += cubeValue * 10;

            if (CurrentPlayer.Score >= _winScore)
            {
                GameState = GameState.WIN;
            }
        }

        #endregion

        #region Private Methods - Physics of fall 

        private bool DropAllCubes()
        {
            bool anyMoved = false;

            for (int column = _playableColumnStart; column < _playableColumnEnd; column++)
            {
                if (DropCubesInColumn(column))
                    anyMoved = true;
            }

            return anyMoved;
        }

        private bool DropCubesInColumn(int column)
        {
            bool hasMoved = false;
            
            int writePos = RowCount - 1; 
            
            for (int readPos = RowCount - 1; readPos >= _playableRowStart; readPos--)
            {
                var cube = GetCube(readPos, column);
                if (cube.Value != 0)
                {
                    if (readPos != writePos)
                    {
                        var targetCube = GetCube(writePos, column);
                        targetCube.Value = cube.Value;
                        cube.Value = 0;
                        hasMoved = true;
                    }
                    writePos--; 
                }
            }
            
            return hasMoved;
        }

        #endregion

        #region Private Methods - Movement

        private bool CanMoveLeft(int row) => row != -1 && CurrentCube[1] > 0;
        private bool CanMoveRight(int row) => row != -1 && CurrentCube[1] < ColumnCount - 1;
        private bool CanMoveDown(int column) => column != -1 && CurrentCube[0] < RowCount - 1;
        private bool CanMoveUp(int column) => column != -1 && CurrentCube[0] > 0;

        private void ShiftRowLeft(int row)
        {
            if (row == -1 || CurrentCube[1] == 0) return;

            int firstColumn = 0;
            if (row != 0)
            {
                for (int i = ColumnCount - 1; i >= 1; i--)
                {
                    if (GetCube(row, i).Value == 0)
                    {
                        firstColumn = i;
                    }
                }
            }

            for (int i = firstColumn; i < ColumnCount - 1; i++)
            {
                var currentCube = GetCube(row, i);
                var nextCube = GetCube(row, i + 1);
                currentCube.Value = nextCube.Value;
            }

            GetCube(row, ColumnCount - 1).Value = 0;
            if (row != 0)
            {
                GetCube(row, 0).Value = 0;
            }
        }

        private void ShiftRowRight(int row)
        {
            if (row == -1 || CurrentCube[1] == ColumnCount - 1) return;

            int firstColumn = ColumnCount - 2;
            if (row != 0)
            {
                for (int i = 1; i < ColumnCount - 2; i++)
                {
                    if (GetCube(row, i).Value == 0)
                    {
                        firstColumn = i - 1;
                    }
                }
            }

            for (int i = firstColumn; i >= 0; i--)
            {
                var targetCube = GetCube(row, i + 1);
                var sourceCube = GetCube(row, i);
                targetCube.Value = sourceCube.Value;
            }

            GetCube(row, 0).Value = 0;
            if (row != 0)
            {
                GetCube(row, ColumnCount - 1).Value = 0;
            }
        }

        private void ShiftColumnDown(int column)
        {
            if (column == -1 || CurrentCube[0] == RowCount - 1) return;

            int firstRow = RowCount - 1;
            if (column != 0 && column != ColumnCount - 1)
            {
                for (int i = 1; i <= RowCount - 1; i++)
                {
                    if (GetCube(i, column).Value == 0)
                    {
                        firstRow = i;
                    }
                }
            }

            for (int i = firstRow - 1; i >= 0; i--)
            {
                var targetCube = GetCube(i + 1, column);
                var sourceCube = GetCube(i, column);
                targetCube.Value = sourceCube.Value;
            }
            GetCube(0, column).Value = 0;
        }

        private void ShiftColumnUp(int column)
        {
            if (column == -1 || CurrentCube[0] == 0) return;

            for (int i = 0; i < RowCount - 1; i++)
            {
                var targetCube = GetCube(i, column);
                var sourceCube = GetCube(i + 1, column);
                targetCube.Value = sourceCube.Value;
            }

            GetCube(RowCount - 1, column).Value = 0;
        }

        #endregion

        #region Private Methods - Utilities

        private void FillEmptyCells()
        {
            EnsureRandomInitialized();
            
            for (int row = _playableRowStart; row < _playableRowEnd; row++)
            {
                for (int column = _playableColumnStart; column < _playableColumnEnd; column++)
                {
                    var cube = GetCube(row, column);
                    if (cube.Value == 0)
                    {
                        cube.Value = _random.Next(1, 7);
                    }
                }
            }
        }

        private bool IsValidPosition(int row, int column)
        {
            return row >= 0 && row < RowCount && column >= 0 && column < ColumnCount;
        }

        private bool IsValidPlayablePosition(int row, int column)
        {
            return row >= _playableRowStart && row < _playableRowEnd && 
                   column >= _playableColumnStart && column < _playableColumnEnd;
        }

        private bool IsEmptyCell(int row, int column)
        {
            if (!IsValidPosition(row, column))
                return false;
                
            var cube = GetCube(row, column);
            return cube?.Value == 0;
        }

        private bool IsCornerPosition(int row, int column)
        {
            return (row == 0 && column == 0) ||                           
                   (row == 0 && column == ColumnCount - 1) ||             
                   (row == RowCount - 1 && column == 0) ||                 
                   (row == RowCount - 1 && column == ColumnCount - 1);    
        }

        #endregion
    }
}

