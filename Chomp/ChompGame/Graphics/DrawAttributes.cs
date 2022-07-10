using ChompGame.Data;

namespace ChompGame.Graphics
{
    public class DrawAttributes
    {
        private readonly GameByte _data;
        private GameBit _flipX, _flipY;

        public DrawAttributes(int address, SystemMemory memory)
        {
            _data = new GameByte(address, memory);
            _flipX = new GameBit(address, Bit.Bit2, memory);
            _flipY = new GameBit(address, Bit.Bit3, memory);
        }

        public byte PaletteIndex
        {
            get => (byte)(_data.Value & 3);
            set
            {
                byte v = (byte)(_data.Value & 252);
                v = (byte)(v | value);
                _data.Value = v;
            }
        }

        public bool FlipX
        {
            get => _flipX.Value;
            set => _flipX.Value = value;
        }

        public bool FlipY
        {
            get => _flipY.Value;
            set => _flipY.Value = value;
        }
    }
}
