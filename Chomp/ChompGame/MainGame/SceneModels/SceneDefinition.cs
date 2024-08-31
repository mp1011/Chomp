using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels.Themes;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChompGame.MainGame.SceneModels
{


    public enum CornerStairStyle : byte
    {
        OneBlockDouble,
        TwoBlockDouble,
        TwoBlockLeft,
        TwoBlockRight
    }

    public enum HorizontalScrollStyle : byte
    {
        HorizonAndSky,
        Interior
    }

    public enum BackgroundLayer
    {
        Begin,
        Back1,
        Back2,
        ForegroundStart,
        Foreground
    }

    public enum PlayerSpriteGroup : byte
    {
        Normal,       
        PlaneTakeoff,
        PlaneAutoscroll
    }


    public enum ScrollStyle : byte
    {
        None=0,
        NameTable=1,
        Horizontal=2,
        Vertical=3
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
        public const int Bytes = 4;

        public int StatusBarTiles => Constants.StatusBarTiles;

        private readonly SystemMemory _systemMemory;
        private readonly Specs _specs;

        //byte 0
        private readonly TwoBitEnum<ScrollStyle> _scrollStyle;
        private readonly TwoBitEnum<LevelShape> _levelShape;
        private readonly TwoBit _bgPosition1;
     
        //
        private readonly TwoBitEnum<CornerStairStyle> _cornerStairStyle;
        private readonly TwoBitEnum<HorizontalScrollStyle> _horizontalScrollStyle;


        //byte 1
        private GameByteEnum<ThemeType> _theme;

        //byte 2
        private GameByteEnum<EnemyIndex> _enemy1; //3
        private MaskedByte _enemy2; //2
        private GameByteEnum<SpriteGroup> _spriteGroup; //3
             
        //byte 3 (four TwoBits or two Nibbles)
        private TwoBit _left;
        private TwoBit _top;
        private TwoBit _right;
        private TwoBit _bottom;
        private Nibble _begin;
        private Nibble _end;

        public int Address => _scrollStyle.Address;

        public bool IsMidBossScene => HasSprite(SpriteType.Chomp);
        public bool IsLevelBossScene => HasSprite(SpriteType.LevelBoss);

        public int GroundFillStart => 8;

        public int GroundFillEnd => GroundFillStart + 1;

        public int GroundLeftCorner => GroundFillEnd + 1;

        public int GroundTopBegin => GroundLeftCorner + 1;

        public int GroundTopEnd => GroundTopBegin + 1;

        public int GroundRightCorner => GroundTopEnd + 1;

        public int LeftTileIndex => GroundRightCorner + 1;

        public int RightTileIndex => LeftTileIndex + 1;

        public LevelShape LevelShape => _levelShape.Value;

        public ScrollStyle ScrollStyle => _scrollStyle.Value;

        public SpriteGroup SpriteGroup => _spriteGroup.Value;

        public bool IsAutoScroll => _spriteGroup.Value == SpriteGroup.Plane;

        public int LeftTiles => _scrollStyle.Value switch {
            ScrollStyle.Vertical => _begin.Value * 2,
            _ => _left.Value * 2 };

        public int RightTiles => _scrollStyle.Value switch {
            ScrollStyle.Vertical => _end.Value * 2,
            _ => _right.Value * 2
        };

        public int TopTiles => _scrollStyle.Value switch {
            ScrollStyle.Horizontal => _begin.Value * 2,
            ScrollStyle.Vertical => 0,
            _ => _top.Value * 2
        };

        public int BottomTiles => _scrollStyle.Value switch {
            ScrollStyle.Horizontal => _end.Value * 2,
            ScrollStyle.Vertical => 4,
            _ => _bottom.Value * 2
        };

        public int LeftEdgeFloorTiles => _scrollStyle.Value switch {
            ScrollStyle.None => _levelShape.Value switch {
                LevelShape.CornerStairs => _cornerStairStyle.Value switch {
                    CornerStairStyle.OneBlockDouble => BottomTiles + 2,
                    CornerStairStyle.TwoBlockDouble => BottomTiles + 4,
                    CornerStairStyle.TwoBlockLeft => BottomTiles + 4, //todo, check stair generation
                    _ => BottomTiles
                },
                _ => BottomTiles
            },
            _ => BottomTiles
        };

        public int RightEdgeFloorTiles => _scrollStyle.Value switch {
            ScrollStyle.None => _levelShape.Value switch {
                LevelShape.CornerStairs => _cornerStairStyle.Value switch {
                    CornerStairStyle.OneBlockDouble => BottomTiles + 4,
                    CornerStairStyle.TwoBlockDouble => BottomTiles + 4,
                    CornerStairStyle.TwoBlockRight => BottomTiles + 4, //todo, check stair generation
                    _ => BottomTiles
                },
                _ => BottomTiles
            },
            _ => BottomTiles
        };

        public ThemeType Theme => _theme.Value;
        public ThemeSetup CreateThemeSetup(ChompGameModule chompGameModule) => 
            ThemeSetup.Create(Theme, _specs, this, chompGameModule);
        public CornerStairStyle CornerStairStyle => _cornerStairStyle.Value;
        public HorizontalScrollStyle HorizontalScrollStyle => _horizontalScrollStyle.Value;
        public FallCheck SpriteFallCheck => ScrollStyle switch {
                    ScrollStyle.Horizontal => FallCheck.ScreenHeight,
                    ScrollStyle.None => FallCheck.ScreenHeight,
                    ScrollStyle.Vertical => FallCheck.None,
                    _ => FallCheck.WrapAround };

        public IEnumerable<SpriteType> Sprites
        {
            get
            {
                yield return SpriteType.Player;

                switch(_spriteGroup.Value)
                {
                    case SpriteGroup.Normal:
                        yield return _enemy1.Value.ToSpriteType();
                        yield return (_enemy1.Value + _enemy2.Value + 1).ToSpriteType();
                        break;
                    case SpriteGroup.PlaneTakeoff:
                    case SpriteGroup.Plane:
                        yield return SpriteType.Plane;

                        if (_enemy1.Value == EnemyIndex.Midboss)
                            yield return SpriteType.Chomp;
                        else
                        {
                            yield return _enemy1.Value.ToSpriteType();
                            yield return (_enemy1.Value + _enemy2.Value + 1).ToSpriteType();
                        }
                        break;
                    case SpriteGroup.Boss:
                        yield return _enemy1.Value.ToBossSpriteType();
                        break;
                }
            }
        }
        public bool HasSprite(SpriteType spriteType) => Sprites.Contains(spriteType);
        public int GetBackgroundLayerTile(BackgroundLayer layer, bool includeStatusBar) =>
            layer switch {
                BackgroundLayer.Begin => (includeStatusBar ? StatusBarTiles : 0) + TopTiles,
                BackgroundLayer.Back1 => GetBackgroundLayerTile(BackgroundLayer.Begin, includeStatusBar) + (_bgPosition1.Value*2),
                BackgroundLayer.Back2 => GetBackgroundLayerTile(BackgroundLayer.Back1, includeStatusBar) + 2,
                BackgroundLayer.ForegroundStart => GetBackgroundLayerTile(BackgroundLayer.Back2, includeStatusBar) + 2,
                BackgroundLayer.Foreground => LevelTileHeight + (includeStatusBar? Constants.StatusBarTiles : 0) - BottomTiles,                
                _ => 0
            };

        public int GetBackgroundLayerPixel(BackgroundLayer layer, bool includeStatusBar) =>
            GetBackgroundLayerTile(layer, includeStatusBar) * _specs.TileHeight;

        public static SceneDefinition NoScrollFlat(
            SystemMemoryBuilder memoryBuilder,
            Specs specs, 
            ThemeType theme, 
            SpriteGroup spriteGroup,
            EnemyIndex enemy1,
            EnemyIndex enemy2,
            byte left, 
            byte top, 
            byte right, 
            byte bottom,
            byte bgPosition)
        {
            return new SceneDefinition(memoryBuilder, 
                specs, 
                ScrollStyle.None, 
                LevelShape.Flat,
                theme,
                enemy1,
                enemy2,
                spriteGroup,
                left,
                top,
                right,
                bottom,
                bgPosition);
        }

        public static SceneDefinition NoScrollCornerStairs(
           SystemMemoryBuilder memoryBuilder,
           Specs specs,
           ThemeType theme,
           EnemyIndex enemy1,
           EnemyIndex enemy2,
           SpriteGroup spriteGroup,
           byte left,
           byte top,
           byte right,
           byte bottom,
           byte bgPosition,
           CornerStairStyle cornerStairStyle)
        {
            return new SceneDefinition(memoryBuilder,
                specs,
                ScrollStyle.None,
                LevelShape.CornerStairs,
                theme,
                enemy1,
                enemy2,
                spriteGroup,
                left,
                top,
                right,
                bottom,
                bgPosition,
                (byte)cornerStairStyle);
        }

        public static SceneDefinition NoScrollBigStairs(
           SystemMemoryBuilder memoryBuilder,
           Specs specs,
           ThemeType theme,
           EnemyIndex enemy1,
           EnemyIndex enemy2,
           SpriteGroup spriteGroup,
           byte left,
           byte top,
           byte right,
           byte bottom,
           byte bgPosition)
        {
            return new SceneDefinition(memoryBuilder,
                specs,
                ScrollStyle.None,
                LevelShape.BigStair,
                theme,
                enemy1,
                enemy2,
                spriteGroup,
                left,
                top,
                right,
                bottom,
                bgPosition);
        }

        public static SceneDefinition NoScrollTShape(
           SystemMemoryBuilder memoryBuilder,
           Specs specs,
           ThemeType theme,
           EnemyIndex enemy1,
           EnemyIndex enemy2,
           SpriteGroup spriteGroup,
           byte leftY,
           byte rightY,
           byte pitX,
           byte hallSize,
           byte bgPosition)
        {
            return new SceneDefinition(memoryBuilder,
                specs,
                ScrollStyle.None,
                LevelShape.TShape,
                theme,
                enemy1,
                enemy2,
                spriteGroup,
                leftY,
                pitX,
                rightY,
                hallSize,
                bgPosition,
                0);
        }

        public static SceneDefinition HorizontalScroll(
            SystemMemoryBuilder memoryBuilder,
            Specs specs,
            ThemeType theme,
            LevelShape variance,
            EnemyIndex enemy1,
            EnemyIndex enemy2,
            SpriteGroup spriteGroup,
            byte top,
            byte bottom,
            byte bgPosition1,
            HorizontalScrollStyle style = HorizontalScrollStyle.HorizonAndSky)
        {
            return new SceneDefinition(memoryBuilder,
                specs,
                ScrollStyle.Horizontal,
                variance,
                theme,
                enemy1,
                enemy2,
                spriteGroup,
                0,
                top,
                0,
                bottom,
                bgPosition1,
                (byte)style);
        }
        public static SceneDefinition VerticalScroll(
            SystemMemoryBuilder memoryBuilder,
            Specs specs,
            ThemeType theme,
            LevelShape shape,
            EnemyIndex enemy1,
            EnemyIndex enemy2,
            SpriteGroup spriteGroup,
            byte left,
            byte right)
        {
            return new SceneDefinition(memoryBuilder,
                specs,
                ScrollStyle.Vertical,
                shape,
                theme,
                enemy1,
                enemy2,
                spriteGroup,
                left,
                0,
                right,
                0);
        }

        public static SceneDefinition BossScene(
            SystemMemoryBuilder memoryBuilder,
            Specs specs,
            ThemeType theme)
        {
            return new SceneDefinition(memoryBuilder,
               specs,
               ScrollStyle.NameTable,
               LevelShape.Flat,
               theme,
               EnemyIndex.Bird,//todo
               EnemyIndex.Ogre,//todo
               SpriteGroup.Boss,
               0,
               0,
               0,
               1);
        }

        public static SceneDefinition NametableScroll(
            SystemMemoryBuilder memoryBuilder, 
            Specs specs,
            ThemeType theme,
            LevelShape shape,
            EnemyIndex enemy1,
            EnemyIndex enemy2,
            SpriteGroup spriteGroup,
            byte left,
            byte top, 
            byte right,
            byte bottom)
        {
            return new SceneDefinition(memoryBuilder,
                specs,
                ScrollStyle.NameTable,
                shape, 
                theme,
                enemy1,
                enemy2,
                spriteGroup,
                left,
                top,
                right,
                bottom);
        }

        private SceneDefinition(SystemMemoryBuilder memoryBuilder, 
            Specs specs,
            ScrollStyle scrollStyle,
            LevelShape levelShape,
            ThemeType theme,
            EnemyIndex enemy1,
            EnemyIndex enemy2,
            SpriteGroup spriteGroup,
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

            _cornerStairStyle = new TwoBitEnum<CornerStairStyle>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 6);
            _horizontalScrollStyle = new TwoBitEnum<HorizontalScrollStyle>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 6);

            memoryBuilder.AddByte();

            _theme = new GameByteEnum<ThemeType>(memoryBuilder.AddByte());

            _enemy1 = new GameByteEnum<EnemyIndex>(new MaskedByte(memoryBuilder.CurrentAddress, Bit.Right3, memoryBuilder.Memory));
            _enemy2 = new MaskedByte(memoryBuilder.CurrentAddress, (Bit)24, memoryBuilder.Memory, leftShift: 3);
            _spriteGroup = new GameByteEnum<SpriteGroup>(new MaskedByte(memoryBuilder.CurrentAddress, Bit.Left3, memoryBuilder.Memory, leftShift: 5));
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

            _spriteGroup.Value = spriteGroup;
            _enemy1.Value = enemy1;
            _enemy2.Value = (byte)((enemy2 - enemy1) - 1);
            if (enemy2 <= enemy1 && enemy1 != EnemyIndex.Boss && enemy1 != EnemyIndex.Midboss)
                throw new Exception("Second enemy must have higher index");

            if (enemy2 > enemy1+4)
                throw new Exception("Second enemy can't be more than 4 indices away from first");


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
            _horizontalScrollStyle.Value = (HorizontalScrollStyle)bg2;

        }

        public SceneDefinition(int address, SystemMemory systemMemory, Specs specs)
        {
            _specs = specs;
            _scrollStyle = new TwoBitEnum<ScrollStyle>(systemMemory, address, 0);
            _levelShape = new TwoBitEnum<LevelShape>(systemMemory, address, 2);
            _bgPosition1 = new TwoBit(systemMemory, address, 4);
            _horizontalScrollStyle = new TwoBitEnum<HorizontalScrollStyle>(systemMemory, address, 6);
            _cornerStairStyle = new TwoBitEnum<CornerStairStyle>(systemMemory, address, 6);

            _theme = new GameByteEnum<ThemeType>(new GameByte(address+1, systemMemory));

            _enemy1 = new GameByteEnum<EnemyIndex>(new MaskedByte(address + 2, Bit.Right3, systemMemory));
            _enemy2 = new MaskedByte(address + 2, (Bit)24, systemMemory, leftShift: 3);
            _spriteGroup = new GameByteEnum<SpriteGroup>(new MaskedByte(address + 2, Bit.Left3, systemMemory, leftShift: 5));
      
            _begin = new LowNibble(address + 3, systemMemory);
            _end = new HighNibble(address + 3, systemMemory);

            _left = new TwoBit(systemMemory, address + 3, 0);
            _top = new TwoBit(systemMemory, address + 3, 2);
            _right = new TwoBit(systemMemory, address + 3, 4);
            _bottom = new TwoBit(systemMemory, address + 3, 6);
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
            HasSprite(SpriteType.Plane) ? (_specs.ScreenWidth / _specs.TileWidth) * 3
            :   ScrollStyle switch {
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
    }
}
