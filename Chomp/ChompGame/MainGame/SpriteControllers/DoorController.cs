using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class DoorController : ISpriteController
    {
        private SpriteDefinition _spriteDefinition;
        private SpritesModule _spritesModule;
        private GameByte _levelTimer;

        public byte SpriteIndex
        {
            get => WorldSprite.SpriteIndex;
            set => WorldSprite.SpriteIndex.Value = value;
        }

        public WorldSpriteStatus Status
        {
            get => WorldSprite.Status;
            set => WorldSprite.Status = value;
        }

        public WorldSprite WorldSprite { get; }

        public DoorController(
            ChompGameModule gameModule,
            SystemMemoryBuilder memoryBuilder)
        {
            _spriteDefinition = new SpriteDefinition(SpriteType.Door, memoryBuilder.Memory);
            _spritesModule = gameModule.SpritesModule;
            _levelTimer = gameModule.LevelTimer;

            WorldSprite = new WorldSprite(
                specs: _spritesModule.Specs,
                spriteDefinition: _spriteDefinition,
                memoryBuilder: memoryBuilder,
                spritesModule: _spritesModule,
                scroller: gameModule.WorldScroller);
        }

        public Sprite GetSprite() => WorldSprite.GetSprite();
        public void InitializeSprite(byte palette)
        {
            var sprite = GetSprite();
            WorldSprite.ConfigureSprite(sprite);
            sprite.Palette = 0; //todo
        }

        public void Update()
        {
            WorldSprite.UpdateSprite();

        }
    }
}
