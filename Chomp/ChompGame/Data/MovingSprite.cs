using ChompGame.GameSystem;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChompGame.Data
{
    class MovingSprite
    {
        private byte _motionScale = 4;
        private SpritesModule _spritesModule;
        private GameByte _spriteIndex;
        private ByteVector _motion;
        private GameByte _subPixelX;
        private GameByte _subPixelY;

        public Sprite GetSprite() => _spritesModule.GetSprite(_spriteIndex);

        public Point TopLeft
        {
            get
            {
                var sprite = _spritesModule.GetSprite(_spriteIndex);
                return new Point(sprite.X, sprite.Y);
            }
        }

        public Point BottomRight
        {
            get
            {
                var sprite = _spritesModule.GetSprite(_spriteIndex);
                return new Point(sprite.Right, sprite.Bottom);
            }
        }

        public ByteRectangle Bounds
        {
            get
            {
                var sprite = _spritesModule.GetSprite(_spriteIndex);
                return new ByteRectangle(sprite.X, sprite.Y, sprite.Width, sprite.Height);
            }
        }

        public int X
        {
            get => _spritesModule.GetSprite(_spriteIndex).X;
            set => _spritesModule.GetSprite(_spriteIndex).X = value;
        }

        public int Y
        {
            get => _spritesModule.GetSprite(_spriteIndex).Y;
            set => _spritesModule.GetSprite(_spriteIndex).Y = value;
        }

      


        public byte SpriteIndex
        {
            get => _spriteIndex.Value;
            set => _spriteIndex.Value = value;
        }

        public int XSpeed
        {
            get => _motion.X;
            set => _motion.X = value;
        }

        public int YSpeed
        {
            get => _motion.Y;
            set => _motion.Y = value;
        }

        public MovingSprite(SpritesModule spritesModule, SystemMemoryBuilder memoryBuilder)
        {
            _spritesModule = spritesModule;
            _spriteIndex = memoryBuilder.AddByte();
            _motion = new ByteVector(memoryBuilder.AddByte(), memoryBuilder.AddByte());
            _subPixelX = memoryBuilder.AddByte();
            _subPixelY = memoryBuilder.AddByte();
        }

        public void Update()
        {
            int sx = _subPixelX.Value;
            sx += _motion.X * _motionScale;

            var pixelX = 0;
            if (sx >= 256)
                pixelX = 1;
            else if (sx < 0)
                pixelX = -1;

            _subPixelX.Value = (byte)(sx % 256);
            if (pixelX != 0)
            {
                var sprite = _spritesModule.GetSprite(SpriteIndex);
                sprite.X = (byte)(sprite.X + pixelX);
            }

            int sy = _subPixelY.Value;
            sy += _motion.Y * _motionScale;

            var pixelY = 0;
            if (sy >= 256)
                pixelY = 1;
            else if (sy < 0)
                pixelY = -1;

            _subPixelY.Value = (byte)(sy % 256);
            if (pixelY != 0)
            {
                var sprite = _spritesModule.GetSprite(SpriteIndex);
                sprite.Y = (byte)(sprite.Y + pixelY);
            }

        }
    }
}
