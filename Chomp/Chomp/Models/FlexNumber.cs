using Chomp.Extensions;

namespace Chomp.Models
{
    public struct FlexNumber
    {
        public int Value { get; }
        public int Bits { get; }

        public int Bytes => Bits / 8;
        public int Kilobytes => Bytes / 1024;

        private FlexNumber(int value, int bits)
        {
            Value = value;
            Bits = bits;
        }

        public static FlexNumber FromValue(int value)
        {
            int bits = 1;
            while (bits.Pow2() < value)
                bits++;

            return new FlexNumber(value:value, bits: bits);               
        }

        public static FlexNumber FromBits(int bits)
        {
            return new FlexNumber(value: bits.Pow2(), bits: bits);
        }

    }
}
