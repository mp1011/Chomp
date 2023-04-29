namespace ChompGame.Data
{
    public class DenseTwoBitArray
    {
        private readonly int _address;
        private readonly SystemMemory _memory;

        public int Address => _address;

        public DenseTwoBitArray(int address, SystemMemory memory)
        {
            _address = address;
            _memory = memory;
        }

        public byte this[int index]
        {
            get
            {
                int byteIndex = _address + (index / 4);
                int shift = (index % 4) * 2;
                int mask = 3 << shift;

                var ret = (byte)((_memory[byteIndex] & mask) >> shift);
                return ret;
            }
            set
            {
                int byteIndex = _address + (index / 4);
                int shift = (index % 4) * 2;
                value = (byte)((value % 4) << shift);

                byte mask = (byte)(~(3 << shift));

                _memory[byteIndex] = (byte)(_memory[byteIndex] & mask);
                _memory[byteIndex] = (byte)(_memory[byteIndex] | value);
            }
        }
            
    }
}
