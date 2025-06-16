
using System;

namespace Slide_a_Lama.Core
{
    [Serializable]
    public class Cube
    {

        public Cube(int value)
        {
            Value = value;
        }

        public int Value { get; set; }
    }
}

