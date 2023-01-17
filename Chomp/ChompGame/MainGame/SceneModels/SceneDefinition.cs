﻿using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using System;

namespace ChompGame.MainGame.SceneModels
{
    public enum Theme : byte
    {
        Plains = 0,
        Ocean = 1,
        Forest = 2,
        Desert = 3,
        City = 4,
        Space = 5,
        TechBase = 6,
        GlitchCore = 7,
        Max =15
    }

    public enum ParallaxLayer
    {
        Begin,
        Back1,
        Back2,
        Foreground
    }

    public enum EnemyGroup : byte
    {
        Lizard_Bird
    }


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
        public const int Bytes = 3;

        public int StatusBarTiles => Constants.StatusBarTiles;

        private readonly SystemMemory _systemMemory;
        private readonly Specs _specs;

        //byte 0
        private readonly TwoBitEnum<ScrollStyle> _scrollStyle;
        private readonly TwoBitEnum<LevelShape> _levelShape;
        private readonly TwoBit _bgPosition1;
        private readonly TwoBit _bgPosition2;

        //byte 1
        private NibbleEnum<Theme> _theme;          
        private NibbleEnum<EnemyGroup> _enemies;   

        //byte 2 (four TwoBits or two Nibbles)
        private TwoBit _left;
        private TwoBit _top;
        private TwoBit _right;
        private TwoBit _bottom;
        private Nibble _begin; 
        private Nibble _end;

        public int Address => _scrollStyle.Address;
        
        public int GroundFillStart => 8;

        public int GroundFillEnd => GroundFillStart + 1;

        public int GroundLeftCorner => GroundFillEnd + 1;

        public int GroundTopBegin => GroundLeftCorner + 1;

        public int GroundTopEnd => GroundTopBegin + 1;

        public int GroundRightCorner => GroundTopEnd + 1;

        public int LeftTile => GroundRightCorner + 1;

        public int RightTile => LeftTile + 1;

        public LevelShape LevelShape => _levelShape.Value;

        public ScrollStyle ScrollStyle => _scrollStyle.Value;

        public int BeginTiles => _begin.Value * 2;
        public int EndTiles => _end.Value * 2;
        public int LeftTiles => _left.Value * 2;
        public int RightTiles => _right.Value * 2;
        public int TopTiles => _top.Value * 2;
        public int BottomTiles => _bottom.Value * 2;

        public Theme Theme => _theme.Value;

        public bool HasSprite(SpriteLoadFlags flag)
        {
            if (flag.HasFlag(SpriteLoadFlags.Player))
                return true;

            return _enemies.Value switch {
                EnemyGroup.Lizard_Bird => flag.HasFlag(SpriteLoadFlags.Bird) || flag.HasFlag(SpriteLoadFlags.Lizard),
                _ => false
            };
        }

        public int GetParallaxLayerTile(ParallaxLayer layer, bool includeStatusBar) =>
            layer switch {
                ParallaxLayer.Begin => (includeStatusBar ? StatusBarTiles : 0) + BeginTiles,
                ParallaxLayer.Back1 => GetParallaxLayerTile(ParallaxLayer.Begin, includeStatusBar) + _bgPosition1.Value,
                ParallaxLayer.Back2 => GetParallaxLayerTile(ParallaxLayer.Back1, includeStatusBar) + _bgPosition2.Value,
                ParallaxLayer.Foreground => LevelTileHeight + (includeStatusBar? Constants.StatusBarTiles : 0) - EndTiles,
                _ => 0
            };

        public int GetParallaxLayerPixel(ParallaxLayer layer, bool includeStatusBar) =>
            GetParallaxLayerTile(layer, includeStatusBar) * _specs.TileHeight;

        public byte GetBgPosition1()
        {
            //todo, may depend on scroll style
            if (_bgPosition1.Value == 0)
                return 0;
            else
                return (byte)(2 + _bgPosition1.Value*2);
        }

        public SceneDefinition(SystemMemoryBuilder memoryBuilder, 
            Specs specs,
            ScrollStyle scrollStyle,
            LevelShape levelShape,
            Theme theme,
            EnemyGroup enemyGroup,
            byte left=0,
            byte top=0,
            byte right=0,
            byte bottom=0,
            byte bg1=0,
            byte bg2=0)
        {
            _specs = specs;
            _systemMemory = memoryBuilder.Memory;

            _scrollStyle = new TwoBitEnum<ScrollStyle>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 0);
            _levelShape = new TwoBitEnum<LevelShape>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 2);
            _bgPosition1 = new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 4);
            _bgPosition2 = new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 6);
            memoryBuilder.AddByte();

            _theme = new NibbleEnum<Theme>(new LowNibble(memoryBuilder));
            _enemies = new NibbleEnum<EnemyGroup>(new HighNibble(memoryBuilder));
            memoryBuilder.AddByte();

            _begin = new LowNibble(memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            _end = new HighNibble(memoryBuilder.CurrentAddress, memoryBuilder.Memory);

            _left = new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 0);
            _top = new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 2);
            _right = new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 4);
            _bottom  = new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 6);
            memoryBuilder.AddByte();


            _scrollStyle.Value = scrollStyle;
            _levelShape.Value = levelShape;
            _theme.Value = theme;
            _enemies.Value = enemyGroup;
             
            if (scrollStyle == ScrollStyle.Horizontal)
            {
                _begin.Value = top;
                _end.Value = bottom;
            }
            else if(scrollStyle == ScrollStyle.Vertical)
            {
                _begin.Value = left;
                _end.Value = right;
            }
            else
            {
                _left.Value = left;
                _top.Value = top;
                _right.Value = right;
                _bottom.Value = bottom;
            }

            _bgPosition1.Value = bg1;
            _bgPosition2.Value = bg2;
        }

        public SceneDefinition(int address, SystemMemory systemMemory, Specs specs)
        {
            _specs = specs;
            _scrollStyle = new TwoBitEnum<ScrollStyle>(systemMemory, address, 0);
            _levelShape = new TwoBitEnum<LevelShape>(systemMemory, address, 2);
            _bgPosition1 = new TwoBit(systemMemory, address, 4);
            _bgPosition2 = new TwoBit(systemMemory, address, 6);

            _theme = new NibbleEnum<Theme>(new LowNibble(address + 1, systemMemory));
            _enemies = new NibbleEnum<EnemyGroup>(new HighNibble(address + 1, systemMemory));
      
            _begin = new LowNibble(address + 2, systemMemory);
            _end = new HighNibble(address + 2, systemMemory);

            _left = new TwoBit(systemMemory, address + 2, 0);
            _top = new TwoBit(systemMemory, address + 2, 2);
            _right = new TwoBit(systemMemory, address + 2, 4);
            _bottom = new TwoBit(systemMemory, address + 2, 6);
        }

        public SceneDefinition(Level level, SystemMemory memory, Specs specs)
            :this(memory.GetAddress(AddressLabels.SceneDefinitions) + ((int)level * SceneDefinition.Bytes), memory,specs)
        { 
        }

        public int GetGroundFillTile(int row, int col)
        {
            int groundTileCount = 2;

            int index = row % groundTileCount;
            if (col % 2 == 0)
                index = (index + 1) % groundTileCount;

            return GroundFillStart + index;            
        }

        public int GetGroundTopTile(int col)
        {
            int groundTileCount = 2;

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

        //todo, need to change how we do palettes
        public byte GetPalette(ScenePartType scenePartType)
        {
            return scenePartType switch {
                ScenePartType.EnemyType1 => 2,
                ScenePartType.EnemyType2 => 3,
                _ => 0
            };
        }
    }
}
