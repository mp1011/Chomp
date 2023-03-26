using ChompGame.Data;

namespace ChompGame.MainGame.SceneModels
{
    class DynamicBlockLocation
    {
        private SceneDefinition _sceneDefinition;
        private MaskedByte _x;
        private MaskedByte _y;

        public byte TileX
        {
            get => (byte)(_x.Value * 2);
            set => _x.Value = (byte)(value / 2);
        }

        public byte TileY
        {
            get => (byte)(_y.Value * 2);
            set => _y.Value = (byte)(value / 2);
        }

        public DynamicBlockLocation(SystemMemory memory, int address, SceneDefinition sceneDefinition)
        {
            _sceneDefinition = sceneDefinition;

            switch (sceneDefinition.ScrollStyle)
            {
                case ScrollStyle.NameTable:
                    _x = new MaskedByte(address, (Bit)15, memory);
                    _y = new MaskedByte(address, (Bit)240, memory, leftShift: 4);
                    break;
                case ScrollStyle.Horizontal:
                    _x = new MaskedByte(address, (Bit)31, memory);
                    _y = new MaskedByte(address, (Bit)224, memory, leftShift: 5);
                    break;
                case ScrollStyle.Vertical:
                    _x = new MaskedByte(address, (Bit)248, memory, leftShift: 3);
                    _y = new MaskedByte(address, (Bit)7, memory);
                    break;
                default:
                    _x = new MaskedByte(address, (Bit)15, memory);
                    _y = new MaskedByte(address, (Bit)240, memory, leftShift: 4);
                    break;
            }
        }
    }

    /// <summary>
    /// Note: occupies higher 4 bits
    /// </summary>
    class DynamicBlockState
    {
        private GameBit _topLeft, _topRight, _bottomLeft, _bottomRight;

        private GameBit _on;

        private MaskedByte _id;

        public byte Id
        {
            get => _id.Value;
            set => _id.Value = value;
        }

        public bool TopRight
        {
            get => _topRight.Value;
            set => _topRight.Value = value;
        }

        public bool BottomLeft
        {
            get => _bottomLeft.Value;
            set => _bottomLeft.Value = value;
        }

        public bool BottomRight
        {
            get => _bottomRight.Value;
            set => _bottomRight.Value = value;
        }
        public bool TopLeft
        {
            get => _topLeft.Value;
            set => _topLeft.Value = value;
        }

        public bool On
        {
            get => _on.Value;
            set => _on.Value = value;
        }

        public bool AnyOn => _topLeft.Value || _topRight.Value || _bottomLeft.Value || _bottomRight.Value;

        public DynamicBlockState(SystemMemory memory, int address)
        {
            _topLeft = new GameBit(address, Bit.Bit4, memory);
            _topRight = new GameBit(address, Bit.Bit5, memory);
            _bottomLeft = new GameBit(address, Bit.Bit6, memory);
            _bottomRight = new GameBit(address, Bit.Bit7, memory);

            _on = new GameBit(address, Bit.Bit5, memory);
            _id = new MaskedByte(address, (Bit)224, memory, leftShift:5);
        }
    }
}
