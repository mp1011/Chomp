using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers.Base
{
    abstract class ActorController : ISpriteController
    {
        protected readonly GameByte _state;
        protected readonly TwoBit _palette;
        protected readonly MovingSpriteController _movingSpriteController;
        protected readonly GameByte _levelTimer;
        private readonly SpritesModule _spritesModule;
        private readonly MaskedByte _index;
        public byte DestructionBitOffset
        {
            get => _index.Value;
            set => _index.Value = value;
        }

        protected virtual bool DestroyWhenFarOutOfBounds => true;

        WorldSprite ISpriteController.WorldSprite => WorldSprite;

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
            ChompGameModule gameModule,
            SystemMemoryBuilder memoryBuilder)
        {
            _spritesModule = gameModule.SpritesModule;
            _state = memoryBuilder.AddByte();

            _palette = new TwoBit(memoryBuilder.Memory,  memoryBuilder.CurrentAddress, 0);
            _index = new MaskedByte(memoryBuilder.CurrentAddress, Bit.Left6, memoryBuilder.Memory, leftShift: 2);
            memoryBuilder.AddByte();

            _movingSpriteController = new MovingSpriteController(
               gameModule.SpritesModule,
               gameModule.LevelTimer,
               memoryBuilder,
               spriteIndex: 255,
               worldScroller: gameModule.WorldScroller,
               spriteDefinition: new SpriteDefinition(spriteType, memoryBuilder.Memory));
            _levelTimer = gameModule.LevelTimer;
        }

        public AcceleratedMotion Motion => _movingSpriteController.Motion;

        public MovingWorldSprite WorldSprite => _movingSpriteController.WorldSprite;

        public byte SpriteIndex
        {
            get => _movingSpriteController.SpriteIndex;
            set => _movingSpriteController.SpriteIndex = value;
        }

        public void InitializeSprite(byte palette)
        {
            _palette.Value = palette;
            Status = WorldSpriteStatus.Active;
            var sprite = GetSprite();
            WorldSprite.ConfigureSprite(sprite);
            sprite.Palette = _palette.Value;
            _state.Value = 0;

            _movingSpriteController.Motion.Stop();
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
                if (DestroyWhenFarOutOfBounds)
                    Destroy();
                else
                    Hide();
            }
            else if(boundsCheck == BoundsCheck.OutOfBounds)
            {
                Hide();
            }
            else if(Status != WorldSpriteStatus.Active)
            {
                WorldSprite.Show();
                if (Status == WorldSpriteStatus.Active)
                {
                    InitializeSprite(_palette.Value);
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

        public void Hide()
        {
            if (Status != WorldSpriteStatus.Active)
                return;
          
            GameDebug.DebugLog($"Sprite {SpriteIndex} hidden by {GetType().Name}");
            WorldSprite.Hide();
        }

        public void Destroy()
        {
            if (Status != WorldSpriteStatus.Active)
                return;

            GameDebug.DebugLog($"Sprite {SpriteIndex} destroyed by {GetType().Name}");
            WorldSprite.Destroy();
        }

        protected abstract void UpdateActive();
    }
}
