using System;
using System.Collections.Generic;
using System.Linq;

namespace Slide_a_Lama.Core
{
    [Serializable]
    public class PlayerManager : IPlayerManager
    {
        private readonly Player[] _players;
        private int _currentPlayerIndex;
        
        public Player CurrentPlayer => _players[_currentPlayerIndex];
        public IReadOnlyList<Player> Players => Array.AsReadOnly(_players);
        public int PlayersCount => _players.Length;
        
        public PlayerManager(int playersCount)
        {
            if (playersCount <= 0)
                throw new ArgumentException("Players count must be positive", nameof(playersCount));
                
            _players = new Player[playersCount];
            InitializePlayers();
            _currentPlayerIndex = 0;
        }
        
        public void SwitchToNextPlayer()
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % PlayersCount;
        }
        
        public void AddScore(int points)
        {
            CurrentPlayer.Score += points;
        }
        
        public void AddTurn()
        {
            CurrentPlayer.Turns++;
        }
        
        public int GetCurrentScore()
        {
            return Math.Max(0, CurrentPlayer.Score - CurrentPlayer.Turns * GameSettings.TurnPenalty);
        }
        
        public bool CheckWinCondition(int winScore)
        {
            return CurrentPlayer.Score >= winScore;
        }
        
        public void ResetCurrentPlayerStats()
        {
            CurrentPlayer.Score = 0;
            CurrentPlayer.Turns = 0;
        }
        
        private void InitializePlayers()
        {
            for (int i = 0; i < PlayersCount; i++)
            {
                _players[i] = new Player(i + 1);
            }
        }
    }
}

