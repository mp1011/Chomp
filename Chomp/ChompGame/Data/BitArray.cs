using ChompGame.Extensions;

namespace ChompGame.Data
{
    public class BitArray
    {
        private int _address;
        private SystemMemory _memory;

        public int Address => _address;

        public BitArray(int address, SystemMemory memory)
        {
            _memory = memory;
            _address = address;
        }

        public bool this[int index]
        {
            get
            {
                int byteIndex = Address + (index / 8);
                int bitIndex = index % 8;

                return (_memory[byteIndex] & (byte)bitIndex.BitFromIndex()) != 0;
            }
            set
            {
                int byteIndex = Address + (index / 8);
                int bitIndex = index % 8;

                byte mask = (byte)bitIndex.BitFromIndex();

                if (!value)
                    _memory[byteIndex] = (byte)(_memory[byteIndex] & ~mask);
                else
                    _memory[byteIndex] = (byte)(_memory[byteIndex] | mask);
            }
        }

    }

    public abstract class NBitArray
    {
        public abstract byte this[int index]
        {
            get;set;
        }
    }
    public class TwoBitArray : NBitArray
    {
        private readonly BitArray _bit0, _bit1;

        public int Address => _bit0.Address;

        public TwoBitArray(BitArray bit0, BitArray bit1)
        {
            _bit0 = bit0;
            _bit1 = bit1;
        }

        public override byte this[int index]
        {
            get
            {
                return (byte)((_bit0[index] ? 1 : 0) + (_bit1[index] ? 2 : 0));
            }
            set
            {
                _bit0[index] = (value & 1) != 0;
                _bit1[index] = (value & 2) != 0;
            }
        }
    }
}
