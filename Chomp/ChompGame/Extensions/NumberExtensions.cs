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
        public static bool IsMod(this int i, int mod)
          => (i % mod) == 0;

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

        public static int Clamp(this int i, int min, int max)
        {
            if (i < min)
                return min;
            if (i > max)
                return max;
            else
                return i;
        }

        public static byte Clamp(this byte i, byte min, byte max)
        {
            if (i < min)
                return min;
            if (i > max)
                return max;
            else
                return i;
        }

        public static byte ByteClamp(this int i, byte max) =>
            (byte)i.Clamp(0, max);

        public static byte ByteClamp(this int i, int max) =>
          (byte)i.Clamp(0, max);

        public static byte ByteClamp(this byte i, byte max) =>
           i.Clamp(0, max);

        public static byte ByteClamp(this byte i, int max) =>
          (byte)i.Clamp(0, (byte)max);
    }
}
