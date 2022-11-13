using ChompGame.Data;

namespace ChompGame.Extensions
{
    public static class ByteExtensions
    {
        public static bool IsMod(this byte b, byte mod)
            => (b % mod) == 0;

        public static bool IsMod(this GameByte b, byte mod)
            => (b.Value % mod) == 0;

    }
}
