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
        Interior,
        Flat,
        Forest
    }
    public enum BackgroundPart
    {
        Top,
        Upper,
        Middle,
        Lower,
        Bottom,
        Left,
        Right
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
        HighVariance=2,
        TwoByTwoVariance=3,

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
        private TwoBit _enemy2;
        private readonly TwoBitEnum<CornerStairStyle> _cornerStairStyle;
        private readonly TwoBitEnum<HorizontalScrollStyle> _horizontalScrollStyle; // shared with above

        //byte 1 + 6 bits of byte 1
        private DenseTwoBitArray _layerPositions; // 8 + 6 bits

        // last 2 bits of byte 1
        private GameByteEnum<SpriteGroup> _spriteGroup; // 2 bits

        //byte 2
        private GameByteEnum<EnemyIndex> _enemy1; // 3 bits               
        private GameByteEnum<ThemeType> _theme; // 5 bits
       

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

        public int LeftTiles => _layerPositions[(int)BackgroundPart.Left] * 2;

        public int RightTiles => _layerPositions[(int)BackgroundPart.Right] * 2;

        public int TopTiles => _layerPositions[(int)BackgroundPart.Top] * 2;

        public int UpperTiles => _layerPositions[(int)BackgroundPart.Upper] * 2;

        public int MiddleTiles => _layerPositions[(int)BackgroundPart.Middle] * 2;

        public int LowerTiles => _layerPositions[(int)BackgroundPart.Lower] * 2;

        public int BottomTiles => _layerPositions[(int)BackgroundPart.Bottom] * 2;

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
                LevelShape.BigStair => 12,
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
        public int GetBackgroundLayerTile(BackgroundPart layer, bool includeStatusBar)
        {
            int tile = includeStatusBar ? 2 : 0;
            for(BackgroundPart x = BackgroundPart.Top; x <= layer; x++)
            {
                tile += x switch {
                    BackgroundPart.Top => TopTiles,
                    BackgroundPart.Upper => UpperTiles,
                    BackgroundPart.Middle => MiddleTiles,
                    BackgroundPart.Lower => LowerTiles,
                    BackgroundPart.Bottom => BottomTiles,
                    _ => 0,
                };
            }
            return tile;
        }

        public int GetBackgroundLayerPixel(BackgroundPart layer, bool includeStatusBar) =>
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
            byte upper,
            byte mid=1,
            byte lower=1)
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
                upper,
                mid,
                lower);
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
                left: left,
                top: top,
                right: right,
                bottom: bottom,
                upper: bgPosition,
                mid: 1,
                lower:1,
                extraStyle: (byte)cornerStairStyle);
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
                left:left,
                top: top,
                right: right,
                bottom: bottom,
                upper: bgPosition,
                mid: 1,
                lower:1);
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
                1);
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
            byte upper,
            byte middle=1,
            byte lower=1,
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
                left: 0,
                top: top,
                right: 0,
                bottom: bottom,
                upper: upper,
                mid: middle,
                lower: lower,
                extraStyle: (byte)style
               );
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
                left: left,
                top: 0,
                right: right,
                bottom: 2,
                upper: 1,
                mid: 1,
                lower: 1);
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
                left: left,
                top: top,
                right: right,
                bottom: bottom,
                upper: 1,
                mid: 1,
                lower: 1);
               
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
            byte upper=1,
            byte mid=0,
            byte lower=0,
            byte extraStyle=0)
        {
            _specs = specs;
            _systemMemory = memoryBuilder.Memory;

            //byte 0
            _scrollStyle = new TwoBitEnum<ScrollStyle>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 0);
            _levelShape = new TwoBitEnum<LevelShape>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 2);
            _enemy2 = new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 4);
            _cornerStairStyle = new TwoBitEnum<CornerStairStyle>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 6);
            _horizontalScrollStyle = new TwoBitEnum<HorizontalScrollStyle>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 6);
            memoryBuilder.AddByte();

            //bytes 1 and 2
            _layerPositions = new DenseTwoBitArray(memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            _spriteGroup = new GameByteEnum<SpriteGroup>(new MaskedByte(memoryBuilder.CurrentAddress+1, Bit.Left2, memoryBuilder.Memory, leftShift: 6));
            memoryBuilder.AddByte();
            memoryBuilder.AddByte();

            //byte 3
            _theme = new GameByteEnum<ThemeType>(new MaskedByte(memoryBuilder.CurrentAddress, Bit.Right5, memoryBuilder.Memory));
            _enemy1 = new GameByteEnum<EnemyIndex>(new MaskedByte(memoryBuilder.CurrentAddress, Bit.Left3, memoryBuilder.Memory, leftShift: 5));
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

            _layerPositions[(int)BackgroundPart.Lower] = lower;
            _layerPositions[(int)BackgroundPart.Middle] = mid;
            _layerPositions[(int)BackgroundPart.Upper] = upper;
            _layerPositions[(int)BackgroundPart.Top] = top;
            _layerPositions[(int)BackgroundPart.Bottom] = bottom;
            _layerPositions[(int)BackgroundPart.Middle] = mid;
            _layerPositions[(int)BackgroundPart.Left] = left;
            _layerPositions[(int)BackgroundPart.Right] = right;

            // shared with corner stair style
            _horizontalScrollStyle.Value = (HorizontalScrollStyle)extraStyle;
        }

        public SceneDefinition(int address, SystemMemory systemMemory, Specs specs)
        {
            _specs = specs;

            //byte 0
            _scrollStyle = new TwoBitEnum<ScrollStyle>(systemMemory, address, 0);
            _levelShape = new TwoBitEnum<LevelShape>(systemMemory, address, 2);
            _enemy2 = new TwoBit(systemMemory, address, 4);
            _horizontalScrollStyle = new TwoBitEnum<HorizontalScrollStyle>(systemMemory, address, 6);
            _cornerStairStyle = new TwoBitEnum<CornerStairStyle>(systemMemory, address, 6);

            //byte 1 and 2
            _layerPositions = new DenseTwoBitArray(address + 1, systemMemory);
            _spriteGroup = new GameByteEnum<SpriteGroup>(new MaskedByte(address + 2, Bit.Left2, systemMemory, leftShift: 6));

            //byte 3
            _theme = new GameByteEnum<ThemeType>(new MaskedByte(address + 3, Bit.Right5, systemMemory));           
            _enemy1 = new GameByteEnum<EnemyIndex>(new MaskedByte(address + 3, Bit.Left3, systemMemory, leftShift: 5));            
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

        public int LevelTileWidth
        {
            get
            {
                if (_theme.Value == ThemeType.MistAutoscroll)
                    return _specs.NameTableWidth;
                else if (HasSprite(SpriteType.Plane))
                    return (_specs.ScreenWidth / _specs.TileWidth) * 3;
                else
                {
                    return ScrollStyle switch {
                        ScrollStyle.None => _specs.ScreenWidth / _specs.TileWidth,
                        ScrollStyle.Vertical => _specs.ScreenWidth / _specs.TileWidth,
                        ScrollStyle.NameTable => _specs.NameTableWidth,
                        ScrollStyle.Horizontal => (_specs.ScreenWidth / _specs.TileWidth) * 4,
                        _ => throw new NotImplementedException()
                    };
                }
            }
        }

        public int LevelTileHeight =>
            _theme.Value == ThemeType.MistAutoscroll ?
              (_specs.NameTableHeight - StatusBarTiles) :
                 ScrollStyle switch {
                     ScrollStyle.None => (_specs.ScreenHeight / _specs.TileHeight) - StatusBarTiles,
                     ScrollStyle.Horizontal => (_specs.ScreenHeight / _specs.TileHeight) - StatusBarTiles,
                     ScrollStyle.NameTable => _specs.NameTableHeight - StatusBarTiles,
                     ScrollStyle.Vertical => (_specs.ScreenHeight / _specs.TileHeight) * 4,
                     _ => throw new NotImplementedException()
                 };
    }
}
