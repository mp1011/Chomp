using ChompGame.Data;

namespace ChompGame.Graphics
{
    class PaletteSwitch
    {
        private MaskedByte _palette;
        private MaskedByte _commandCount;

        public PaletteSwitch(int address, SystemMemory memory)
        {
            _palette = new MaskedByte(address, Bit.Right2, memory);
            _commandCount = new MaskedByte(address, Bit.Left6, memory);
        }

        public byte CommandCount
        {
            get => (byte)(_commandCount >> 2);
            set => _commandCount.Value = (byte)(value << 2);
        }

        public byte Palette
        {
            get => _palette.Value;
            set => _palette.Value = value;
        }
    }
}
