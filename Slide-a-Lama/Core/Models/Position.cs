
// Structure for coordinates
using System;

namespace Slide_a_Lama.Core
{
    [Serializable]
    public readonly struct Position : IEquatable<Position>
    {
        public int Row { get; }
        public int Column { get; }
        
        public Position(int row, int column)
        {
            Row = row;
            Column = column;
        }
        
        public bool Equals(Position other)
        {
            return Row == other.Row && Column == other.Column;
        }
        
        public override bool Equals(object obj)
        {
            return obj is Position other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Column);
        }
        
        public static bool operator ==(Position left, Position right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(Position left, Position right)
        {
            return !left.Equals(right);
        }
        
        public override string ToString()
        {
            return $"({Row}, {Column})";
        }
        
        public static Position Invalid => new(-1, -1);
        
        public bool IsValid => Row >= 0 && Column >= 0;
    }
}





