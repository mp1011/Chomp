using ChompGame.GameSystem;

namespace ChompGame.Data
{
    public class Sprite
    {
        public const int ByteLength = 4;
        public int Address { get; }

        private readonly Specs _specs;
        private readonly MaskedByte _xPos;
        private readonly GameBit _flipX;

        private readonly MaskedByte _yPos;
        private readonly GameBit _flipY;

        private readonly MaskedByte _tile;
        private readonly TwoBit _secondTileOffset;

        private readonly MaskedByte _palette;
        private readonly GameBit _sizeX;
        private readonly GameBit _sizeY;
        private readonly GameBit _priority;
        private readonly GameBit _visible;

        private readonly GameByteGridPoint _screenScroll;

        public bool Visible
        {
            get => _visible.Value;
            set => _visible.Value = value;
        }

        public Sprite(SystemMemoryBuilder memoryBuilder, Specs specs, GameByteGridPoint screenScroll)
        {
            Address = memoryBuilder.CurrentAddress;
            _screenScroll = screenScroll;
            _specs = specs;

            _xPos = memoryBuilder.AddMaskedByte(Bit.Right7);
            _flipX = new GameBit(_xPos.Address, Bit.Bit7, memoryBuilder.Memory);

            _yPos = memoryBuilder.AddMaskedByte(Bit.Right7);            
            _flipY = new GameBit(_yPos.Address, Bit.Bit7, memoryBuilder.Memory);

            _tile = memoryBuilder.AddMaskedByte(Bit.Right6);
            _secondTileOffset = _secondTileOffset = new TwoBit(memoryBuilder.Memory, _tile.Address, 6);

            _palette = memoryBuilder.AddMaskedByte(Bit.Right2);
            _sizeX = new GameBit(_palette.Address, Bit.Bit2, memoryBuilder.Memory);
            _sizeY = new GameBit(_palette.Address, Bit.Bit3, memoryBuilder.Memory);
            _priority = new GameBit(_palette.Address, Bit.Bit4, memoryBuilder.Memory);
            _visible = new GameBit(_palette.Address, Bit.Bit5, memoryBuilder.Memory);

            _visible.Value = true;
        }

        public Sprite(int address, SystemMemory memory, Specs specs, GameByteGridPoint screenScroll)
        {
            Address = address;
            _screenScroll = screenScroll;
            _specs = specs;

            _xPos = new MaskedByte(address, Bit.Right7, memory);
            _flipX = new GameBit(_xPos.Address, Bit.Bit7, memory);

            _yPos = new MaskedByte(address + 1, Bit.Right7, memory);
            _flipY = new GameBit(_yPos.Address, Bit.Bit7, memory);

            _tile = new MaskedByte(address + 2, Bit.Right6, memory);
            _secondTileOffset = new TwoBit(memory, _tile.Address, 6);

            _palette = new MaskedByte(address + 3, Bit.Right2, memory);
            _sizeX = new GameBit(_palette.Address, Bit.Bit2, memory);
            _sizeY = new GameBit(_palette.Address, Bit.Bit3, memory);
            _priority = new GameBit(_palette.Address, Bit.Bit4, memory);
            _visible = new GameBit(_palette.Address, Bit.Bit5, memory);
        }

        public int SizeX
        {
            get => _sizeX.Value ? 2 : 1;
            set => _sizeX.Value = (value == 2);
        }

        public int SizeY
        {
            get => _sizeY.Value ? 2 : 1;
            set => _sizeY.Value = (value == 2);
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


        public bool IntersectsScanline(byte scanLine)
        {
            return scanLine >= Y && scanLine < Bottom;
        }

        public byte X
        {
            get => _xPos.Value;
            set => _xPos.Value = value;
        }

        public byte Y
        {
            get => _yPos.Value;
            set => _yPos.Value = value;
        }

        public byte Width => (byte)(SizeX * _specs.TileWidth);

        public byte Height => (byte)(SizeY * _specs.TileHeight);

        public int Right => X + Width;
        public int Bottom => Y + Height;

        public byte Tile
        {
            get => _tile.Value;
            set => _tile.Value = value;
        }

        public byte Tile2Offset
        {
            get => _secondTileOffset.Value;
            set => _secondTileOffset.Value = value;
        }

        public byte Palette
        {
            get => _palette.Value;
            set => _palette.Value = value;
        }

    }
}
