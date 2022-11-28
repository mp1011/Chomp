using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.GameSystem;
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
            SpritesModule spritesModule,
            PrecisionMotion motion, 
            WorldScroller scroller)
        {
            SpriteIndex = memoryBuilder.AddMaskedByte(Bit.Right5);
            _status = new TwoBitEnum<WorldSpriteStatus>(memoryBuilder.Memory, SpriteIndex.Address, 6);
                
            _position = memoryBuilder.AddExtendedPoint();
            _scroller = scroller;
            _specs = specs;
            _spritesModule = spritesModule;
            Motion = motion;
        }

        public Sprite GetSprite() => _spritesModule.GetSprite(SpriteIndex.Value);

        public void UpdateSpritePosition()
        {
            var sprite = GetSprite();

            int spriteX = (X - _scroller.WorldScrollPixelX).NMod(_specs.NameTablePixelWidth);
            int spriteY = (Y - _scroller.WorldScrollPixelY).NMod(_specs.NameTablePixelHeight);

            sprite.X = (byte)spriteX;
            sprite.Y = (byte)spriteY;

        }

        public BoundsCheck CheckInBounds()
        {
            if (X >= _spritesModule.Scroll.X
                && X < _spritesModule.Scroll.X + _specs.NameTablePixelWidth
                && Y >= _spritesModule.Scroll.Y
                && Y < _spritesModule.Scroll.Y + _specs.NameTablePixelHeight)
            {
                return BoundsCheck.InBounds;
            }

            int threshold = 32;
            if (X >= _spritesModule.Scroll.X - threshold
              && X < _spritesModule.Scroll.X + _specs.NameTablePixelWidth + threshold
              && Y >= _spritesModule.Scroll.Y - threshold
              && Y < _spritesModule.Scroll.Y + _specs.NameTablePixelHeight + threshold)
            {
                return BoundsCheck.OutOfBounds;
            }

            return BoundsCheck.FarOutOfBounds;
        }
    }
}
