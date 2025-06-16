
// Combination search result
using System;
using System.Collections.Generic;

namespace Slide_a_Lama.Core
{
    [Serializable]
    public class ComboResult
    {
        public bool Found { get; set; }
        public int Value { get; set; }
        public List<Position> Positions { get; set; } = new();
        public ComboType Type { get; set; }
        
        public static ComboResult NotFound => new() { Found = false };
        
        public static ComboResult Create(int value, ComboType type, params Position[] positions)
        {
            return new ComboResult
            {
                Found = true,
                Value = value,
                Type = type,
                Positions = new List<Position>(positions)
            };
        }
    }
    
    [Serializable]
    public enum ComboType
    {
        Horizontal,
        Vertical
    }
}

