using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame
{
    public enum WorldSpriteStatus : byte
    {
        Inactive=0,
        Hidden=1,
        Active=2
    }

    public enum BoundsCheck : byte
    {
        InBounds=0,
        OutOfBounds=1,
        FarOutOfBounds=2
    }

    class WorldSprite 
    {
        private SpriteTileTable _spriteTileTable;
        protected readonly Specs _specs;
        protected SpriteDefinition _spriteDefinition;
        protected readonly SpritesModule _spritesModule;
        protected readonly WorldScroller _scroller;
        private readonly TwoBitEnum<WorldSpriteStatus> _status;
        private GameByteEnum<SpriteTileIndex> _tileIndex;
        public SpriteTileIndex TileIndex => _tileIndex.Value;
      
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

        public WorldSprite(
            SystemMemoryBuilder memoryBuilder,
            SpriteTileTable spriteTileTable,
            Specs specs,
            SpriteDefinition spriteDefinition,
            SpritesModule spritesModule,
            WorldScroller scroller,
            SpriteTileIndex index)
        {
            _spriteTileTable = spriteTileTable;
            SpriteIndex = memoryBuilder.AddMaskedByte(Bit.Right5);
            _status = new TwoBitEnum<WorldSpriteStatus>(memoryBuilder.Memory, SpriteIndex.Address, 6);
            _spriteDefinition = spriteDefinition;

            _position = memoryBuilder.AddExtendedPoint(); // 6 bits free here
            _tileIndex = new GameByteEnum<SpriteTileIndex>(
                new MaskedByte(memoryBuilder.CurrentAddress, Bit.Left6, memoryBuilder.Memory, 2));
            memoryBuilder.AddByte();
            _scroller = scroller;
            _specs = specs;
            _spritesModule = spritesModule;

            _tileIndex.Value = index;
        }

        public Sprite GetSprite() => _spritesModule.GetSprite(SpriteIndex.Value);

        public virtual void UpdateSprite()
        {
            if (Status != WorldSpriteStatus.Active)
                return;

            var sprite = GetSprite();

            int spriteX = (X - _scroller.WorldScrollPixelX);
            int spriteY = (Y - _scroller.WorldScrollPixelY);

            //todo, how to handle out of bounds?
            if (spriteX < 0)
                spriteX = 0;
            if (spriteY < 0)
                spriteY = 0;

            sprite.X = (byte)spriteX;
            sprite.Y = (byte)spriteY;
        }

        public BoundsCheck CheckInBounds()
        {
            int nearThreshold = 12;
            int farThreshold = 32;

            if (X >= _scroller.WorldScrollPixelX - nearThreshold
                && X < _scroller.WorldScrollPixelX + _specs.NameTablePixelWidth + nearThreshold
                && Y >= _scroller.WorldScrollPixelY - nearThreshold
                && Y < _scroller.WorldScrollPixelY + _specs.NameTablePixelHeight + nearThreshold)
            {
                return BoundsCheck.InBounds;
            }

            if (X >= _scroller.WorldScrollPixelX - farThreshold
              && X < _scroller.WorldScrollPixelX + _specs.NameTablePixelWidth + farThreshold
              && Y >= _scroller.WorldScrollPixelY - farThreshold
              && Y < _scroller.WorldScrollPixelY + _specs.NameTablePixelHeight + farThreshold)
            {
                return BoundsCheck.OutOfBounds;
            }

            return BoundsCheck.FarOutOfBounds;
        }

        public void ConfigureSprite(Sprite sprite)
        {
            sprite.Tile = _spriteTileTable.GetTile(_tileIndex.Value);
            sprite.Tile2Offset = _spriteDefinition.SecondTileOffset;
            sprite.SizeX = _spriteDefinition.SizeX;
            sprite.SizeY = _spriteDefinition.SizeY;
            sprite.Visible = true;
        }

        public void Show()
        {
            if (Status == WorldSpriteStatus.Active)
                return;

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
            if (Status == WorldSpriteStatus.Active)
            {
                GetSprite().Tile = 0;
            }

            Status = WorldSpriteStatus.Hidden;
        }

        public void Destroy()
        {
            if (Status == WorldSpriteStatus.Active)
            {
                GetSprite().Tile = 0;
            }

            Status = WorldSpriteStatus.Inactive;
        }

    }

    class MovingWorldSprite : WorldSprite
    {
        public PrecisionMotion Motion { get; }
        public int XSpeed => Motion.XSpeed;
        public int YSpeed => Motion.YSpeed;

        public MovingWorldSprite(
            SystemMemoryBuilder memoryBuilder,
            SpriteTileTable spriteTileTable,
            Specs specs,
            SpriteDefinition spriteDefinition,
            SpritesModule spritesModule,
            PrecisionMotion motion, 
            WorldScroller scroller,
            SpriteTileIndex tileIndex) : base(memoryBuilder,spriteTileTable,specs,spriteDefinition, spritesModule, scroller, tileIndex)
        {           
            Motion = motion;
        }
    }
}
