using Microsoft.Xna.Framework;

namespace ChompGame.Extensions
{
    public static class NumberExtensions
    {
        public static int Power(this int number, int power)
        {
            int result = 1;
            while(power-- > 0)
                result *= number;

            return result;
        }

        public static Point ToPoint(this int index, int columns)
        {
            int row = index / columns;
            int col = index % columns;
            return new Point(col, row);
        }

        public static int Wrap(this int number, int max)
        {
            while (number < 0)
                number += max;
            while (number >= max)
                number -= max;

            return number;
        }
    }
}
