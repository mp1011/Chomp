using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame
{
    class WorldScroller
    {
        private const byte _scrollPad = 8;

        private Specs _specs;
        private GameByte _worldScrollX;
        private GameByte _worldScrollY;
        private TileModule _tileModule;
        private SpritesModule _spritesModule;

        private MovingWorldSprite _focusSprite;
        private NBitPlane _levelNameTable;
        private NBitPlane _levelAttributeTable;

        private SceneDefinition _sceneDefinition;

        public NBitPlane LevelNameTable => _levelNameTable;
        public NBitPlane LevelAttributeTable => _levelAttributeTable;
         
        public WorldScroller(
            SystemMemoryBuilder memoryBuilder,
            Specs specs, 
            TileModule tileModule, 
            SpritesModule spritesModule)
        {
            _specs = specs;
            _tileModule = tileModule;
            _spritesModule = spritesModule;
            _worldScrollX = memoryBuilder.AddByte();
            _worldScrollY = memoryBuilder.AddByte();
        }

        public int WorldScrollPixelX => _worldScrollX * _specs.TileWidth;
        public int WorldScrollPixelY => _worldScrollY * _specs.TileHeight;

        public int CameraPixelX
        {
            get
            {
                int cameraXMax = (_levelNameTable.Width * _specs.TileWidth) - _specs.ScreenWidth;

                return (_focusSprite.X - (_specs.ScreenWidth / 2))
                    .Clamp(0, cameraXMax);
            }
        }

        public int CameraPixelY
        {
            get
            {
                if (_sceneDefinition.IsLevelBossScene)
                    return _specs.ScreenHeight;

                int cameraYMax = (_levelNameTable.Height * _specs.TileHeight) - _specs.ScreenHeight+8;

                return (_focusSprite.Y - (_specs.ScreenHeight / 2))
                    .Clamp(0, cameraYMax);
            }
        }

        public void Initialize(SceneDefinition scene, MovingWorldSprite focusSprite, NBitPlane levelNameTable, NBitPlane levelAttributeTable)
        {
            _sceneDefinition = scene;
            _focusSprite = focusSprite;
            _levelNameTable = levelNameTable;
            _levelAttributeTable = levelAttributeTable;
            _worldScrollX.Value = 0;
            _worldScrollY.Value = 0;

            _tileModule.Scroll.X = 0;
            _tileModule.Scroll.Y = 0;
            _spritesModule.Scroll.X = 0;
            _spritesModule.Scroll.Y = 0;
        }

        public void UpdateVram()
        {
            byte copyWidth = (byte)Math.Min(_specs.NameTableWidth / _specs.AttributeTableBlockSize, _levelAttributeTable.Width);
            byte copyHeight = (byte)Math.Min(_specs.NameTableHeight / _specs.AttributeTableBlockSize, _levelAttributeTable.Height);

          //  System.Diagnostics.Debug.WriteLine($"Scroll WX={_worldScrollX.Value}, WXA={_worldScrollX.Value / _specs.AttributeTableBlockSize}");
            _levelAttributeTable.CopyTo(
                destination: _tileModule.AttributeTable,
                source: new InMemoryByteRectangle(
                    _worldScrollX.Value / _specs.AttributeTableBlockSize,
                    _worldScrollY.Value / _specs.AttributeTableBlockSize,
                    copyWidth,
                    copyHeight),
                destinationPoint: new Point(0, 1),
                specs: _specs,
                memory: _tileModule.GameSystem.Memory);

            switch(_sceneDefinition.ScrollStyle)
            {
                case ScrollStyle.None:
                    UpdateVram_NoScroll();
                    break;
                case ScrollStyle.Horizontal:
                    UpdateVram_Horizontal();
                    break;
                case ScrollStyle.Vertical:
                    UpdateVram_Vertical();
                    break;
                case ScrollStyle.NameTable:
                    UpdateVram_XY();
                    break;
            }
        }

        private void UpdateVram_NoScroll()
        {
            var copyWidth = (byte)_levelNameTable.Width;
            var copyHeight = (byte)_levelNameTable.Height;

            _levelNameTable.CopyTo(
               destination: _tileModule.NameTable,
               source: new InMemoryByteRectangle((byte)0, (byte)0, copyWidth, copyHeight),
               destinationPoint: new Point(0, 2),
               specs: _specs,
               memory: _tileModule.GameSystem.Memory);
        }

        private void UpdateVram_Vertical()
        {
            var copyWidth = _levelNameTable.Width;
            byte copyHeight = (byte)Math.Min(_specs.NameTableHeight - Constants.StatusBarTiles, _levelNameTable.Height);

            _levelNameTable.CopyTo(
                 destination: _tileModule.NameTable,
                 source: new InMemoryByteRectangle(0, _worldScrollY.Value, copyWidth, copyHeight),
                 destinationPoint: new Point(0, Constants.StatusBarTiles),
                 specs: _specs,
                 memory: _tileModule.GameSystem.Memory);

        }

        private void UpdateVram_XY()
        {
            UpdateVram_NoScroll();
        }

        private void UpdateVram_Horizontal()
        {
            byte copyWidth = (byte)Math.Min(_specs.NameTableWidth, _levelNameTable.Width);

            //top section
            _levelNameTable.CopyTo(
                destination: _tileModule.NameTable,
                source: new InMemoryByteRectangle(_worldScrollX.Value, 0, copyWidth, _sceneDefinition.GetBackgroundLayerTile(BackgroundLayer.Back1, includeStatusBar: false)),
                destinationPoint: new Point(0, 2),
                specs: _specs,
                memory: _tileModule.GameSystem.Memory);

            int bottomSectionBegin = _sceneDefinition.GetBackgroundLayerTile(BackgroundLayer.ForegroundStart, includeStatusBar: false);
            int bottomSectionHeight = _levelNameTable.Height - bottomSectionBegin;

            //bottom section
            _levelNameTable.CopyTo(
                destination: _tileModule.NameTable,
                source: new InMemoryByteRectangle(_worldScrollX.Value, bottomSectionBegin, copyWidth, bottomSectionHeight),
                destinationPoint: new Point(0, bottomSectionBegin + 2),
                specs: _specs,
                memory: _tileModule.GameSystem.Memory);
        }

        private int AdjustWorldScrollX()
        {
            int newWorldScroll = (CameraPixelX - (_specs.NameTablePixelWidth - _specs.ScreenWidth) / 2)
                .Clamp(0, WorldScrollMaxX * _specs.TileWidth);

            newWorldScroll = newWorldScroll / _specs.TileWidth;
            newWorldScroll = (newWorldScroll / _specs.AttributeTableBlockSize) * _specs.AttributeTableBlockSize;
            
            _worldScrollX.Value = (byte)newWorldScroll;
            UpdateVram();

            return CameraPixelX - WorldScrollPixelX;
        }

        private int AdjustWorldScrollY()
        {
            int newWorldScroll = (CameraPixelY - (_specs.NameTablePixelHeight - _specs.ScreenHeight) / 2)
                .Clamp(0, WorldScrollMaxY * _specs.TileHeight);

            _worldScrollY.Value = (byte)(newWorldScroll / _specs.TileHeight);
            UpdateVram();

            return CameraPixelY - newWorldScroll;
        }

        private int WorldScrollMaxX => _levelNameTable.Width - _specs.NameTableWidth;
        private int WorldScrollMaxY => _levelNameTable.Height - _specs.NameTableHeight;

        public bool Update()
        {
            switch(_sceneDefinition.ScrollStyle)
            {
                case ScrollStyle.Vertical: 
                    return Update_Vertical();
                case ScrollStyle.Horizontal:
                    return Update_Horizontal();
                case ScrollStyle.NameTable:
                    return Update_XY();
                default:
                    return false;
            }
        }

        private bool Update_Horizontal()
        {

            bool changed = false;
            int scrollX = CameraPixelX - WorldScrollPixelX;

            if (scrollX < _scrollPad && _worldScrollX.Value > 0)
            {
                scrollX = AdjustWorldScrollX();
                changed = true;
            }
            else if (scrollX > _specs.NameTablePixelWidth - _specs.ScreenWidth - _scrollPad
                && _worldScrollX.Value < WorldScrollMaxX)
            {
                scrollX = AdjustWorldScrollX();
                changed = true;
            }

            _tileModule.Scroll.X = (byte)scrollX;
            _spritesModule.Scroll.X = (byte)scrollX;

            return changed;
        }


        private bool Update_Vertical()
        {

            bool changed = false;
            int scrollY = CameraPixelY - WorldScrollPixelY;

            if (scrollY < _scrollPad && _worldScrollY.Value > 0)
            {
                scrollY = AdjustWorldScrollY();
                changed = true;
            }
            else if (scrollY > _specs.NameTablePixelHeight - _specs.ScreenHeight - _scrollPad
                && _worldScrollY.Value < WorldScrollMaxY)
            {
                scrollY = AdjustWorldScrollY();
                changed = true;
            }

            _tileModule.Scroll.Y = (byte)scrollY;
            _spritesModule.Scroll.Y = (byte)scrollY;

            return changed;
        }

        private bool Update_XY()
        {
            int scrollX = CameraPixelX - WorldScrollPixelX;
            int scrollY = CameraPixelY - WorldScrollPixelY;

            _tileModule.Scroll.X = (byte)scrollX;
            _spritesModule.Scroll.X = (byte)scrollX;

            _tileModule.Scroll.Y = (byte)scrollY;
            _spritesModule.Scroll.Y = (byte)scrollY;

            return false;
        }

        public void ModifyTiles(Action<NBitPlane, NBitPlane> modify)
        {
            modify(_levelNameTable, _levelAttributeTable);
            UpdateVram();
        }

    }
}
