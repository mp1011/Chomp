using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame.WorldScrollers
{
    class NametableScroller : WorldScroller
    {
        private GameByte _lastUpdateX, _lastUpdateY;
        private bool _isLevelBoss = false;

        public NametableScroller(SystemMemoryBuilder memoryBuilder, Specs specs, TileModule tileModule, SpritesModule spritesModule, bool isLevelBoss) 
            : base(memoryBuilder, specs, tileModule, spritesModule)
        {
            _lastUpdateX = new GameByte(_seamTile.Address, memoryBuilder.Memory);
            _lastUpdateY = new GameByte(_scrollWindowBegin.Address, memoryBuilder.Memory);
            _isLevelBoss = isLevelBoss;
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

        public override bool Update()
        {
            byte scrollX = (byte)(_focusSprite.X - _halfWindowSize).Clamp(0, ScrollXMax);
            byte scrollY = 0;

            _tileModule.Scroll.X = scrollX;
            _spritesModule.Scroll.X = scrollX;

            if (_isLevelBoss)
            {
                _tileModule.Scroll.Y = (byte)_specs.ScreenHeight;
                _spritesModule.Scroll.Y = (byte)_specs.ScreenHeight;
            }
            else
            {
                scrollY = (byte)(_focusSprite.Y - _halfWindowSize).Clamp(0, ScrollYMax);
                _tileModule.Scroll.Y = scrollY;
                _spritesModule.Scroll.Y = scrollY;
            }

            int xDiff = Math.Abs(scrollX / _specs.TileWidth - _lastUpdateX);
            int yDiff = _isLevelBoss ? 0 : Math.Abs(scrollY / _specs.TileHeight - _lastUpdateY);

            if(xDiff > 8 || yDiff > 0)
            {
                _lastUpdateX.Value = (byte)(scrollX / _specs.TileWidth);
                _lastUpdateY.Value = (byte)(scrollY / _specs.TileHeight);
                return true;
            }

            return false;
        }
    }
}
