using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers.Base
{
    abstract class ActorController : ISpriteController
    {
        protected readonly MaskedByte _state;
        protected readonly TwoBit _palette;
        protected readonly MovingSpriteController _movingSpriteController;
        protected readonly GameByte _levelTimer;

        public byte Palette
        {
            get => _palette.Value;
            set => _palette.Value = value;
        }

        protected ActorController(
            SpriteType spriteType,
            SpritesModule spritesModule,
            SystemMemoryBuilder memoryBuilder,
            GameByte levelTimer,
            Bit stateMask = Bit.Right6)
        {
            _state = memoryBuilder.AddMaskedByte(stateMask);
            _palette = new TwoBit(memoryBuilder.Memory, _state.Address, 6);

            _movingSpriteController = new MovingSpriteController(
               spritesModule,
               levelTimer,
               memoryBuilder,
               spriteIndex: 255,
               spriteDefinition: new SpriteDefinition(spriteType, memoryBuilder.Memory));
            _levelTimer = levelTimer;
        }

        public AcceleratedMotion Motion => _movingSpriteController.Motion;

        public WorldSprite WorldSprite => _movingSpriteController.WorldSprite;

        public byte SpriteIndex
        {
            get => _movingSpriteController.SpriteIndex;
            set => _movingSpriteController.SpriteIndex = value;
        }

        public void ConfigureSprite(Sprite sprite)
        {
            _movingSpriteController.ConfigureSprite(sprite);
            sprite.Palette = _palette.Value;
            _state.Value = 0;
            OnSpriteCreated(sprite);
        }

        protected virtual void OnSpriteCreated(Sprite sprite)
        {

        }

        public bool DestroyIfOutOfBounds()
        {
            if (!WorldSprite.IsInBounds())
            {
                Destroy();
                return true;
            }

            return false;
        }

        public void Destroy()
        {
            GetSprite().Tile = 0;
            SpriteIndex = 255;
        }

        public Sprite GetSprite() => _movingSpriteController.GetSprite();
    }
}
