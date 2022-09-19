namespace ChompGame.Data
{
    public class GameByteArray
    {
        private readonly SystemMemory _systemMemory;
        public int Address { get; }

        public GameByteArray(int address, SystemMemory systemMemory)
        {
            _systemMemory = systemMemory;
            Address = address;
        }

        public byte this[int index]
        {
            get => _systemMemory[Address + index];
            set => _systemMemory[Address + index] = value;
        }
    }
}
