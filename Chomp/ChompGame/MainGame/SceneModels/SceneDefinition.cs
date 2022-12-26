﻿using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using System;

namespace ChompGame.MainGame.SceneModels
{
    public enum ScrollStyle : byte
    {
        None=0,
        NameTable=1,
        Horizontal=2,
        Vertical=3
    }

    [Flags]
    public enum SpriteLoadFlags : byte
    {
        Player = 1,
        Lizard = 2,
        Bird = 4
    }

    public enum LevelShape : byte
    {
        Flat = 0,

        //ScrollStyle 0
        CornerStairs=1,
        BigStair=2,
        TShape=3,

        //ScrollStyle 1
        TwoHorizontalChambers=1,
        TwoVerticalChambers=2,
        FourChambers=3,

        //ScrollStyle 2
        LowVariance=1,
        MediumVariance=2,
        HighVariance=3,

        //ScrollStyle 3
        ZigZag=1,
        Ladder=2
    }

    class SceneDefinition
    {
        public const int Bytes = 6;

        public const int StatusBarTiles = 2;

        private readonly SystemMemory _systemMemory;
        private readonly Specs _specs;

        //byte 0
        private readonly TwoBitEnum<ScrollStyle> _scrollStyle;
        private readonly TwoBitEnum<LevelShape> _levelShape;
        private TwoBit _beginTiles;
        private TwoBit _endTiles;

        //byte 1
        private GameByteEnum<SpriteLoadFlags> _sprites;

        //byte 2
        private readonly MaskedByte _parallaxTileBegin;
        private readonly TwoBit _groundFillTiles;
        private readonly TwoBit _groundTopTiles;
        private readonly GameBit _sideTiles;

        //byte 3
        private readonly LowNibble _bgTileRow;
        private readonly TwoBit _parallaxSizeA;
        private readonly TwoBit _parallaxSizeB;

        //byte 4 (todo, repurpose this)
        private readonly GameByte _partsAddress;

        //byte 5
        private TwoBit _enemyAPalette;
        private TwoBit _enemyBPalette;
        private TwoBit _extraAPalette;
        private TwoBit _extraBPalette;

        public int Address => _scrollStyle.Address;
        
        public int BgTile => 7;

        public int BeginTiles => _beginTiles.Value;

        public int EndTiles => _endTiles.Value;

        public int GroundFillStart => BgTile + 2;

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

        public byte TileRow
        {
            get => _bgTileRow.Value;
        }

        public int ParallaxLayerABeginTile => BeginTiles + _parallaxTileBegin.Value;

        public int ParallaxLayerATiles => _parallaxSizeA.Value;

        public int ParallaxLayerBTiles => _parallaxSizeB.Value;

        public LevelShape LevelShape => _levelShape.Value;

        public ScrollStyle ScrollStyle => _scrollStyle.Value;

        public bool HasSprite(SpriteLoadFlags flag) => _sprites.Value.HasFlag(flag);

        public int LayerABeginTile => (StatusBarTiles + ParallaxLayerABeginTile);
        public int LayerBBeginTile => LayerABeginTile + ParallaxLayerATiles;
        public int LayerCBeginTile => LayerBBeginTile + ParallaxLayerBTiles;
        public int ParallaxEndTile => LayerCBeginTile + ParallaxLayerATiles;

        public SceneDefinition(SystemMemoryBuilder memoryBuilder, 
            Specs specs,
            ScrollStyle scrollStyle,
            LevelShape levelShape,
            byte beginTiles,
            byte endTiles,
            SpriteLoadFlags spriteLoadFlags,
            byte groundFillTiles,
            byte groundTopTiles,
            byte sideTiles,
            byte tileRow,
            byte parallaxTileBegin,
            byte parallaxSizeA,
            byte parallaxSizeB,
            byte enemyAPalette,
            byte enemyBPalette,
            byte extraAPalette,
            byte extraBPalette)
        {
            _specs = specs;
            _systemMemory = memoryBuilder.Memory;

            _scrollStyle = new TwoBitEnum<ScrollStyle>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 0);
            _levelShape = new TwoBitEnum<LevelShape>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 2);
            _beginTiles = new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 4);
            _endTiles = new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 6);
            memoryBuilder.AddByte();

            _sprites = new GameByteEnum<SpriteLoadFlags>(memoryBuilder.AddByte());

            _parallaxTileBegin = memoryBuilder.AddMaskedByte(Bit.Right3);
            _groundFillTiles = new TwoBit(memoryBuilder.Memory, _parallaxTileBegin.Address, 3);
            _groundTopTiles = new TwoBit(memoryBuilder.Memory, _parallaxTileBegin.Address, 5);
            _sideTiles = new GameBit(_parallaxTileBegin.Address, Bit.Bit7, memoryBuilder.Memory);
            
            _bgTileRow = new LowNibble(memoryBuilder);
            _parallaxSizeA = new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 4);
            _parallaxSizeB = new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 6);
            memoryBuilder.AddByte();

            _partsAddress = memoryBuilder.AddByte(); //parts address

            _scrollStyle.Value = scrollStyle;
            _levelShape.Value = levelShape;
            _beginTiles.Value = beginTiles;
            _endTiles.Value = endTiles;

            _sprites.Value = spriteLoadFlags;

            _groundFillTiles.Value = (byte)(groundFillTiles-1);
            _groundTopTiles.Value = (byte)(groundTopTiles -1);
            _sideTiles.Value = sideTiles == 2;

            _bgTileRow.Value = tileRow;

            _parallaxTileBegin.Value = parallaxTileBegin;
            _parallaxSizeA.Value = parallaxSizeA;
            _parallaxSizeB.Value = parallaxSizeB;

            var palletes = memoryBuilder.AddByte();
            _enemyAPalette = new TwoBit(memoryBuilder.Memory, palletes.Address, 0);
            _enemyBPalette = new TwoBit(memoryBuilder.Memory, palletes.Address, 2);
            _extraAPalette = new TwoBit(memoryBuilder.Memory, palletes.Address, 4);
            _extraBPalette = new TwoBit(memoryBuilder.Memory, palletes.Address, 6);

            _enemyAPalette.Value = enemyAPalette;
            _enemyBPalette.Value = enemyBPalette;
            _extraAPalette.Value = extraAPalette;
            _extraBPalette.Value = extraBPalette;
        }

        public SceneDefinition(int address, SystemMemory systemMemory, Specs specs)
        {
            _specs = specs;
            _scrollStyle = new TwoBitEnum<ScrollStyle>(systemMemory, address, shift: 0);
            _levelShape = new TwoBitEnum<LevelShape>(systemMemory, address, shift: 2);
            _beginTiles = new TwoBit(systemMemory, address, 4);
            _endTiles = new TwoBit(systemMemory, address, 6);

            _sprites = new GameByteEnum<SpriteLoadFlags>(new GameByte(address + 1, systemMemory));

            _parallaxTileBegin = new MaskedByte(address + 2, Bit.Right3, systemMemory);
            _groundFillTiles = new TwoBit(systemMemory, address + 2, 3);
            _groundTopTiles = new TwoBit(systemMemory, address + 2, 5);
            _sideTiles = new GameBit(address + 2, Bit.Bit7, systemMemory);

            _bgTileRow = new LowNibble(address + 3, systemMemory);
            _parallaxSizeA = new TwoBit(systemMemory, address + 3, 4);
            _parallaxSizeB = new TwoBit(systemMemory, address + 3, 6);

            _partsAddress = new GameByte(address + 4, systemMemory);
            _systemMemory = systemMemory;

            _enemyAPalette = new TwoBit(systemMemory, address + 5, 0);
            _enemyBPalette = new TwoBit(systemMemory, address + 5, 2);
            _extraAPalette = new TwoBit(systemMemory, address + 5, 4);
            _extraBPalette = new TwoBit(systemMemory, address + 5, 6);

        }

        public SceneDefinition(Level level, SystemMemory memory, Specs specs)
            :this(memory.GetAddress(AddressLabels.SceneDefinitions) + ((int)level * SceneDefinition.Bytes), memory,specs)
        { 
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

        public int LevelTileWidth =>
            ScrollStyle switch {
                ScrollStyle.None => _specs.ScreenWidth / _specs.TileWidth,
                ScrollStyle.Vertical => _specs.ScreenWidth / _specs.TileWidth,
                ScrollStyle.NameTable => _specs.NameTableWidth,
                ScrollStyle.Horizontal => (_specs.ScreenWidth / _specs.TileWidth) * 4,
                _ => throw new NotImplementedException()
            };

        public int LevelTileHeight =>
             ScrollStyle switch {
                 ScrollStyle.None => (_specs.ScreenHeight / _specs.TileHeight) - StatusBarTiles,
                 ScrollStyle.Horizontal => (_specs.ScreenHeight / _specs.TileHeight) - StatusBarTiles,
                 ScrollStyle.NameTable => _specs.NameTableHeight - StatusBarTiles,
                 ScrollStyle.Vertical => (_specs.ScreenHeight / _specs.TileHeight) * 4,
                 _ => throw new NotImplementedException()
             };

        public byte GetPalette(ScenePartType scenePartType)
        {
            return scenePartType switch {
                ScenePartType.EnemyType1 => _enemyAPalette.Value,
                ScenePartType.EnemyType2 => _enemyBPalette.Value,
                _ => 0
            };
        }
    }
}
