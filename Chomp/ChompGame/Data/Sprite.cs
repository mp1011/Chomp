using ChompGame.GameSystem;

namespace ChompGame.Data
{
    public class Sprite
    {
        public int Address => _position.Address;

        private readonly Specs _specs;
        private readonly GameByteGridPoint _position;
        private readonly MaskedByte _tile;
        private readonly GameBit _flipX;
        private readonly GameBit _flipY;

        private readonly MaskedByte _secondTileOffset;
        private readonly MaskedByte _palette;
        private readonly GameEnum2<Orientation> _orientation;
        private readonly GameBit _priority;
        private readonly GameBit _screenX;
        private readonly GameBit _screenY;

        public Orientation Orientation
        {
            get => _orientation.Value;
            set => _orientation.Value = value;
        }

        public Sprite(int address, SystemMemory memory, Specs specs)
        {
            _position = new GameByteGridPoint(address, memory, (byte)specs.ScreenWidth, (byte)specs.ScreenHeight);
            _specs = specs;
            _tile = new MaskedByte(address + 2, Bit.Right6, memory);
            _flipX = new GameBit(address + 2, Bit.Bit6, memory);
            _flipY = new GameBit(address + 2, Bit.Bit7, memory);

            _secondTileOffset = new MaskedByte(address + 3, Bit.Right2, memory);
            _palette = new MaskedByte(address + 3, (Bit)12, memory);
            _orientation = new GameEnum2<Orientation>(address + 3, Bit.Bit4, memory);
            _priority = new GameBit(address + 3, Bit.Bit5, memory);
            _screenX = new GameBit(address + 3, Bit.Bit6, memory);
            _screenY = new GameBit(address + 3, Bit.Bit7, memory);
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
            get => _position.X + (_screenX.Value ? 256 : 0);
            set
            {
                _screenX.Value = value > 255;
                _position.X = (byte)value;
            }
        }

        public int Y
        {
            get => _position.Y + (_screenY.Value ? 256 : 0);
            set
            {
                _screenY.Value = value > 255;
                _position.Y = (byte)value;
            }
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
                    return X + _specs.TileWidth * 2;
                else
                    return X + _specs.TileWidth;
            }
        }
        public int Bottom
        {
            get
            {
                if (Tile2Offset == 0 || Orientation == Orientation.Horizontal)
                    return Y + _specs.TileHeight;
                else
                    return Y + _specs.TileHeight*2;
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
