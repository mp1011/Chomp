using Microsoft.Xna.Framework;

namespace ChompGame.Extensions
{
    public static class NumberExtensions
    {

        public static int NMod(this int number, int mod)
        {
            if (number >= 0)
                return number % mod;

            while (number < 0)
                number += mod;

            return number % mod;
        }

        public static byte NModByte(this int number, int mod)
        {
            return (byte)number.NMod(mod);
        }

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

        public static byte Toggle(this byte b, byte c1, byte c2)
        {
            if (b == c1)
                return c2;
            else
                return c1;
        }
    }
}
