using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame.WorldScrollers
{
    class Level6BossScroller : WorldScroller
    {
        private GameByte _lastUpdateX;
        private GameBit _forceScrollOn;
        private MaskedByte _scrollLock;
        private GameByte _levelTimer;
        private GameByte _scrollOffset;
        private GameBit _scrollExtra;
        private ExtendedByte _fullScroll;

        public Level6BossScroller(SystemMemoryBuilder memoryBuilder, Specs specs, TileModule tileModule, SpritesModule spritesModule, GameByte levelTimer) : base(memoryBuilder, specs, tileModule, spritesModule)
        {
            _levelTimer  = levelTimer;
            _lastUpdateX = new GameByte(_seamTile.Address, memoryBuilder.Memory);
            _scrollOffset = memoryBuilder.AddByte();
            GameDebug.Watch1 = new DebugWatch("PX", () => _focusSprite.X);
            GameDebug.Watch2 = new DebugWatch("SX", () => _tileModule.Scroll.X);
        }

        public override Rectangle ViewPane
        {
            get
            {
                if (_forceScrollOn.Value)
                {
                    int scrollX = _fullScroll.Value;
                    int scrollY = (_focusSprite.Y - _halfWindowSize).Clamp(0, 4096);

                    return new Rectangle(scrollX, scrollY, _specs.ScreenWidth, _specs.ScreenHeight);
                }
                else
                {
                    int scrollX = (_focusSprite.X - _halfWindowSize).Clamp(0, 4096);
                    int scrollY = (_focusSprite.Y - _halfWindowSize).Clamp(0, 4096);

                    return new Rectangle(scrollX, scrollY, _specs.ScreenWidth, _specs.ScreenHeight);
                }
            }
        }

        public override void RefreshNametable()
        {
            for (byte c = 0; c < _specs.NameTableWidth; c++)
                CopyTileColumn(c, c);
        }

        public override void OffsetCamera(int x, int y)
        {
            _tileModule.Scroll.X = (byte)x;
            _tileModule.Scroll.Y = (byte)y;
            _spritesModule.Scroll.X = (byte)x;
            _spritesModule.Scroll.Y = (byte)y;
        }

        public override bool Update()
        {
            if(_forceScrollOn == null)
            {
                _forceScrollOn = new GameBit(Extra.Address, Bit.Bit7, _spritesModule.GameSystem.Memory);
                _scrollLock = new MaskedByte(Extra.Address, Bit.Right6, _spritesModule.GameSystem.Memory);
                _scrollExtra = new GameBit(Extra.Address, Bit.Bit6, _spritesModule.GameSystem.Memory);
                _fullScroll = new ExtendedByte(new GameByte(_tileModule.Scroll.Address, _spritesModule.GameSystem.Memory), _scrollExtra);
            }

            _tileModule.Scroll.Y = (byte)_specs.ScreenHeight;
            _spritesModule.Scroll.Y = (byte)_specs.ScreenHeight;
            int scrollX;

            if(_forceScrollOn.Value)
            {
                if (_levelTimer.IsMod(4))
                    _fullScroll.Value++;

                scrollX = _fullScroll.Value;

                if (_focusSprite.X > ViewPane.Right-4)
                    _focusSprite.X = ViewPane.Right-4;
            }
            else
            {
                _fullScroll.Value = 0;
                scrollX = (_focusSprite.X - _halfWindowSize).Clamp(0, 4096);
            }

            if (_scrollLock.Value > 0)
            {
                var maxScroll = 8 * _scrollLock.Value;
                if (scrollX > maxScroll)
                    scrollX =maxScroll;

                var maxX = (maxScroll + _specs.ScreenWidth) -4;
                if (_focusSprite.X > maxX)
                    _focusSprite.X = maxX;
            }
        
            _tileModule.Scroll.X = (byte)scrollX;
            _spritesModule.Scroll.X = (byte)scrollX;

            int xDiff = Math.Abs(scrollX / _specs.TileWidth - _lastUpdateX);
           
            if (xDiff > 8)
            {
                _lastUpdateX.Value = (byte)(scrollX / _specs.TileWidth);
                return true;
            }

            return false;
        }
    }
}
