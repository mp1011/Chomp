using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.WorldScrollers
{
    class HorizontalWorldScroller : WorldScroller
    {
        private byte ScrollXMax => (byte)((_levelNameTable.Width * _specs.TileWidth) - _specs.ScreenWidth);

        public override Rectangle ViewPane
        {
            get
            {
                int scrollX = (_focusSprite.X - _halfWindowSize).Clamp(0, ScrollXMax);
                int scrollY = 0;

                return new Rectangle(scrollX, scrollY, _specs.ScreenWidth, _specs.ScreenHeight);
            }
        }

        protected override byte ScrollableSectionBegin => (byte)_sceneDefinition.GetBackgroundLayerTile(BackgroundLayer.ForegroundStart, includeStatusBar: false);

        public override int DistanceFromViewpane(Rectangle r)
        {
            if (r.Intersects(ViewPane))
                return 0;

            var rightDistance = ViewPane.X - r.Right;
            var leftDistance = r.Left - ViewPane.Right;

            if (rightDistance > 0)
                return rightDistance;
            else if (leftDistance > 0)
                return leftDistance;
            else
                return 0;
        }

        public HorizontalWorldScroller(SystemMemoryBuilder memoryBuilder, Specs specs, TileModule tileModule, SpritesModule spritesModule) 
            : base(memoryBuilder, specs, tileModule, spritesModule)
        {
        }

        public override void RefreshNametable()
        {
            int worldScrollBegin = (_focusSprite.X - _halfWindowSize).Clamp(0, ScrollXMax);
            int worldScrollBeginTile = worldScrollBegin / _specs.TileWidth;
            byte ntScrollBegin = worldScrollBegin.NModByte(_specs.NameTablePixelWidth);
            byte ntScrollBeginTile = (byte)(ntScrollBegin / _specs.TileWidth);

            byte ntCol = ntScrollBeginTile;
            byte worldCol = (byte)worldScrollBeginTile;

            var seamTile = (ntScrollBeginTile + _forwardSeamOffset).NModByte(_specs.NameTableWidth);

            while (ntCol != seamTile)
            {
                CopyTileColumn(worldCol, ntCol);
                ntCol = (ntCol + 1).NModByte(_specs.NameTableWidth);
                worldCol = (worldCol + 1).NModByte(_levelNameTable.Width);
            }

            seamTile = (ntScrollBeginTile - _backwardSeamOffset).NModByte(_specs.NameTableWidth);
            ntCol = (ntScrollBeginTile - 1).NModByte(_specs.NameTableWidth);
            worldCol = (worldScrollBeginTile - 1).NModByte(_specs.NameTableWidth);

            while (ntCol != seamTile)
            {
                CopyTileColumn(worldCol, ntCol);
                ntCol = (ntCol - 1).NModByte(_specs.NameTableWidth);
                worldCol = (worldCol - 1).NModByte(_levelNameTable.Width);
            }
        }

        public override bool Update()
        {
            int worldScrollBegin = (_focusSprite.X - _halfWindowSize).Clamp(0, ScrollXMax);
            int worldScrollBeginTile = worldScrollBegin / _specs.TileWidth;
            var ntScrollBegin = worldScrollBegin.NModByte(_specs.NameTablePixelWidth);
            var ntScrollBeginTile = ntScrollBegin / _specs.TileWidth;

            if (ntScrollBegin == _scrollWindowBegin.Value)
                return false;

            var addDifference = (ntScrollBegin - _scrollWindowBegin.Value).NModByte(_specs.NameTablePixelWidth);
            var subDifference = (_scrollWindowBegin.Value - ntScrollBegin).NModByte(_specs.NameTablePixelWidth);

            _tileModule.Scroll.X = ntScrollBegin;
            _spritesModule.Scroll.X = ntScrollBegin;
            _scrollWindowBegin.Value = ntScrollBegin;

            if (addDifference < subDifference)
            {
                var worldSeamColumn = (worldScrollBeginTile + _forwardSeamOffset).NModByte(_levelNameTable.Width);
                if (worldSeamColumn == _seamTile.Value)
                    return false;

                var ntSeamColumn = (ntScrollBeginTile + _forwardSeamOffset).NModByte(_specs.NameTableWidth);
                CopyTileColumn(worldSeamColumn, ntSeamColumn);
                _seamTile.Value = worldSeamColumn;
                return true;
            }
            else
            {
                var worldSeamColumn = (byte)(worldScrollBeginTile - _backwardSeamOffset).NModByte(_specs.NameTableWidth);
                if (worldSeamColumn == _seamTile.Value)
                    return false;

                var ntSeamColumn = (ntScrollBeginTile - _backwardSeamOffset).NModByte(_specs.NameTableWidth);
                CopyTileColumn(worldSeamColumn, ntSeamColumn);
                _seamTile.Value = worldSeamColumn;
                return true;
            }
        }

       
    }
}
