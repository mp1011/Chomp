using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame
{
    public enum WorldSpriteStatus : byte
    {
        Inactive=0,
        Hidden=1,
        Active=2,
        Dying=3
    }

    public enum BoundsCheck : byte
    {
        InBounds=0,
        OutOfBounds=1,
        FarOutOfBounds=2
    }

    class WorldSprite : IWithPosition
    {
        private ChompGameModule _gameModule;
        private SpriteTileTable _spriteTileTable;
        protected readonly Specs _specs;
        protected readonly SpriteDefinition _spriteDefinition;
        protected readonly SpritesModule _spritesModule;
        protected readonly WorldScroller _scroller;
        private readonly TwoBitEnum<SpritePalette> _palette;
        private readonly TwoBitEnum<WorldSpriteStatus> _status;
        private GameByteEnum<SpriteTileIndex> _tileIndex;

        private GameBit _visible;

        public bool Visible
        {
            get => _visible.Value;
            set => _visible.Value = value;
        }

        public SpriteTileIndex TileIndex
        {
            get => _tileIndex.Value;
            set => _tileIndex.Value = value;
        }
      
        public GameByte SpriteIndex { get; }

        public byte Tile
        {
            get => GetSprite().Tile;
            set => GetSprite().Tile = value;
        }

        public WorldSpriteStatus Status
        {
            get => _status.Value;
            set => _status.Value = value;
        }

        public bool FlipX
        {
            get => GetSprite().FlipX;
            set => GetSprite().FlipX = value;
        }

        public bool FlipY
        {
            get => GetSprite().FlipY;
            set => GetSprite().FlipY = value;
        }

        private ExtendedPoint _position;

        public int X
        {
            get => _position.X;
            set => _position.X = value;
        }

        public int Y
        {
            get => _position.Y;
            set => _position.Y = value;
        }

        public Point TopLeft => new Point(X, Y);
        public Point BottomRight
        {
            get
            {
                var sprite = GetSprite();
                return new Point(X + sprite.Width, Y + sprite.Height);
            }
        }

        public Rectangle Bounds
        {
            get
            {
                var sprite = GetSprite();
                return new Rectangle(X, Y, sprite.Width, sprite.Height);
            }
        }

        public Point Center
        {
            get => Bounds.Center;
            set
            {
                var sprite = GetSprite();
                X = value.X - sprite.Width / 2;
                Y = value.Y - sprite.Height / 2;
            }
        }

        public AnimationStyle AnimationStyle => _spriteDefinition.AnimationStyle;

        public WorldSprite(
            SystemMemoryBuilder memoryBuilder,
            SpriteTileTable spriteTileTable,
            Specs specs,
            SpriteDefinition spriteDefinition,
            SpritesModule spritesModule,
            WorldScroller scroller,
            SpriteTileIndex index,
            TwoBitEnum<SpritePalette> palette)
        {
            _spriteTileTable = spriteTileTable;
            SpriteIndex = memoryBuilder.AddMaskedByte(Bit.Right5);
            _status = new TwoBitEnum<WorldSpriteStatus>(memoryBuilder.Memory, SpriteIndex.Address, 5);
            _visible = new GameBit(SpriteIndex.Address, Bit.Bit7, memoryBuilder.Memory);

            _spriteDefinition = spriteDefinition;

            _position = memoryBuilder.AddExtendedPoint(); // 6 bits free here
            _tileIndex = new GameByteEnum<SpriteTileIndex>(
                new MaskedByte(memoryBuilder.CurrentAddress, Bit.Left6, memoryBuilder.Memory, 2));
            memoryBuilder.AddByte();
            _scroller = scroller;
            _specs = specs;
            _spritesModule = spritesModule;

            _tileIndex.Value = index;
            _palette = palette;
        }

        public Sprite GetSprite() => _spritesModule.GetSprite(SpriteIndex.Value);

        public void UpdateSprite()
        {
            if (Status != WorldSpriteStatus.Active && Status != WorldSpriteStatus.Dying)
                return;

            var sprite = GetSprite();
            if (_scroller.DistanceFromViewpane(Bounds) > 8)
            {
                sprite.Visible = false;
            }
            else
            {
                sprite.Visible = Visible;
                sprite.X = (byte)(X % _specs.NameTablePixelWidth);
                sprite.Y = (byte)(Y % _specs.NameTablePixelHeight);
            }
        }

        public int XDistanceTo(WorldSprite other)
        {
            return Math.Abs(Center.X - other.Center.X);
        }

        public BoundsCheck CheckInBounds()
        {
            int distanceFromViewpane = _scroller.DistanceFromViewpane(Bounds);

            int nearThreshold = 12;
            int farThreshold = 32;

            if (distanceFromViewpane <= nearThreshold)
                return BoundsCheck.InBounds;
            else if (distanceFromViewpane <= farThreshold)
                return BoundsCheck.OutOfBounds;
            else
                return BoundsCheck.FarOutOfBounds;
        }

        public void ConfigureSprite(Sprite sprite)
        {
            sprite.Tile = _spriteTileTable.GetTile(_tileIndex.Value);
            sprite.Tile2Offset = _spriteDefinition.SecondTileOffset;
            sprite.SizeX = _spriteDefinition.SizeX;
            sprite.SizeY = _spriteDefinition.SizeY;
            sprite.Visible = true;
            sprite.Palette = _palette.Value;
            Visible = true;

            GameDebug.DebugLog($"Configure sprite #{SpriteIndex}, Tile={sprite.Tile}, SX={sprite.SizeX}, SY={sprite.SizeY}, T20={sprite.Tile2Offset},,,", DebugLogFlags.SpriteSpawn);
        }

        public void Show()
        {
            if (Status == WorldSpriteStatus.Active)
                return;

            GameDebug.DebugLog($"Showing Sprite {SpriteIndex}", DebugLogFlags.SpriteSpawn);

            var newSpriteIndex = _spritesModule.GetFreeSpriteIndex();
            if (newSpriteIndex == 255)
            {
                Status = WorldSpriteStatus.Hidden;
            }
            else
            {
                SpriteIndex.Value = newSpriteIndex;
                Status = WorldSpriteStatus.Active;
                ConfigureSprite(GetSprite());
            }
        }

        public void Hide()
        {
            GameDebug.DebugLog($"Hiding Sprite {SpriteIndex}", DebugLogFlags.SpriteSpawn);

            if (Status == WorldSpriteStatus.Active)
            {
                GetSprite().Tile = 0;
            }

            Status = WorldSpriteStatus.Hidden;
        }

        public void Destroy()
        {
            if (Status == WorldSpriteStatus.Active || Status == WorldSpriteStatus.Dying)
            {
                GetSprite().Tile = 0;
                GameDebug.DebugLog($"Sprite {SpriteIndex} erased", DebugLogFlags.SpriteSpawn);
            }

            Status = WorldSpriteStatus.Inactive;
        }
    }
}
