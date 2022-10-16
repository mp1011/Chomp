using ChompGame.GameSystem;

namespace ChompGame.MainGame
{
    class WorldScroller
    {
        private Specs _specs;
        private TileModule _tileModule;
        private SpritesModule _spritesModule;
        private WorldSprite _focusSprite;

        public WorldScroller(Specs specs, TileModule tileModule, SpritesModule spritesModule, WorldSprite focusSprite)
        {
            _specs = specs;
            _tileModule = tileModule;
            _spritesModule = spritesModule;
            _focusSprite = focusSprite;
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
