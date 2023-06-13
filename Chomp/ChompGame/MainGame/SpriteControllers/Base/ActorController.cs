using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers.Base
{
    public enum FallCheck : byte
    {
        None,
        ScreenHeight,
        WrapAround
    }

    abstract class ActorController : ISpriteController
    {
        protected readonly GameByte _state;
        protected readonly TwoBit _palette;
        protected readonly MovingSpriteController _movingSpriteController;
        protected readonly GameByte _levelTimer;
        protected readonly SpritesModule _spritesModule;
        protected readonly SpriteTileTable _spriteTileTable;
        private readonly GameByte _destructionBitOffset;
        private readonly TwoBitEnum<FallCheck> _fallCheck;

        public FallCheck FallCheck
        {
            get => _fallCheck.Value;
            set => _fallCheck.Value = value;
        }

        public byte DestructionBitOffset
        {
            get => _destructionBitOffset.Value;
            set => _destructionBitOffset.Value = value;
        }

        protected virtual bool DestroyWhenFarOutOfBounds => true;
        protected virtual bool DestroyWhenOutOfBounds => false;

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

        private GameBit _collisionEnabled;
        public bool CollisionEnabled
        {
            get => _collisionEnabled.Value;
            set => _collisionEnabled.Value = value;
        }

        protected MaskedByte _hitPoints;

        protected ActorController(
            SpriteType spriteType,
            ChompGameModule gameModule,
            SystemMemoryBuilder memoryBuilder,
            SpriteTileIndex tileIndex)
        {
            _spritesModule = gameModule.SpritesModule;
            _spriteTileTable = gameModule.SpriteTileTable;
            _state = memoryBuilder.AddByte();

            _destructionBitOffset = memoryBuilder.AddByte();
            memoryBuilder.AddByte();
            _movingSpriteController = new MovingSpriteController(
               gameModule.SpritesModule,
               _spriteTileTable,
               gameModule.LevelTimer,
               memoryBuilder,
               spriteIndex: tileIndex,
               worldScroller: gameModule.WorldScroller,
               spriteDefinition: new SpriteDefinition(spriteType, memoryBuilder.Memory));

            _palette = new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress, shift: 0);
            _fallCheck = new TwoBitEnum<FallCheck>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, shift: 2);
            _hitPoints = new MaskedByte(memoryBuilder.CurrentAddress, (Bit)112, memoryBuilder.Memory, 4);
            _collisionEnabled = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit7, memoryBuilder.Memory);
            memoryBuilder.AddByte();
            _levelTimer = gameModule.LevelTimer;
        }

        public AcceleratedMotion Motion => _movingSpriteController.Motion;

        public MovingWorldSprite WorldSprite => _movingSpriteController.WorldSprite;

        public byte SpriteIndex
        {
            get => _movingSpriteController.SpriteIndex;
            set => _movingSpriteController.SpriteIndex = value;
        }

        protected virtual void BeforeInitializeSprite()
        {

        }

        public void InitializeSprite(byte palette)
        {
            _collisionEnabled.Value = true;
            BeforeInitializeSprite();
            _palette.Value = palette;
            Status = WorldSpriteStatus.Active;
            var sprite = GetSprite();
            sprite.FlipY = false;
            WorldSprite.ConfigureSprite(sprite);
            sprite.Palette = _palette.Value;
            _state.Value = 0;

            _movingSpriteController.Motion.Stop();
            _hitPoints.Value = 0;
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
                if (DestroyWhenOutOfBounds)
                    Destroy();
                else
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

                CheckFall();
            }
        }

        private void CheckFall()
        {
            if (FallCheck == FallCheck.None)
                return;

            if(FallCheck == FallCheck.ScreenHeight && WorldSprite.Y > _spritesModule.Specs.ScreenHeight + 16)
            {
                HandleFall();
                return;
            }

            //todo, not sure how this was supposed to work
            //if (FallCheck == FallCheck.WrapAround && WorldSprite.Y < _spritesModule.Specs.ScreenHeight + 4)
            //{
            //    HandleFall();
            //    return;
            //}
        }

        protected virtual void HandleFall()
        {

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
