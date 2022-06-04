using System;

namespace Chomp.Extensions
{
    public static class NumberExtensions
    {
        public static int Pow2(this int number)
        {
            return (int)Math.Pow(2, number);
        }
    }
}
