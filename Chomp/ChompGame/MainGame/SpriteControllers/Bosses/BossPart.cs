using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers.Bosses
{
    class BossPart
    {
        private readonly SpritesModule _spritesModule;
        private readonly SpriteTileTable _spriteTileTable;
        private GameByte _spriteIndex;
        
        private SpriteDefinition _spriteDefinition;
        private GameByte _xPosition, _yPosition;

        public Sprite Sprite => _spritesModule.GetSprite(_spriteIndex);

        public BossPart(
            SystemMemoryBuilder memoryBuilder,
            SpritesModule spritesModule, 
            SpriteTileTable spriteTileTable,
            SpriteDefinition spriteDefinition)
        {
            _spriteIndex = memoryBuilder.AddByte();
            _xPosition = memoryBuilder.AddByte(128);
            _yPosition = memoryBuilder.AddByte(128);

            _spritesModule = spritesModule;
            _spriteTileTable = spriteTileTable;
            _spriteDefinition = spriteDefinition;
        }

        public Sprite PrepareSprite(SpriteTileIndex tileIndex)
        {
            _spriteIndex.Value = _spritesModule.GetFreeSpriteIndex();           
            var partSprite = _spritesModule.GetSprite(_spriteIndex);
            partSprite.Tile = _spriteTileTable.GetTile(tileIndex);
            partSprite.SizeX = _spriteDefinition.SizeX;
            partSprite.SizeY = _spriteDefinition.SizeY;
            partSprite.Tile2Offset = 0;
            partSprite.Visible = true;
            partSprite.Palette = 2;
            partSprite.FlipX = false;
            return partSprite;
        }

        public int XPosition
        {
            get => _xPosition.Value - 128;
            set => _xPosition.Value = (byte)(value + 128);
        }

        public int YPosition
        {
            get => _yPosition.Value - 128;
            set => _yPosition.Value = (byte)(value + 128);
        }

        public void UpdatePosition(Sprite bossCore)
        {
            var partSprite = Sprite;
            partSprite.X = (byte)(bossCore.X + XPosition);
            partSprite.Y = (byte)(bossCore.Y + YPosition);
        }
    }
}
