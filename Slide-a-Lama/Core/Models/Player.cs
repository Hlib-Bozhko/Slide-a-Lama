
using System;

namespace Slide_a_Lama.Core
{
    [Serializable]
    public class Player
    {
        public Player(int team)
        {
            Team = team;
            Score = 0;
            Turns = 0;
        }

        public int Score { get; set; }
        public int Team { get; }

        public int Turns { get; set; }
    }
}

