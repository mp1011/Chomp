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

        public Level6BossScroller(SystemMemoryBuilder memoryBuilder, Specs specs, TileModule tileModule, SpritesModule spritesModule) : base(memoryBuilder, specs, tileModule, spritesModule)
        {
            _lastUpdateX = new GameByte(_seamTile.Address, memoryBuilder.Memory);
            GameDebug.Watch1 = new DebugWatch("PX", () => _focusSprite.X);
            GameDebug.Watch2 = new DebugWatch("SX", () => _tileModule.Scroll.X);
        }

        public override Rectangle ViewPane
        {
            get
            {
                int scrollX = (_focusSprite.X - _halfWindowSize).Clamp(0, 4096);
                int scrollY = (_focusSprite.Y - _halfWindowSize).Clamp(0, 4096);

                return new Rectangle(scrollX, scrollY, _specs.ScreenWidth, _specs.ScreenHeight);
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
            _tileModule.Scroll.Y = (byte)_specs.ScreenHeight;
            _spritesModule.Scroll.Y = (byte)_specs.ScreenHeight;
            byte scrollX = (byte)(_focusSprite.X - _halfWindowSize).Clamp(0, 4096);
        
            _tileModule.Scroll.X = scrollX;
            _spritesModule.Scroll.X = scrollX;

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
