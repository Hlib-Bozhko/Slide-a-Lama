using System.Collections.Generic;

namespace Slide_a_Lama.Core
{
    public interface IPlayerManager
    {
        Player CurrentPlayer { get; }
        IReadOnlyList<Player> Players { get; }
        int PlayersCount { get; }
        
        void SwitchToNextPlayer();
        void AddScore(int points);
        void AddTurn();
        int GetCurrentScore();
        bool CheckWinCondition(int winScore);
        void ResetCurrentPlayerStats();
    }
}



