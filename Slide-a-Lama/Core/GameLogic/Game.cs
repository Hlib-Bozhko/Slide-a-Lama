using System;
using System.Collections.Generic;

namespace Slide_a_Lama.Core
{
    [Serializable]
    public class Game : IGame
    {
        #region Components
        
        private readonly GameSettings _settings;
        private readonly IGameBoard _board;
        private readonly IComboDetector _comboDetector;
        private readonly ICubePhysics _physics;
        private readonly IPlayerManager _playerManager;
        private readonly IActiveCubeManager _activeCubeManager;
        private readonly ICubeMovement _cubeMovement;
        private readonly IRandomGenerator _randomGenerator;
        
        #endregion
        
        #region Properties
        
        public GameState GameState { get; private set; }
        public GameSettings Settings => _settings;
        public Position CurrentCubePosition => _activeCubeManager.CurrentPosition;
        public Player CurrentPlayer => _playerManager.CurrentPlayer;
        public IReadOnlyList<Player> Players => _playerManager.Players;
        public bool ToMenu { get; set; }
        
        #endregion
        
        #region Constructor 
        
        public Game(int rowCount, int columnCount, int playersCount, int winScore)
            : this(new GameSettings(rowCount, columnCount, playersCount, winScore))
        {
        }
        
        public Game(GameSettings settings)
            : this(settings, new RandomGenerator())
        {
        }
        
        public Game(GameSettings settings, IRandomGenerator randomGenerator)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _randomGenerator = randomGenerator ?? throw new ArgumentNullException(nameof(randomGenerator));
            
            _board = new GameBoard(_settings);
            _comboDetector = new ComboDetector(_settings);
            _physics = new CubePhysics(_settings);
            _playerManager = new PlayerManager(_settings.PlayersCount);
            _activeCubeManager = new ActiveCubeManager(_settings);
            _cubeMovement = new CubeMovement(_settings);
            
            // Initialize the game
            Initialize();
        }
        
        // (for testing)
        public Game(
            GameSettings settings,
            IGameBoard board,
            IComboDetector comboDetector,
            ICubePhysics physics,
            IPlayerManager playerManager,
            IActiveCubeManager activeCubeManager,
            ICubeMovement cubeMovement,
            IRandomGenerator randomGenerator)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _comboDetector = comboDetector ?? throw new ArgumentNullException(nameof(comboDetector));
            _physics = physics ?? throw new ArgumentNullException(nameof(physics));
            _playerManager = playerManager ?? throw new ArgumentNullException(nameof(playerManager));
            _activeCubeManager = activeCubeManager ?? throw new ArgumentNullException(nameof(activeCubeManager));
            _cubeMovement = cubeMovement ?? throw new ArgumentNullException(nameof(cubeMovement));
            _randomGenerator = randomGenerator ?? throw new ArgumentNullException(nameof(randomGenerator));
            
            GameState = GameState.PLAYING;
        }
        
        #endregion
        
        #region Main game actions
        
        public void AddCube()
        {
            if (_activeCubeManager.HasActiveCube)
            {
                _playerManager.SwitchToNextPlayer();
            }

            _activeCubeManager.AddNewCube(_board, _randomGenerator);
            _playerManager.AddTurn();
        }
        
        public bool MoveCurCube(Position newPosition)
        {
            return _activeCubeManager.MoveTo(_board, newPosition);
        }
        
        public void PutCube()
        {
            _activeCubeManager.PutCube(_board, _cubeMovement);
        }
        
        public bool UpdateField()
        {
            bool hasChanges = false;
            
            for (int i = 0; i < GameSettings.MaxIterations; i++)
            {
                bool iterationHadChanges = false;
                
                // dropping all the cubes
                bool cubesDropped = _physics.DropAllCubes(_board);
                if (cubesDropped)
                {
                    iterationHadChanges = true;
                }
                
                // find and delete combinations
                var combo = _comboDetector.FindAndRemoveCombo(_board);
                if (combo.Found)
                {
                    iterationHadChanges = true;
                    hasChanges = true;
                    ProcessCombo(combo);
                }
                
                if (!iterationHadChanges)
                    break;
            }

            return hasChanges;
        }
        
        #endregion
        
        #region Utilities
        
        public Cube GetCube(Position position)
        {
            return _board.GetCube(position);
        }
        
        public int GetScore()
        {
            return _playerManager.GetCurrentScore();
        }
        
        public bool HasActiveCube()
        {
            if (!_activeCubeManager.HasActiveCube)
                return false;
                
            var cube = _board.GetCube(_activeCubeManager.CurrentPosition);
            return cube?.Value > 0;
        }
        
        public int GetCurrentCubeValue()
        {
            if (!_activeCubeManager.HasActiveCube)
                return 0;
                
            var cube = _board.GetCube(_activeCubeManager.CurrentPosition);
            return cube?.Value ?? 0;
        }
        
        public bool CanMoveTo(Position position)
        {
            return _board.CanMoveTo(position);
        }
        
        public bool CanPutCube()
        {
            return _activeCubeManager.CanPutCube(_board);
        }
        
        public List<Position> GetValidMoves()
        {
            return _board.GetValidMoves();
        }
        
        #endregion
        
        #region Debugging and diagnostics
        
        public void DebugPrintField()
        {
            _board.DebugPrint();
            Console.WriteLine("Current Cube: {0} = {1}", 
                CurrentCubePosition, GetCurrentCubeValue());
            Console.WriteLine("Current Player: {0}, Score: {1}", 
                CurrentPlayer.Team, GetScore());
            Console.WriteLine("---");
        }
        
        public bool ValidateFieldIntegrity()
        {
            return _board.ValidateIntegrity();
        }
        
        public void ForceDropAllCubes()
        {
            _physics.ForceDropAllCubes(_board);
        }
        
        #endregion
        
        #region Private methods
        
        private void Initialize()
        {
            _board.Initialize(_randomGenerator);
            GameState = GameState.PLAYING;
            
            StabilizeField();
            _playerManager.ResetCurrentPlayerStats();
        }
        
        private void StabilizeField()
        {
            while (UpdateField())
            {
                _board.FillEmptyCells(_randomGenerator);
            }
        }
        
        private void ProcessCombo(ComboResult combo)
        {
            int points = combo.Value * GameSettings.ComboMultiplier;
            _playerManager.AddScore(points);

            if (_playerManager.CheckWinCondition(_settings.WinScore))
            {
                GameState = GameState.WIN;
            }
        }
        
        #endregion
    }
}

