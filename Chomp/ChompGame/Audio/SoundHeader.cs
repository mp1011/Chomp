using ChompGame.Data;

namespace ChompGame.Audio
{
    class SoundHeader
    {
        public int Address => _noteDuration.Address;

        public static int Length => 3;

        private GameByte _noteDuration;
        private GameByte _sequenceStart;
        private GameByte _sequenceLength;

        public byte NoteDuration
        {
            get => _noteDuration.Value;
            set => _noteDuration.Value = value;
        }

        public byte SequenceStart
        {
            get => _sequenceStart.Value;
            set => _sequenceStart.Value = value;
        }

        public byte SequenceLength
        {
            get => _sequenceLength.Value;
            set => _sequenceLength.Value = value;
        }

        public SoundHeader(SystemMemoryBuilder memoryBuilder)
        {
            _noteDuration = memoryBuilder.AddByte();
            _sequenceStart = memoryBuilder.AddByte();
            _sequenceLength = memoryBuilder.AddByte();
        }

        public SoundHeader(int address, SystemMemory memory)
        {
            _noteDuration = new GameByte(address, memory);
            _sequenceStart = new GameByte(address+1, memory);
            _sequenceLength = new GameByte(address+2, memory);
        }

        public void Set(byte sequenceStart, byte noteDuration, byte sequenceLength)
        {
            SequenceStart = sequenceStart;
            NoteDuration = noteDuration;
            SequenceLength = sequenceLength;
        }
    }
}
