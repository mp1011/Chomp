using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame
{
    class WorldScroller
    {
        private Specs _specs;
        private GameByte _worldScrollX;
        private GameByte _worldScrollY;
        private TileModule _tileModule;
        private SpritesModule _spritesModule;
        private WorldSprite _focusSprite;
        private NBitPlane _levelNameTable;
        private SceneDefinition _sceneDefinition;

        public WorldScroller(Specs specs, 
            TileModule tileModule, 
            SpritesModule spritesModule, 
            WorldSprite focusSprite, 
            NBitPlane levelNameTable,
            GameByte x,
            GameByte y, 
            SceneDefinition sceneDefinition)
        {
            _specs = specs;
            _tileModule = tileModule;
            _spritesModule = spritesModule;
            _focusSprite = focusSprite;
            _levelNameTable = levelNameTable;
            _sceneDefinition = sceneDefinition;
            _worldScrollX = x;
            _worldScrollY = y;
        }

        public void UpdateVram()
        {
            byte copyWidth = (byte)Math.Min(_specs.NameTableWidth, _levelNameTable.Width);
            byte copyHeight = (byte)Math.Min(_specs.NameTableHeight, _levelNameTable.Height);

            _levelNameTable.CopyTo(
                destination: _tileModule.NameTable,
                source: new InMemoryByteRectangle(_worldScrollX.Value, _worldScrollY.Value, copyWidth, copyHeight),
                destinationPoint: new Point(0, _sceneDefinition.GroundLow),
                specs: _specs,
                memory: _tileModule.GameSystem.Memory);
        }

        public void Update()
        {
            if(_focusSprite.X < _specs.ScreenWidth / 2)
            {
                _tileModule.Scroll.X = 0;
                _spritesModule.Scroll.X = 0;
            }
            else
            {
                byte scroll = (byte)(_focusSprite.X - (_specs.ScreenWidth / 2));
                _tileModule.Scroll.X = scroll;
                _spritesModule.Scroll.X = scroll;
            }
        }
    }
}
