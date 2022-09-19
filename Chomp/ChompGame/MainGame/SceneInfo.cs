using ChompGame.Data;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame
{
    class SceneInfo
    {
        private SystemMemory _systemMemory;

        private const int _bytesPerRegion = 3;
        public int Bytes => 1 + (_patternTableRegions * _bytesPerRegion);

        private readonly GameByte _nameTableIndex;
        private readonly GameByte _patternTableRegions;

        public byte RegionCount => _patternTableRegions.Value;

        public SceneInfo(byte nameTableIndex, byte regionCount, SystemMemoryBuilder memoryBuilder)
        {
            _nameTableIndex = memoryBuilder.AddByte(nameTableIndex);
            _patternTableRegions = memoryBuilder.AddByte(regionCount);
            memoryBuilder.AddBytes(regionCount * _bytesPerRegion);

            _systemMemory = memoryBuilder.Memory;
        }

        public SceneInfo(int address, SystemMemory systemMemory)
        {
            _nameTableIndex = new GameByte(address, systemMemory);
            _patternTableRegions = new GameByte(address + 1, systemMemory);
            _systemMemory = systemMemory;
        }

        public SceneInfoRegion GetRegion(int index)
        {
            return new SceneInfoRegion(
                _patternTableRegions.Address + 1 + (index * _bytesPerRegion),
                _systemMemory);
        }

        public void DefineRegion(int index, 
            InMemoryByteRectangle region,
            Point destination,
            SystemMemory systemMemory)
        {
            int address = _patternTableRegions.Address + 1 + (index * _bytesPerRegion);

            new NibbleRectangle(address, systemMemory)
                .CopyFrom(region);

            var pt = new NibblePoint(address + 2, systemMemory);
            pt.X = (byte)destination.X;
            pt.Y = (byte)destination.Y;
        }
    }

    public class SceneInfoRegion
    {
        private NibbleRectangle _region;
        private NibblePoint _destination;

        public Point TileDestination => new Point(_destination.X, _destination.Y);

        public ByteRectangle TileRegion => _region;

        public SceneInfoRegion(int address, SystemMemory systemMemory)
        {
            _region = new NibbleRectangle(address, systemMemory);
            _destination = new NibblePoint(address + 2, systemMemory);
        }
    }
}
