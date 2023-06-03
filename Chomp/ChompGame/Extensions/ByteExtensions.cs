using ChompGame.Data;

namespace ChompGame.Extensions
{
    public static class ByteExtensions
    {
        public static bool IsMod(this byte b, byte mod)
            => (b % mod) == 0;

        public static bool IsMod(this GameByte b, byte mod)
            => (b.Value % mod) == 0;

        public static byte ParseByteSpecial(this char c)
        {
            return c switch
            {
                '!' => 16,
                '@' => 17,
                '#' => 18,
                '$' => 19,
                '%' => 20,
                '^' => 21,
                '&' => 22,
                '*' => 23,
                _ => byte.Parse(c.ToString(), System.Globalization.NumberStyles.HexNumber)
            };
        }

    }
}
