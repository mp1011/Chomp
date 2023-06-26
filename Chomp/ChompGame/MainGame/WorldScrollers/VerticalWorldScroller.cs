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
            int worldScrollBegin = (_focusSprite.Y - _halfWindowSize ).Clamp(0, ScrollYMax);
            int worldScrollBeginTile = worldScrollBegin / _specs.TileHeight;
            byte ntScrollBegin = (worldScrollBegin).NModByte(_specs.NameTablePixelHeight);
            byte ntScrollBeginTile = (byte)(ntScrollBegin / _specs.TileHeight);

            byte ntRow = ntScrollBeginTile;
            byte worldRow = (byte)worldScrollBeginTile;

            var seamTile = (ntScrollBeginTile + _forwardSeamOffset).NModByte(_specs.NameTableHeight);

            while (ntRow != seamTile)
            {
                CopyTileRow(worldRow, ntRow);
                ntRow = (ntRow + 1).NModByte(_specs.NameTableHeight);
                worldRow = (worldRow + 1).NModByte(_levelNameTable.Height);
            }

            seamTile = (ntScrollBeginTile - _backwardSeamOffset).NModByte(_specs.NameTableHeight);
            ntRow = (ntScrollBeginTile - 1).NModByte(_specs.NameTableHeight);
            worldRow = (worldScrollBeginTile - 1).NModByte(_specs.NameTableHeight);

            while (ntRow != seamTile)
            {
                CopyTileRow(worldRow, ntRow);
                ntRow = (ntRow - 1).NModByte(_specs.NameTableHeight);
                worldRow = (worldRow - 1).NModByte(_levelNameTable.Height);
            }
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

            _tileModule.Scroll.Y = ntScrollBegin;
            _spritesModule.Scroll.Y = ntScrollBegin;
            _scrollWindowBegin.Value = ntScrollBegin;

            if (addDifference < subDifference)
            {
                var worldSeamRow = (byte)(worldScrollBeginTile + _forwardSeamOffset).NModByte(_levelNameTable.Height);
                if (worldSeamRow == _seamTile.Value)
                    return false;

                var ntSeamColumn = (ntScrollBeginTile + _forwardSeamOffset).NModByte(_specs.NameTableHeight);
                CopyTileRow(worldSeamRow, ntSeamColumn);
                _seamTile.Value = worldSeamRow;
                return true;
            }
            else
            {
                var worldSeamRow = (byte)(worldScrollBeginTile - _backwardSeamOffset).NModByte(_specs.NameTableHeight);
                if (worldSeamRow == _seamTile.Value)
                    return false;

                var ntSeamColumn = (ntScrollBeginTile - _backwardSeamOffset).NModByte(_specs.NameTableHeight);
                CopyTileRow(worldSeamRow, ntSeamColumn);
                _seamTile.Value = worldSeamRow;
                return true;
            }
        }


        protected void CopyTileRow(byte worldRow, byte ntRow)
        {
            ntRow = (ntRow + 2).NModByte(_tileModule.NameTable.Height);

            for (int col = 0; col < _levelNameTable.Width; col++)
            {
                _tileModule.NameTable[col, ntRow] = _levelNameTable[col, worldRow];
                _tileModule.AttributeTable[col / _specs.AttributeTableBlockSize,
                    ntRow / _specs.AttributeTableBlockSize] = _levelAttributeTable[col / _specs.AttributeTableBlockSize, worldRow / _specs.AttributeTableBlockSize];
            }

            if (ntRow <= Constants.StatusBarTiles)
                _statusBar.InitializeTiles();

            GameDebug.DebugLog($"Seam Update: W{worldRow}->N{ntRow}", DebugLogFlags.WorldScroller);
        }
    }
}
