using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame
{
    class WorldSprite
    {
        private readonly Specs _specs;
        private readonly SpritesModule _spritesModule;
        private readonly WorldScroller _scroller;

        public GameByte SpriteIndex { get; }

        public PrecisionMotion Motion { get; }

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
            Specs specs,
            SpritesModule spritesModule, 
            GameByte spriteIndex, 
            ExtendedPoint position,
            PrecisionMotion motion, 
            WorldScroller scroller)
        {
            _position = position;
            _scroller = scroller;
            _specs = specs;
            _spritesModule = spritesModule;
            SpriteIndex = spriteIndex;
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

        public bool IsInBounds()
        {
            //todo need to fix this
            return true;

            //return X >= _spritesModule.Scroll.X
            //    && X < _spritesModule.Scroll.X + _specs.NameTablePixelWidth
            //    && Y >= _spritesModule.Scroll.Y
            //    && Y < _spritesModule.Scroll.Y + _specs.NameTablePixelHeight;
        }
    }
}
