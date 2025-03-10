﻿using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame
{
    // only supports horizontal scrolling
    class SimpleWorldSprite : IWithPosition, IWithBounds
    {
        private readonly SpritesModule _spritesModule;
        private readonly WorldScroller _scroller;
        private readonly Specs _specs;
        private GameByte _spriteIndex;
        private TwoBit _xPosExtra;

        public byte SpriteIndex
        {
            get => _spriteIndex.Value;
            set => _spriteIndex.Value = value;
        }

        public int X
        {
            get => Sprite.X | (_xPosExtra.Value << 7);
            set
            {
                Sprite.X = (byte)(value % 128);
                _xPosExtra.Value = (byte)(value >> 7);
            }
        }

        public int Y
        {
            get => Sprite.Y;
            set => Sprite.Y = (byte)value;
        }

        public Rectangle Bounds => new Rectangle(X, Y, Sprite.Width, Sprite.Height);
        public Rectangle ReducedBounds => new Rectangle(X+1, Y+1, Sprite.Width-2, Sprite.Height-2);

        public Point Center
        {
            get => Bounds.Center;
            set
            {
                var sprite = Sprite;
                X = value.X - sprite.Width / 2;
                Y = value.Y - sprite.Height / 2;
            }
        }

        public bool IsErased => _spriteIndex.Value == 63;

        public SimpleWorldSprite(
            ChompGameModule gameModule,
            SystemMemoryBuilder memoryBuilder)
        {
            _spritesModule = gameModule.SpritesModule;
            _scroller = gameModule.WorldScroller;
            _specs = gameModule.Specs;
            _spriteIndex = memoryBuilder.AddMaskedByte(Bit.Right6);
            _xPosExtra = new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress - 1, 6);
        }

        public SimpleWorldSprite(
            ChompGameModule gameModule,
            int address)
        {
            _spritesModule = gameModule.SpritesModule;
            _scroller = gameModule.WorldScroller;
            _specs = gameModule.Specs;
            _spriteIndex = new MaskedByte(address, Bit.Right6, gameModule.GameSystem.Memory); 
            _xPosExtra = new TwoBit(gameModule.GameSystem.Memory, address, 6);
        }

        public Sprite Sprite => _spritesModule.GetSprite(_spriteIndex.Value);

        public void AssignSpriteIndex()
        {
            _spriteIndex.Value = _spritesModule.GetFreeSpriteIndex();
        }

        public void SetVisibleWhenInBounds()
        {
            var sprite = Sprite;
            sprite.Visible = _scroller.DistanceFromViewpane(Bounds) <= 8;
        }

        public void Erase()
        {
            Sprite.Tile = 0;
            _spriteIndex.Value = 63;            
        }
    }
}
