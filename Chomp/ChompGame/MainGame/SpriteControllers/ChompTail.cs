using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.MainGame.SpriteModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChompGame.MainGame.SpriteControllers
{
    class ChompTail
    {
        private readonly int _numSections;
        private readonly SpritesModule _spritesModule;
        private readonly SpriteTileTable _spriteTileTable;
        private readonly GameByteArray _tailSprites;

        public ChompTail(SystemMemoryBuilder memoryBuilder, 
            int numSections, 
            SpritesModule spritesModule,
            SpriteTileTable spriteTileTable)
        {
            _numSections = numSections;
            _spritesModule = spritesModule;
            _spriteTileTable = spriteTileTable;

            _tailSprites = new GameByteArray(memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            memoryBuilder.AddBytes(numSections);
        }

        public Sprite GetSprite(int index) => _spritesModule.GetSprite(_tailSprites[index]);

        public void CreateTail()
        {
            for (int i = 0; i < _numSections; i++)
            {
                var spriteIndex = _spritesModule.GetFreeSpriteIndex();
               _tailSprites[i] = spriteIndex;

                var sprite = _spritesModule.GetSprite(spriteIndex);
                sprite.Tile = (byte)(_spriteTileTable.GetTile(SpriteTileIndex.Extra2));
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
