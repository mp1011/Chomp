using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame.WorldScrollers
{
    class AutoscrollWorldScroller : WorldScroller
    {
        public AutoscrollWorldScroller(SystemMemoryBuilder memoryBuilder, Specs specs, TileModule tileModule, SpritesModule spritesModule) 
            : base(memoryBuilder, specs, tileModule, spritesModule)
        {
        }

        private byte ScrollXMax => (byte)((_levelNameTable.Width * _specs.TileWidth) - _specs.ScreenWidth);
        private byte ScrollYMax => (byte)((_levelNameTable.Height * _specs.TileHeight) - _specs.ScreenHeight);

        public override Rectangle ViewPane
        {
            get
            {
                int scrollX = (_focusSprite.X - _halfWindowSize).Clamp(0, ScrollXMax);
                int scrollY = (_focusSprite.Y - _halfWindowSize).Clamp(0, ScrollYMax);

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
            byte scrollX = (byte)(_focusSprite.X - _halfWindowSize).Clamp(0, ScrollXMax);
            byte scrollY = (byte)(_focusSprite.Y - _halfWindowSize).Clamp(0, ScrollYMax);

            _tileModule.Scroll.X = (byte)(scrollX + x);
            _spritesModule.Scroll.X = (byte)(scrollX + x);

            _tileModule.Scroll.Y = (byte)(scrollY + y);
            _spritesModule.Scroll.Y = (byte)(scrollY + y);            
        }

        public override bool Update() => false;
    }
}
