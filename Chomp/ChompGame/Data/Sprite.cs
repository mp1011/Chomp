using ChompGame.GameSystem;

namespace ChompGame.Data
{
    public class Sprite
    {
        public int Address { get; }

        private readonly Specs _specs;
        private readonly ExtendedByte _xPos;
        private readonly ExtendedByte _yPos;

        private readonly MaskedByte _tile;
        private readonly GameBit _flipX;
        private readonly GameBit _flipY;

        private readonly MaskedByte _secondTileOffset;
        private readonly MaskedByte _palette;
        private readonly GameEnum2<Orientation> _orientation;
        private readonly GameBit _priority;

        private readonly GameByteGridPoint _screenScroll;

        public Orientation Orientation
        {
            get => _orientation.Value;
            set => _orientation.Value = value;
        }

        public bool Visible => Tile != 0;

        public Sprite(int address, SystemMemory memory, Specs specs, GameByteGridPoint screenScroll)
        {
            _screenScroll = screenScroll;

            Address = address;
            _specs = specs;

            var x = new MaskedByte(address, (Bit)(specs.NameTablePixelWidth/2-1), memory);
            var y = new MaskedByte(address+1, (Bit)(specs.NameTablePixelHeight / 2 - 1), memory);
            _tile = new MaskedByte(address + 2, Bit.Right6, memory);
            _flipX = new GameBit(address + 2, Bit.Bit6, memory);
            _flipY = new GameBit(address + 2, Bit.Bit7, memory);

            _secondTileOffset = new MaskedByte(address + 3, Bit.Right2, memory);
            _palette = new MaskedByte(address + 3, (Bit)12, memory);
            _orientation = new GameEnum2<Orientation>(address + 3, Bit.Bit4, memory);
            _priority = new GameBit(address + 3, Bit.Bit5, memory);
            var screenX = new GameBit(address + 3, Bit.Bit6, memory);
            var screenY = new GameBit(address + 3, Bit.Bit7, memory);

            _xPos = new ExtendedByte(x, screenX, specs.ScreenWidth);
            _yPos = new ExtendedByte(y, screenY, specs.ScreenHeight);
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

        public int X
        {
            get
            {
                return _xPos.Value;
            }
            set => _xPos.Value = value;
        }

        public int Y
        {
            get
            {
                var y = _yPos.Value;

                if (y > _specs.ScreenHeight && _screenScroll.Y < _specs.ScreenHeight)
                    y -= (_specs.ScreenHeight * 2);

                return y;
            }
            set => _yPos.Value = value;
        }

        public byte Width
        {
            get
            {
                if (Tile2Offset != 0 && Orientation == Orientation.Horizontal)
                    return (byte)(_specs.TileWidth * 2);
                else
                    return (byte)(_specs.TileWidth);
            }
        }

        public byte Height
        {
            get
            {
                if (Tile2Offset != 0 && Orientation == Orientation.Vertical)
                    return (byte)(_specs.TileHeight * 2);
                else
                    return (byte)(_specs.TileHeight);
            }
        }

        public int Right
        {
            get
            {
                if (Tile2Offset != 0 && Orientation == Orientation.Horizontal)
                    return (X + _specs.TileWidth * 2);
                else
                    return (X + _specs.TileWidth);
            }
        }
        public int Bottom
        {
            get
            {
                if (Tile2Offset == 0 || Orientation == Orientation.Horizontal)
                    return (Y + _specs.TileHeight);
                else
                    return (Y + _specs.TileHeight*2);
            }
        }

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
            get => (byte)(_palette.Value >> 2);
            set => _palette.Value = (byte)(value << 2);
        }

    }
}
