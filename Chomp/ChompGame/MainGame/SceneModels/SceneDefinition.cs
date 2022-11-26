using ChompGame.Data;
using ChompGame.GameSystem;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame.SceneModels
{
    public enum LevelShape : byte
    {
        Horizontal=0,
        Vertical=1,
        Square=2
    }

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

        private readonly TwoBitEnum<LevelShape> _shape;

        //one bit remaining 

        public int Address => _regionMapCount.Address;

        public int RegionStartAddress => Address + 4;

        public int TileRegionStartAddress => RegionStartAddress +
            (RegionCount * PatternTableRegionMap.ByteLength);

        public byte RegionCount => _regionMapCount.Value;

        public byte MapSize => _mapSize.Value;

        public byte GroundHigh => _groundHighTile.Value;
        
        public byte GroundLow => _groundLowTile.Value;

        public byte GroundVariation => _groundVariation.Value;

        public int BlockTile => 8;

        public int GroundFillStart => BlockTile + 1;

        public int GroundFillEnd => GroundFillStart + _groundFillTiles.Value;

        public int GroundFillTileCount => _groundFillTiles.Value + 1;

        public int GroundLeftCorner => GroundFillEnd + 1;

        public int GroundTopBegin => GroundLeftCorner + 1;

        public int GroundTopEnd => GroundTopBegin + _groundTopTiles.Value;

        public int GroundRightCorner => GroundTopEnd + 1;

        public int LeftTileBegin => GroundRightCorner + 1;

        public int LeftTileEnd => LeftTileBegin + (_sideTiles.Value ? 1 : 0);

        public int RightTileBegin => LeftTileEnd + 1;

        public int RightTileEnd => RightTileBegin + (_sideTiles.Value ? 1 : 0);

        public LevelShape Shape => _shape.Value;

        public byte TileRow
        {
            get => _tileRow.Value;
        }

        public SceneDefinition(SystemMemoryBuilder memoryBuilder, 
            byte tileRow,
            byte mapSize,
            LevelShape shape,
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
            _shape = new TwoBitEnum<LevelShape>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, shift: 5);

            memoryBuilder.AddByte();

            _mapSize.Value = mapSize;
            _groundHighTile.Value = groundHighTile;
            _groundLowTile.Value = groundLowTile;
            _groundVariation.Value = groundVariation;
            _groundFillTiles.Value = (byte)(groundFillTiles-1);
            _groundTopTiles.Value = (byte)(groundTopTiles -1);
            _sideTiles.Value = sideTiles == 2;
            _tileRow.Value = tileRow;
            _shape.Value = shape;
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

            _shape = new TwoBitEnum<LevelShape>(systemMemory, address + 3, shift: 5);

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

        public int GetLeftSideTile(int row) => _sideTiles.Value
            ? LeftTileBegin + (row % 2)
            : LeftTileBegin;

        public int GetRightSideTile(int row) => _sideTiles.Value
         ? RightTileBegin + (row % 2)
         : RightTileBegin;

        public int GetGroundFillTile(int row, int col)
        {
            int groundTileCount = _groundFillTiles.Value + 1;

            int index = row % groundTileCount;
            if (col % 2 == 0)
                index = (index + 1) % groundTileCount;

            return GroundFillStart + index;            
        }

        public int GetGroundTopTile(int col)
        {
            int groundTileCount = _groundTopTiles.Value + 1;

            int index = col % groundTileCount;
            return GroundTopBegin + index;
        }

        public int GetLevelTileWidth(Specs specs)
        {
            switch (Shape)
            {
                case LevelShape.Horizontal:
                    return (MapSize + 1) * specs.NameTableWidth;
                default:
                    throw new NotImplementedException();
            }
        }

        public int GetLevelTileHeight(Specs specs)
        {
            switch (Shape)
            {
                case LevelShape.Horizontal:
                    return specs.NameTableHeight;
                default:
                    throw new NotImplementedException();
            }
        }
    }


}
