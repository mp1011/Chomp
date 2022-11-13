using ChompGame.Data;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SceneModels
{
    class SceneDefinition
    {
        private readonly SystemMemory _systemMemory;
        private readonly GameByte _regionMapCount;

        public int Address => _regionMapCount.Address;
        public byte RegionCount => _regionMapCount.Value;

        public SceneDefinition(SystemMemoryBuilder memoryBuilder)
        {
            _regionMapCount = memoryBuilder.AddByte();
            _systemMemory = memoryBuilder.Memory;
        }

        public SceneDefinition(int address, SystemMemory systemMemory)
        {
            _regionMapCount = new GameByte(address, systemMemory);
            _systemMemory = systemMemory;
        }

        public PatternTableRegionMap GetRegion(int index)
        {
            return new PatternTableRegionMap(
                Address + 1 + (index * PatternTableRegionMap.ByteLength),
                _systemMemory);
        }

        public void AddRegion(
            SystemMemoryBuilder memoryBuilder,
            InMemoryByteRectangle region,
            Point destination)
        {
            _regionMapCount.Value++;

            int address = memoryBuilder.CurrentAddress;
            memoryBuilder.AddBytes(PatternTableRegionMap.ByteLength);

            new NibbleRectangle(address, memoryBuilder.Memory)
                .CopyFrom(region);

            var pt = new NibblePoint(address + 2, memoryBuilder.Memory);
            pt.X = (byte)destination.X;
            pt.Y = (byte)destination.Y;
        }
    }

   
}
