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
        private readonly SpritesModule _spritesModule;

        public WorldSpriteStatus Status
        {
            get => WorldSprite.Status;
            set => WorldSprite.Status = value;
        }

        public byte Palette
        {
            get => _palette.Value;
            set => _palette.Value = value;
        }

        protected ActorController(
            SpriteType spriteType,
            SpritesModule spritesModule,
            WorldScroller scroller,
            SystemMemoryBuilder memoryBuilder,
            GameByte levelTimer,
            Bit stateMask = Bit.Right6)
        {
            _spritesModule = spritesModule;
            _state = memoryBuilder.AddMaskedByte(stateMask);
            _palette = new TwoBit(memoryBuilder.Memory, _state.Address, 6);

            _movingSpriteController = new MovingSpriteController(
               spritesModule,
               levelTimer,
               memoryBuilder,
               spriteIndex: 255,
               worldScroller: scroller,
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

        public void InitializeSprite()
        {
            Status = WorldSpriteStatus.Active;
            var sprite = GetSprite();
            WorldSprite.ConfigureSprite(sprite);
            sprite.Palette = _palette.Value;
            _state.Value = 0;
            OnSpriteCreated(sprite);
        }

        protected virtual void OnSpriteCreated(Sprite sprite)
        {

        }

        private void HideOrDestroyIfOutOfBounds()
        {
            var boundsCheck = WorldSprite.CheckInBounds();

            if(boundsCheck == BoundsCheck.FarOutOfBounds)
            {
                WorldSprite.Destroy();
            }
            else if(boundsCheck == BoundsCheck.OutOfBounds)
            {
                WorldSprite.Hide();
            }
            else if(Status != WorldSpriteStatus.Active)
            {
                WorldSprite.Show();
                if (Status == WorldSpriteStatus.Active)
                {
                    InitializeSprite();
                }
            }
        }

        public Sprite GetSprite() => _movingSpriteController.GetSprite();

        public void Update()
        {
            HideOrDestroyIfOutOfBounds();

            if (WorldSprite.Status == WorldSpriteStatus.Active)
            {
                UpdateActive();
                WorldSprite.UpdateSprite();
            }
        }

        protected abstract void UpdateActive();
    }
}
