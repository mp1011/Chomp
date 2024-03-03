using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.WorldScrollers
{
    class VerticalWorldScroller : WorldScroller
    {
        private readonly StatusBar _statusBar;

        public VerticalWorldScroller(SystemMemoryBuilder memoryBuilder, Specs specs, TileModule tileModule, SpritesModule spritesModule, StatusBar statusBar) 
            : base(memoryBuilder, specs, tileModule, spritesModule)
        {
            _statusBar = statusBar;
        }

        private byte ScrollYMax => (byte)((_levelNameTable.Height * _specs.TileHeight) - _specs.ScreenHeight);

        public override Rectangle ViewPane
        {
            get
            {
                int scrollX = 0;
                int scrollY = (_focusSprite.Y - _halfWindowSize).Clamp(0, ScrollYMax);

                return new Rectangle(scrollX, scrollY, _specs.ScreenWidth, _specs.ScreenHeight);
            }
        }

        public override int DistanceFromViewpane(Rectangle r)
        {
            if (r.Intersects(ViewPane))
                return 0;

            var downDistance = ViewPane.Top - r.Bottom;
            var upDistance = r.Top - ViewPane.Bottom;

            if (downDistance > 0)
                return downDistance;
            else if (upDistance > 0)
                return upDistance;
            else
                return 0;
        }

        public override void RefreshNametable()
        {
            _tileModule.Scroll.X = (byte)_specs.ScreenWidth;

            int worldScrollBegin = (_focusSprite.Y - _halfWindowSize ).Clamp(0, ScrollYMax);
            int worldScrollBeginTile = worldScrollBegin / _specs.TileHeight;
            byte ntScrollBegin = (worldScrollBegin).NModByte(_specs.NameTablePixelHeight);
            byte ntScrollBeginTile = (byte)(ntScrollBegin / _specs.TileHeight);

            byte ntRow = ntScrollBeginTile;
            byte worldRow = (byte)worldScrollBeginTile;

            var seamTile = (ntScrollBeginTile + _forwardSeamOffset).NModByte(_specs.NameTableHeight);
            GameDebug.DebugLog($"Seam Refresh Down to {seamTile}", DebugLogFlags.WorldScroller);

            while (ntRow != seamTile)
            {
                CopyTileRow(worldRow, ntRow);
                ntRow = (ntRow + 1).NModByte(_specs.NameTableHeight);
                worldRow = (worldRow + 1).NModByte(_levelNameTable.Height);
            }
            CopyTileRow(worldRow, ntRow);

            seamTile = (ntScrollBeginTile - _backwardSeamOffset - 2).NModByte(_specs.NameTableHeight);
            ntRow = (ntScrollBeginTile - 1).NModByte(_specs.NameTableHeight);
            worldRow = (worldScrollBeginTile - 1).NModByte(_levelNameTable.Height);
            GameDebug.DebugLog($"Seam Refresh Up to {seamTile}", DebugLogFlags.WorldScroller);

            while (ntRow != seamTile)
            {
                CopyTileRow(worldRow, ntRow);
                ntRow = (ntRow - 1).NModByte(_specs.NameTableHeight);
                worldRow = (worldRow - 1).NModByte(_levelNameTable.Height);
            }
            CopyTileRow(worldRow, ntRow);
        }

        public override bool Update()
        {

            int worldScrollBegin = (_focusSprite.Y - _halfWindowSize).Clamp(0, ScrollYMax);
            int worldScrollBeginTile = worldScrollBegin / _specs.TileHeight;
            var ntScrollBegin = (worldScrollBegin).NModByte(_specs.NameTablePixelHeight);
            var ntScrollBeginTile = ntScrollBegin / _specs.TileHeight;

            if (ntScrollBegin == _scrollWindowBegin.Value)
                return false;

            var addDifference = (ntScrollBegin - _scrollWindowBegin.Value).NModByte(_specs.NameTablePixelHeight);
            var subDifference = (_scrollWindowBegin.Value - ntScrollBegin).NModByte(_specs.NameTablePixelHeight);

            _tileModule.Scroll.X = (byte)_specs.ScreenWidth;            
            _tileModule.Scroll.Y = ntScrollBegin;
            _spritesModule.Scroll.Y = ntScrollBegin;
            _scrollWindowBegin.Value = ntScrollBegin;

            if (addDifference < subDifference)
            {
                var worldSeamRow = (byte)(worldScrollBeginTile + _forwardSeamOffset).NModByte(_levelNameTable.Height);
                if (worldSeamRow == _seamTile.Value)
                    return false;

                var ntSeamRow = (ntScrollBeginTile + _forwardSeamOffset).NModByte(_specs.NameTableHeight);
                CopyTileRow(worldSeamRow, ntSeamRow);
                _seamTile.Value = worldSeamRow;
                return true;
            }
            else
            {
                var worldSeamRow = (byte)(worldScrollBeginTile - _backwardSeamOffset).NModByte(_levelNameTable.Height);
                if (worldSeamRow == _seamTile.Value)
                    return false;

                var ntSeamRow = (ntScrollBeginTile - _backwardSeamOffset).NModByte(_specs.NameTableHeight);
                CopyTileRow(worldSeamRow, ntSeamRow);
                _seamTile.Value = worldSeamRow;
                return true;
            }
        }


        protected void CopyTileRow(byte worldRow, byte ntRow)
        {
            ntRow = (ntRow + 2).NModByte(_tileModule.NameTable.Height);
            for (int col = 0; col < _levelNameTable.Width; col++)
            {
                _tileModule.NameTable[col + _tilesPerScreen, ntRow] = _levelNameTable[col, worldRow];

                var attrX = (col + _tilesPerScreen) / _specs.AttributeTableBlockSize;
                var attrY = ntRow / _specs.AttributeTableBlockSize;
                _tileModule.AttributeTable[attrX, attrY] = _levelAttributeTable[col / _specs.AttributeTableBlockSize, worldRow / _specs.AttributeTableBlockSize];
            }

            GameDebug.DebugLog($"Seam Update: W{worldRow}->N{ntRow}", DebugLogFlags.WorldScroller);
        }
    }
}
