namespace ChompGame.Data
{
    public class NibbleArray
    {
        private int _address;
        private SystemMemory _memory;

        public int Address => _address;

        public NibbleArray(int address, SystemMemory memory)
        {
            _memory = memory;
            _address = address;
        }

        public byte this[int index]
        {
            get
            {
                int memoryIndex = _address + (index / 2);
                if ((index % 2) == 0)
                    return (byte)(_memory[memoryIndex] & 15);
                else
                    return (byte)((_memory[memoryIndex] & 240) >> 4);
            }
            set
            {
                int memoryIndex = _address + (index / 2);
                if ((index % 2) == 0)
                {
                    _memory[memoryIndex] = (byte)(_memory[memoryIndex] & 240);
                    _memory[memoryIndex] = (byte)(_memory[memoryIndex] | value);
                }
                else
                {
                    _memory[memoryIndex] = (byte)(_memory[memoryIndex] & 15);
                    _memory[memoryIndex] = (byte)(_memory[memoryIndex] | (value << 4));
                }
            }
        }

    }

}
