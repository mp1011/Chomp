using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers.Bosses
{
    class BossPart
    {
        private SimpleWorldSprite _worldSprite;
        private readonly SpriteTileTable _spriteTileTable;       
        private SpriteDefinition _spriteDefinition;
        private GameByte _xOffset, _yOffset;

        public Sprite Sprite => _worldSprite.Sprite;
        public SimpleWorldSprite WorldSprite => _worldSprite;

        public BossPart(
            ChompGameModule gameModule,
            SystemMemoryBuilder memoryBuilder,
            SpriteDefinition spriteDefinition)
        {
            _worldSprite = new SimpleWorldSprite(gameModule, memoryBuilder);
            _xOffset = memoryBuilder.AddByte(128);
            _yOffset = memoryBuilder.AddByte(128);

            _spriteTileTable = gameModule.SpriteTileTable;
            _spriteDefinition = spriteDefinition;
        }

        public Sprite PrepareSprite(SpriteTileIndex tileIndex)
        {
            _worldSprite.AssignSpriteIndex();
            var partSprite = _worldSprite.Sprite;
            partSprite.Tile = _spriteTileTable.GetTile(tileIndex);
            partSprite.SizeX = _spriteDefinition.SizeX;
            partSprite.SizeY = _spriteDefinition.SizeY;
            partSprite.Tile2Offset = 0;
            partSprite.Visible = true;
            partSprite.Palette = SpritePalette.Enemy1;
            partSprite.FlipX = false;
            return partSprite;
        }

        public int XOffset
        {
            get => _xOffset.Value - 128;
            set => _xOffset.Value = (byte)(value + 128);
        }

        public int YOffset
        {
            get => _yOffset.Value - 128;
            set => _yOffset.Value = (byte)(value + 128);
        }

        public void UpdatePosition(WorldSprite bossCore)
        {
            //var partSprite = Sprite;
            //partSprite.X = (byte)(bossCore.X + XOffset);
            //partSprite.Y = (byte)(bossCore.Y + YOffset);

            WorldSprite.X = bossCore.X + XOffset;
            WorldSprite.Y = bossCore.Y + YOffset;
        }
    }
}
