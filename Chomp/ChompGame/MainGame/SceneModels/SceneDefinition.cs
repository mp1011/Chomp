using ChompGame.Data;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame.SceneModels
{
    class SceneDefinition
    {
        private readonly SystemMemory _systemMemory;
        
        private readonly LowNibble _regionMapCount;
        private readonly HighNibble _mapSize;

        private readonly LowNibble _tileRow;
        private readonly HighNibble _groundVariation;

        private readonly LowNibble _groundLowTile;
        private readonly HighNibble _groundHighTile;

        private readonly TwoBit _groundFillTiles;
        private readonly TwoBit _groundTopTiles;
        private readonly GameBit _sideTiles;
        //note: three bits remaining

        public int Address => _regionMapCount.Address;

        public int RegionStartAddress => Address + 4;

        public int TileRegionStartAddress => RegionStartAddress +
            (RegionCount * PatternTableRegionMap.ByteLength);

        public byte RegionCount => _regionMapCount.Value;

        public byte MapSize => _mapSize.Value;

        public byte GroundHigh => _groundHighTile.Value;
        
        public byte GroundLow => _groundLowTile.Value;

        public byte GroundVariation => _groundVariation.Value;


        public byte TileRow
        {
            get => _tileRow.Value;
        }

        public SceneDefinition(SystemMemoryBuilder memoryBuilder, 
            byte tileRow,
            byte mapSize,
            byte groundLowTile,
            byte groundHighTile,
            byte groundVariation,
            byte groundFillTiles,
            byte groundTopTiles,
            byte sideTiles)
        {

            _systemMemory = memoryBuilder.Memory;

            memoryBuilder.AddNibbles(ref _regionMapCount, ref _mapSize);
            memoryBuilder.AddNibbles(ref _tileRow, ref _groundVariation);
            memoryBuilder.AddNibbles(ref _groundLowTile, ref _groundHighTile);

            _groundFillTiles = new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 0);
            _groundTopTiles = new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 2);
            _sideTiles = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit4, memoryBuilder.Memory);
            memoryBuilder.AddByte();

            _mapSize.Value = mapSize;
            _groundHighTile.Value = groundHighTile;
            _groundLowTile.Value = groundLowTile;
            _groundVariation.Value = groundVariation;
            _groundFillTiles.Value = groundFillTiles;
            _groundTopTiles.Value = groundTopTiles;
            _sideTiles.Value = sideTiles == 2;
            _tileRow.Value = tileRow;
        }

        public SceneDefinition(int address, SystemMemory systemMemory)
        {
            _regionMapCount = new LowNibble(address, systemMemory);
            _mapSize = new HighNibble(address, systemMemory);

            _tileRow = new LowNibble(address + 1, systemMemory);
            _groundVariation = new HighNibble(address + 1, systemMemory);

            _groundLowTile = new LowNibble(address + 2, systemMemory);
            _groundHighTile = new HighNibble(address + 2, systemMemory);

            _groundFillTiles = new TwoBit(systemMemory, address + 3, 0);
            _groundTopTiles = new TwoBit(systemMemory, address + 3, 2);
            _sideTiles = new GameBit(address + 3, Bit.Bit4, systemMemory);

            _systemMemory = systemMemory;
        }

        public PatternTableRegionMap GetRegion(int index)
        {
            return new PatternTableRegionMap(
                RegionStartAddress + (index * PatternTableRegionMap.ByteLength),
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
    }

   
}
