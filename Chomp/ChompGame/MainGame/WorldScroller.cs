using ChompGame.Data;
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

        private WorldSprite _focusSprite;
        private NBitPlane _levelNameTable;
        private NBitPlane _levelAttributeTable;

        private SceneDefinition _sceneDefinition;

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
        public void Initialize(SceneDefinition scene, WorldSprite focusSprite, NBitPlane levelNameTable, NBitPlane levelAttributeTable)
        {
            _sceneDefinition = scene;
            _focusSprite = focusSprite;
            _levelNameTable = levelNameTable;
            _levelAttributeTable = levelAttributeTable;
        }

        public void UpdateVram()
        {
            byte copyWidth = (byte)Math.Min(_specs.NameTableWidth / _specs.AttributeTableBlockSize, _levelAttributeTable.Width);
            byte copyHeight = (byte)Math.Min(_specs.NameTableHeight / _specs.AttributeTableBlockSize, _levelAttributeTable.Height);

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

            copyWidth = (byte)Math.Min(_specs.NameTableWidth, _levelNameTable.Width);
          
            //top section
            _levelNameTable.CopyTo(
                destination: _tileModule.NameTable,
                source: new InMemoryByteRectangle(_worldScrollX.Value, 0, copyWidth, _sceneDefinition.ParallaxLayerABeginTile),
                destinationPoint: new Point(0, 2),
                specs: _specs,
                memory: _tileModule.GameSystem.Memory);

            int bottomSectionBegin = 2 + (_sceneDefinition.ParallaxLayerABeginTile + (_sceneDefinition.ParallaxLayerATiles * 2) + _sceneDefinition.ParallaxLayerBTiles);

            int bottomSectionHeight = _levelNameTable.Height - bottomSectionBegin;

            //bottom section
            _levelNameTable.CopyTo(
                destination: _tileModule.NameTable,
                source: new InMemoryByteRectangle(_worldScrollX.Value, bottomSectionBegin, copyWidth, bottomSectionHeight),
                destinationPoint: new Point(0, bottomSectionBegin  + 2),
                specs: _specs,
                memory: _tileModule.GameSystem.Memory);


        }

        private int AdjustWorldScroll()
        {
            int newWorldScroll = (CameraPixelX - (_specs.NameTablePixelWidth - _specs.ScreenWidth) / 2)
                .Clamp(0, WorldScrollMaxX * _specs.TileWidth);

            _worldScrollX.Value = (byte)(newWorldScroll / _specs.TileWidth);
            UpdateVram();

            return CameraPixelX - newWorldScroll;
        }

        private int WorldScrollMaxX => _levelNameTable.Width - _specs.NameTableWidth;

        public bool Update()
        {
            bool changed = false;
            int scrollX = CameraPixelX - WorldScrollPixelX;

            if (scrollX < _scrollPad && _worldScrollX.Value > 0)
            {
                scrollX = AdjustWorldScroll();
                changed = true;
            }
            else if (scrollX > _specs.NameTablePixelWidth - _specs.ScreenWidth - _scrollPad
                && _worldScrollX.Value < WorldScrollMaxX)
            {
                scrollX = AdjustWorldScroll();
                changed = true;
            }

            _tileModule.Scroll.X = (byte)scrollX;
            _spritesModule.Scroll.X = (byte)scrollX;

            return changed;
        }

    }
}
