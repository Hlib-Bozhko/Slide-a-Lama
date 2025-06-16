using System;

namespace Slide_a_Lama.Core
{
    [Serializable]
    public class GameSettings
    {
        public const int MaxIterations = 50;
        public const int MaxCubeValue = 6;
        public const int MinCubeValue = 1;
        public const int TurnPenalty = 5;
        public const int ComboMultiplier = 10;
        
        // The main parameters of the game
        public int OriginalRowCount { get; }
        public int OriginalColumnCount { get; }
        public int PlayersCount { get; }
        public int WinScore { get; }
        
        // field parameters 
        public int RowCount { get; }
        public int ColumnCount { get; }
        
        public int PlayableRowStart { get; }
        public int PlayableRowEnd { get; }
        public int PlayableColumnStart { get; }
        public int PlayableColumnEnd { get; }
        
        public GameSettings(int rowCount, int columnCount, int playersCount, int winScore)
        {
            if (rowCount <= 0 || columnCount <= 0 || playersCount <= 0 || winScore <= 0)
                throw new ArgumentException("All parameters must be positive");
                
            OriginalRowCount = rowCount;
            OriginalColumnCount = columnCount;
            PlayersCount = playersCount;
            WinScore = winScore;
            
            ColumnCount = columnCount + 2; 
            RowCount = rowCount + 1; 
            
            PlayableRowStart = 1;
            PlayableRowEnd = RowCount; 
            PlayableColumnStart = 1;
            PlayableColumnEnd = ColumnCount - 1;
        }
        
        public bool IsValidPosition(Position position)
        {
            return position.Row >= 0 && position.Row < RowCount && 
                   position.Column >= 0 && position.Column < ColumnCount;
        }
        
        public bool IsPlayablePosition(Position position)
        {
            return position.Row >= PlayableRowStart && position.Row < PlayableRowEnd && 
                   position.Column >= PlayableColumnStart && position.Column < PlayableColumnEnd;
        }
        
        public bool IsCornerPosition(Position position)
        {
            return (position.Row == 0 && position.Column == 0) ||                           // Top left corner
                   (position.Row == 0 && position.Column == ColumnCount - 1) ||             // Top right corner
                   (position.Row == RowCount - 1 && position.Column == 0) ||                // Lower left corner  
                   (position.Row == RowCount - 1 && position.Column == ColumnCount - 1);    // Lower right corner
        }
        
        public bool IsBorderPosition(Position position)
        {
            if (IsCornerPosition(position))
                return false;
        
            return (position.Row == 0) || 
                   (position.Column == 0) || 
                   (position.Column == ColumnCount - 1); 
        }
        
        public Position GetCenterPosition()
        {
            return new Position(0, (ColumnCount - 1) / 2);
        }
    }
}

