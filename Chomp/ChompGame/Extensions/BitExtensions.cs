using ChompGame.Data;

namespace ChompGame.Extensions
{
    public static class BitExtensions
    {
        /// <summary>
        /// Returns a byte with only the given bit on
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        public static byte ToMask(this Bit bit)
        {
            return (byte)bit;
        }

        public static Bit BitFromIndex(this int index)
        {
            switch(index)
            {
                case 1: return Bit.Bit1;
                case 2: return Bit.Bit2;
                case 3: return Bit.Bit3;
                case 4: return Bit.Bit4;
                case 5: return Bit.Bit5;
                case 6: return Bit.Bit6;
                case 7: return Bit.Bit7;
                default: return Bit.Bit0;

            }
        }
    }
}
