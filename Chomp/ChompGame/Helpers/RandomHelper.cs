using System;
using System.Collections.Generic;
using System.Text;

namespace ChompGame.Helpers
{
    class RandomHelper
    {
        private Random _rng;

        public RandomHelper(int seed)
        {
            _rng = new Random(seed);
        }

        public T RandomItem<T>(params T[] choices)
        {
            return choices[_rng.Next(choices.Length)];
        }

    }
}
