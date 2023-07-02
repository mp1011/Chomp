using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.MainGame.Motion;
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
        protected readonly TwoBit _palette;
        protected readonly GameByte _levelTimer;
        protected readonly SpritesModule _spritesModule;
        protected readonly SpriteTileTable _spriteTileTable;
        private readonly GameByte _destructionBitOffset;
        private readonly TwoBitEnum<FallCheck> _fallCheck;
        private readonly AnimationController _animationController;
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

        private GameBit _visible;
        public bool Visible
        {
            get => _visible.Value;
            set => _visible.Value = value;
        }

        public virtual IMotion Motion { get; } = new NoMotion();

        protected ActorController(
            SpriteType spriteType,
            ChompGameModule gameModule,
            SystemMemoryBuilder memoryBuilder,
            SpriteTileIndex tileIndex)
        {
            _spritesModule = gameModule.SpritesModule;
            _spriteTileTable = gameModule.SpriteTileTable;
            _destructionBitOffset = memoryBuilder.AddByte();

            _palette = new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress, shift: 0);
            _fallCheck = new TwoBitEnum<FallCheck>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, shift: 2);
            _visible = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit6, memoryBuilder.Memory);
            _collisionEnabled = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit7, memoryBuilder.Memory);
            memoryBuilder.AddByte();

            WorldSprite = new WorldSprite(
             specs: _spritesModule.Specs,
             spriteTileTable: _spriteTileTable,
             spriteDefinition: new SpriteDefinition(spriteType, memoryBuilder.Memory),
             memoryBuilder: memoryBuilder,
             spritesModule: _spritesModule,
             scroller: gameModule.WorldScroller,
             index: tileIndex);

            _levelTimer = gameModule.LevelTimer;

            _animationController = new AnimationController(new SpriteDefinition(spriteType, memoryBuilder.Memory),
                gameModule.SpriteTileTable, gameModule.LevelTimer);
        }

        public WorldSprite WorldSprite { get; }

        public byte SpriteIndex
        {
            get => WorldSprite.SpriteIndex.Value;
            set => WorldSprite.SpriteIndex.Value = value;
        }

        protected virtual void BeforeInitializeSprite()
        {

        }

        public void InitializeSprite(byte palette)
        {
            GameDebug.DebugLog($"Initialized sprite #{SpriteIndex} from {GetType().Name}", DebugLogFlags.SpriteSpawn);

            _collisionEnabled.Value = true;
            BeforeInitializeSprite();
            _palette.Value = palette;
            Status = WorldSpriteStatus.Active;
            var sprite = GetSprite();
            sprite.FlipY = false;
            WorldSprite.ConfigureSprite(sprite);
            sprite.Palette = _palette.Value;
            OnSpriteCreated(sprite);
            Motion.Stop();
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

        public Sprite GetSprite() => WorldSprite.GetSprite();

        public void Update()
        {
             HideOrDestroyIfOutOfBounds();

            if (WorldSprite.Status == WorldSpriteStatus.Active)
            {
                UpdateActive();
                _animationController.Update(WorldSprite, Motion);
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
          
            GameDebug.DebugLog($"Sprite {SpriteIndex} hidden by {GetType().Name}", DebugLogFlags.SpriteSpawn);
            WorldSprite.Hide();
        }

        public void Destroy()
        {
            if (Status != WorldSpriteStatus.Active)
                return;

            GameDebug.DebugLog($"Sprite {SpriteIndex} destroyed by {GetType().Name}", DebugLogFlags.SpriteSpawn);
            WorldSprite.Destroy();
        }

        protected abstract void UpdateActive();
    }
}
