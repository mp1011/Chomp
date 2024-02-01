using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class ChompTail
    {
        private readonly int _numSections;
        private readonly ChompGameModule _gameModule;
        private readonly SpritesModule _spritesModule;
        private readonly SpriteTileTable _spriteTileTable;
        private readonly int _tailSpritesAddress;

        public ChompTail(SystemMemoryBuilder memoryBuilder, 
            int numSections, 
            ChompGameModule gameModule)
        {
            _gameModule = gameModule;
            _numSections = numSections;
            _spritesModule = gameModule.SpritesModule;
            _spriteTileTable = gameModule.SpriteTileTable;

            _tailSpritesAddress = memoryBuilder.CurrentAddress;
            memoryBuilder.AddBytes(numSections);
        }

        public SimpleWorldSprite GetWorldSprite(int index) => new SimpleWorldSprite(_gameModule, _tailSpritesAddress + index);
        public Sprite GetSprite(int index) => GetWorldSprite(index).Sprite;

        public void Erase(int index) => GetWorldSprite(index).Erase();

        public bool IsErased(int index) => GetWorldSprite(index).IsErased;

        public void CreateTail(SpriteTileIndex tileIndex = SpriteTileIndex.Extra2)
        {
            for (int i = 0; i < _numSections; i++)
            {
                var tailSprite = GetWorldSprite(i);
                tailSprite.AssignSpriteIndex();

                var sprite = tailSprite.Sprite;
                sprite.Tile = (byte)(_spriteTileTable.GetTile(tileIndex));
                sprite.SizeX = 1;
                sprite.SizeY = 1;
                sprite.Palette = 2;
                sprite.Visible = true;
                sprite.X = 0;
                sprite.Y = 0;
            }
        }

    }
}
