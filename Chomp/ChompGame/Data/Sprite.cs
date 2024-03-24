using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame;
using ChompGame.MainGame.SceneModels;
using Microsoft.Xna.Framework;

namespace ChompGame.Data
{
    public class Sprite : IWithPosition
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

        private readonly GameByteEnum<SpritePalette> _palette;
        private readonly GameBit _sizeX;
        private readonly GameBit _sizeY;
        private readonly GameBit _priority;
        private readonly GameBit _visible;

        private readonly GameByteGridPoint _screenScroll;

        public bool Priority
        {
            get => _priority.Value;
            set => _priority.Value = value;
        }

        public bool Visible
        {
            get => Tile != 0 && _visible.Value;
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

            var paletteByte = memoryBuilder.AddMaskedByte(Bit.Right2);
            _palette = new GameByteEnum<SpritePalette>(paletteByte);

            _sizeX = new GameBit(paletteByte.Address, Bit.Bit2, memoryBuilder.Memory);
            _sizeY = new GameBit(paletteByte.Address, Bit.Bit3, memoryBuilder.Memory);
            _priority = new GameBit(paletteByte.Address, Bit.Bit4, memoryBuilder.Memory);
            _visible = new GameBit(paletteByte.Address, Bit.Bit5, memoryBuilder.Memory);

            _visible.Value = true;
            _priority.Value = true;
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

            var paletteByte = new MaskedByte(address + 3, Bit.Right2, memory);
            _palette = new GameByteEnum<SpritePalette>(paletteByte);

            _sizeX = new GameBit(paletteByte.Address, Bit.Bit2, memory);
            _sizeY = new GameBit(paletteByte.Address, Bit.Bit3, memory);
            _priority = new GameBit(paletteByte.Address, Bit.Bit4, memory);
            _visible = new GameBit(paletteByte.Address, Bit.Bit5, memory);
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
            var bottomMod = Bottom.NModByte(_specs.NameTablePixelHeight);
            if(bottomMod < Y)
            {
                return scanLine >= Y || scanLine < bottomMod;
            }
            else 
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

        public SpritePalette Palette
        {
            get => _palette.Value;
            set => _palette.Value = value;
        }
        int IWithPosition.X { get => X; set => X = (byte)value; }
        int IWithPosition.Y { get => Y; set => Y = (byte)value; }

        public Rectangle Bounds =>
            new Rectangle(X, Y, Width, Height);
 
    }
}
