
//Facade for a new architecture
using System;
using System.Collections.Generic;
using System.Linq;

namespace Slide_a_Lama.Core
{

    [Serializable]
    public class FieldAdapter
    {
        #region Private fields 
        
        private readonly IGame _game;
        private readonly GameSettings _settings;
        
        #endregion
        
        #region Public API 
        
        /// <summary>Player array</summary>
        public Player[] Players => _game.Players.ToArray();
        
        /// <summary>Current position of the active cube [row, column]</summary>
        public int[] CurrentCube => new[] { CurrentCubeRow, CurrentCubeColumn };
        
        /// <summary>Menu return flag</summary>
        public bool ToMenu { get; set; }
        
        /// <summary>Number of players</summary>
        public int PlayersCount => _game.Players.Count;
        
        /// <summary>Current player</summary>
        public Player CurrentPlayer => _game.CurrentPlayer;
        
        /// <summary>State of the game</summary>
        public GameState GameState => _game.GameState;
        
        /// <summary>Number of rows (including borders)</summary>
        public int RowCount => _settings.RowCount;
        
        /// <summary>Number of columns (including borders)</summary>
        public int ColumnCount => _settings.ColumnCount;
        
        // Auxiliary properties for CurrentCube
        private int CurrentCubeRow => _game.CurrentCubePosition.IsValid ? _game.CurrentCubePosition.Row : -1;
        private int CurrentCubeColumn => _game.CurrentCubePosition.IsValid ? _game.CurrentCubePosition.Column : -1;
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Main constructor 
        /// </summary>
        public FieldAdapter(int rowCount, int columnCount, int playersCount, int winScore)
        {
            _settings = new GameSettings(rowCount, columnCount, playersCount, winScore);
            _game = new Game(_settings);
        }
        
        /// <summary>
        /// Builder for dependency injection (for testing)
        /// </summary>
        internal FieldAdapter(IGame game, GameSettings settings)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }
        
        #endregion
        
        #region Key game methods
        
        /// <summary>Add a new cube</summary>
        public void AddCube()
        {
            _game.AddCube();
        }
        
        /// <summary>Move the current cube to the position</summary>
        public void MoveCurCube(int row, int column)
        {
            var position = new Position(row, column);
            _game.MoveCurCube(position);
        }
        
        /// <summary>Place the cube (run physics)</summary>
        public void PutCube()
        {
            _game.PutCube();
        }
        
        /// <summary>Update the field (physics + combinations)</summary>
        public bool UpdateField()
        {
            return _game.UpdateField();
        }
        
        #endregion
        
        #region Methods of information
        
        /// <summary>Get the cube in position</summary>
        public Cube GetCube(int row, int column)
        {
            var position = new Position(row, column);
            return _game.GetCube(position);
        }
        
        /// <summary>Is there an active cube</summary>
        public bool HasActiveCube()
        {
            return _game.HasActiveCube();
        }
        
        /// <summary>Can the cube be moved to a position</summary>
        public bool CanMoveTo(int row, int column)
        {
            var position = new Position(row, column);
            return _game.CanMoveTo(position);
        }
        
        /// <summary>Can the current cube be placed</summary>
        public bool CanPutCube()
        {
            return _game.CanPutCube();
        }
        
        /// <summary>Get the player's current account</summary>
        public int GetScore()
        {
            return _game.GetScore();
        }
        
        /// <summary>Get the value of the current cube</summary>
        public int GetCurrentCubeValue()
        {
            return _game.GetCurrentCubeValue();
        }
        
        #endregion
        
        #region Utilitarian methods
        
        /// <summary>Force all the cubes down</summary>
        public void ForceDropAllCubes()
        {
            _game.ForceDropAllCubes();
        }
        
        /// <summary>Check the integrity of the field</summary>
        public bool ValidateFieldIntegrity()
        {
            return _game.ValidateFieldIntegrity();
        }
        
        /// <summary>Debugging field output to the console</summary>
        public void DebugPrintField()
        {
            _game.DebugPrintField();
        }
        
        /// <summary>Get the list of allowed moves</summary>
        public List<(int row, int column)> GetValidMoves()
        {
            var moves = _game.GetValidMoves();
            return moves.Select(move => (move.Row, move.Column)).ToList();
        }
        
        #endregion
        
        #region Movement methods (for ConsoleUi)
        
        /// <summary>Movement to the right</summary>
        public void MoveRight(int row)
        {
            if (row == -1) return;
            
            var currentColumn = CurrentCube[1];
            if (currentColumn >= 0 && currentColumn < ColumnCount - 1)
            {
                var newPosition = new Position(row, currentColumn + 1);
                if (_game.CanMoveTo(newPosition))
                {
                    _game.MoveCurCube(newPosition);
                }
            }
        }
        
        /// <summary>Movement to the left</summary>
        public void MoveLeft(int row)
        {
            if (row == -1) return;
            
            var currentColumn = CurrentCube[1];
            if (currentColumn > 0)
            {
                var newPosition = new Position(row, currentColumn - 1);
                if (_game.CanMoveTo(newPosition))
                {
                    _game.MoveCurCube(newPosition);
                }
            }
        }
        
        /// <summary>Downward movement</summary>
        public void MoveDown(int column)
        {
            if (column == -1) return;
            
            var currentRow = CurrentCube[0];
            if (currentRow >= 0 && currentRow < RowCount - 1)
            {
                var newPosition = new Position(currentRow + 1, column);
                if (_game.CanMoveTo(newPosition))
                {
                    _game.MoveCurCube(newPosition);
                }
            }
        }
        
        /// <summary>Upward movement</summary>
        public void MoveUp(int column)
        {
            if (column == -1) return;
            
            var currentRow = CurrentCube[0];
            if (currentRow > 0)
            {
                var newPosition = new Position(currentRow - 1, column);
                if (_game.CanMoveTo(newPosition))
                {
                    _game.MoveCurCube(newPosition);
                }
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Factory for creating FieldAdapter
    /// </summary>
    public static class FieldAdapterFactory
    {
        /// <summary>
        /// Create a new game with standard components
        /// </summary>
        public static FieldAdapter CreateField(int rowCount, int columnCount, int playersCount, int winScore)
        {
            return new FieldAdapter(rowCount, columnCount, playersCount, winScore);
        }
        
        /// <summary>
        /// Create a game with implemented dependencies (for testing)
        /// </summary>
        internal static FieldAdapter CreateFieldWithDependencies(IGame game, GameSettings settings)
        {
            return new FieldAdapter(game, settings);
        }
        
        /// <summary>
        /// Create a game with default settings
        /// </summary>
        public static FieldAdapter CreateDefaultField()
        {
            return CreateField(8, 8, 2, 1000);
        }
    }
}

