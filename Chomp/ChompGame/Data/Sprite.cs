using ChompGame.GameSystem;

namespace ChompGame.Data
{
    public class Sprite
    {
        public static int Bytes => 3; //todo, needs to be configurable

        public int Address => _position.Address;

        private readonly Specs _specs;
        private readonly GameByteGridPoint _position;
        private readonly GameByte _tile;

        public Sprite(GameByteGridPoint position, GameByte tile, Specs specs)
        {
            _position = position;
            _tile = tile;
            _specs = specs;
        }

        public Sprite(int address, SystemMemory systemMemory, Specs specs)
        {
            _position = new GameByteGridPoint(address, systemMemory, 1,1);
            _tile = new GameByte(address+2, systemMemory);
            _specs = specs;
        }

        public bool IntersectsScanline(byte scanLine)
        {
            return scanLine >= Y && scanLine < (Y + _specs.TileHeight);
        }

        public byte X
        {
            get => _position.X;
            set => _position.X = value;
        }

        public byte Y
        {
            get => _position.Y;
            set => _position.Y = value;
        }

        public byte Right => (byte)(X + _specs.TileWidth);
        public byte Bottom => (byte)(Y + _specs.TileHeight);

        public byte Tile
        {
            get => _tile.Value;
            set => _tile.Value = value;
        }

    }
}
