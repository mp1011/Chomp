using ChompGame.Data;

namespace ChompGame.Extensions
{
    public static class ByteExtensions
    {
        public static byte Mod(this byte b, byte mod) => (byte)(b % mod);
        public static byte Mod(this byte b, int mod) => (byte)(b % mod);

        public static bool IsMod(this byte b, byte mod)
            => (b % mod) == 0;

        public static bool IsMod(this GameByte b, byte mod)
            => (b.Value % mod) == 0;

        public static byte ParseByteSpecial(this char c)
        {
            if(c >= 'A' && c <= 'Z')
            {
                return (byte)(10 + (c - 'A'));
            }
            if (c >= '0' && c <= '9')
            {
                return (byte)(c - '0');
            }
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
                '?' => 24,
                _ => 0
            };
        }

    }
}
