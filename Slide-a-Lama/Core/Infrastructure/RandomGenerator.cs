
//To generate random numbers
using System;

namespace Slide_a_Lama.Core
{
    public interface IRandomGenerator
    {
        int Next(int minValue, int maxValue);
    }
    
    [Serializable]
    public class RandomGenerator : IRandomGenerator
    {
        [NonSerialized]
        private Random _random;
        
        public RandomGenerator()
        {
            EnsureInitialized();
        }
        
        public int Next(int minValue, int maxValue)
        {
            EnsureInitialized();
            return _random.Next(minValue, maxValue);
        }
        
        private void EnsureInitialized()
        {
            if (_random == null)
                _random = new Random();
        }
    }
}

