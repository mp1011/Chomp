using ChompGame.Data;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame.SceneModels
{
    class SceneDefinition
    {
        private readonly SystemMemory _systemMemory;
        private readonly LowNibble _tileRow;
        private readonly HighNibble _tileRegionCount;
        private readonly GameByte _regionMapCount;
        public  TileMasterNameTable TileMaster { get; }

        public int Address => _regionMapCount.Address;

        public int RegionStartAddress => Address + 2 + TileMaster.Bytes;

        public int TileRegionStartAddress => RegionStartAddress +
            (RegionCount * PatternTableRegionMap.ByteLength);

        public byte RegionCount => _regionMapCount.Value;

        public byte TileRegionCount => _tileRegionCount.Value;

        public byte TileRow
        {
            get => _tileRow.Value;
        }

        public SceneDefinition(SystemMemoryBuilder memoryBuilder, 
            byte tileRow,
            Func<SystemMemoryBuilder, TileMasterNameTable> createTileMaster)
        {
            _regionMapCount = memoryBuilder.AddByte();
            _tileRow = new LowNibble(memoryBuilder);
            _tileRegionCount = new HighNibble(memoryBuilder);

            memoryBuilder.AddByte();
            _systemMemory = memoryBuilder.Memory;

            TileMaster = createTileMaster(memoryBuilder);

            _tileRow.Value = tileRow;
        }

        public SceneDefinition(int address, SystemMemory systemMemory)
        {
            _regionMapCount = new GameByte(address, systemMemory);
            _tileRow = new LowNibble(address + 1, systemMemory);
            _tileRegionCount = new HighNibble(address + 1, systemMemory);
            TileMaster = new TileMasterNameTable(address + 2, systemMemory);

            _systemMemory = systemMemory;
        }

        public PatternTableRegionMap GetRegion(int index)
        {
            return new PatternTableRegionMap(
                RegionStartAddress + (index * PatternTableRegionMap.ByteLength),
                _systemMemory);
        }

        public TileRegionMap GetTileRegion(int index)
        {
            return new TileRegionMap(TileRegionStartAddress + (index * TileRegionMap.ByteLength),
                _systemMemory);
        }

        public void AddPatternTableRegion(
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

        public TileRegionMap AddTileRegion(SystemMemoryBuilder memoryBuilder)
        {
            _tileRegionCount.Value++;
            return new TileRegionMap(memoryBuilder);
        }
    }

   
}
