using ChompGame.Data;
using ChompGame.GameSystem;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame
{
    class WorldSprite
    {
        private readonly Specs _specs;
        private readonly SpritesModule _spritesModule;

        public GameByte SpriteIndex { get; }
        public NibblePoint WorldBlock { get; }
        public PrecisionMotion Motion { get; }

        public bool FlipX
        {
            get => GetSprite().FlipX;
            set => GetSprite().FlipX = value;
        }

        public int X
        {
            get
            {
                var sprite = GetSprite();
                return sprite.X + WorldBlock.X * _specs.NameTablePixelWidth;
            }
            set
            {
                var sprite = GetSprite();

                WorldBlock.X = (byte)(value / _specs.NameTablePixelWidth);
                sprite.X = (byte)(value % _specs.NameTablePixelWidth);
            }
        }
        public int Y
        {
            get
            {
                var sprite = GetSprite();
                return sprite.Y + WorldBlock.Y * _specs.NameTablePixelHeight;
            }
            set
            {
                var sprite = GetSprite();

                WorldBlock.Y = (byte)(value / _specs.NameTablePixelHeight);
                sprite.Y = (byte)(value % _specs.NameTablePixelHeight);
            }
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
            NibblePoint worldBlock, 
            PrecisionMotion motion)
        {
            _specs = specs;
            _spritesModule = spritesModule;
            SpriteIndex = spriteIndex;
            WorldBlock = worldBlock;
            Motion = motion;
        }

        public Sprite GetSprite() => _spritesModule.GetSprite(SpriteIndex.Value);

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
