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
        private readonly Specs _specs; 
        private SpriteDefinition _spriteDefinition;
        private readonly SpritesModule _spritesModule;
        private readonly WorldScroller _scroller;
        private readonly TwoBitEnum<WorldSpriteStatus> _status;

        public GameByte SpriteIndex { get; }

        public PrecisionMotion Motion { get; }

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

        public int XSpeed => Motion.XSpeed;
        public int YSpeed => Motion.YSpeed;

        public WorldSprite(
            SystemMemoryBuilder memoryBuilder,
            Specs specs,
            SpriteDefinition spriteDefinition,
            SpritesModule spritesModule,
            PrecisionMotion motion, 
            WorldScroller scroller)
        {
            SpriteIndex = memoryBuilder.AddMaskedByte(Bit.Right5);
            _status = new TwoBitEnum<WorldSpriteStatus>(memoryBuilder.Memory, SpriteIndex.Address, 6);
            _spriteDefinition = spriteDefinition;
                
            _position = memoryBuilder.AddExtendedPoint();
            _scroller = scroller;
            _specs = specs;
            _spritesModule = spritesModule;
            Motion = motion;
        }

        public Sprite GetSprite() => _spritesModule.GetSprite(SpriteIndex.Value);

        public void UpdateSprite()
        {
            if (Status != WorldSpriteStatus.Active)
                return;

            var sprite = GetSprite();

            int spriteX = (X - _scroller.WorldScrollPixelX).NMod(_specs.NameTablePixelWidth);
            int spriteY = (Y - _scroller.WorldScrollPixelY).NMod(_specs.NameTablePixelHeight);

            sprite.X = (byte)spriteX;
            sprite.Y = (byte)spriteY;

        }

        public BoundsCheck CheckInBounds()
        {
            if (X >= _scroller.WorldScrollPixelX
                && X < _scroller.WorldScrollPixelX + _specs.NameTablePixelWidth
                && Y >= _scroller.WorldScrollPixelY
                && Y < _scroller.WorldScrollPixelY + _specs.NameTablePixelHeight)
            {
                return BoundsCheck.InBounds;
            }

            int threshold = 32;
            if (X >= _scroller.WorldScrollPixelX - threshold
              && X < _scroller.WorldScrollPixelX + _specs.NameTablePixelWidth + threshold
              && Y >= _scroller.WorldScrollPixelY - threshold
              && Y < _scroller.WorldScrollPixelY + _specs.NameTablePixelHeight + threshold)
            {
                return BoundsCheck.OutOfBounds;
            }

            return BoundsCheck.FarOutOfBounds;
        }

        public void ConfigureSprite(Sprite sprite)
        {
            sprite.Tile = _spriteDefinition.Tile;
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
}
