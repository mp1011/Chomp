using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.MainGame.Motion;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers.Base
{
    public enum FallCheck : byte
    {
        None,
        ScreenHeight,
        NametableHeight
    }

    abstract class ActorController : ISpriteController
    {
        protected readonly RandomModule _rng;
        protected readonly TwoBitEnum<SpritePalette> _palette;
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

        protected virtual bool AlwaysActive => false;

        WorldSprite ISpriteController.WorldSprite => WorldSprite;

        public WorldSpriteStatus Status
        {
            get => WorldSprite.Status;
            set => WorldSprite.Status = value;
        }

        public SpritePalette Palette
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

        public bool Visible
        {
            get => WorldSprite.Visible;
            set => WorldSprite.Visible = value;
        }

        public virtual IMotion Motion { get; } = new NoMotion();

        protected ActorController(
            SpriteType spriteType,
            ChompGameModule gameModule,
            SystemMemoryBuilder memoryBuilder,
            SpriteTileIndex tileIndex)
        {
            _rng = gameModule.RandomModule;
            _spritesModule = gameModule.SpritesModule;
            _spriteTileTable = gameModule.SpriteTileTable;
            _destructionBitOffset = memoryBuilder.AddByte();

            _palette = new TwoBitEnum<SpritePalette>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, shift: 0);
            _fallCheck = new TwoBitEnum<FallCheck>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, shift: 2);
            _collisionEnabled = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit7, memoryBuilder.Memory);
            memoryBuilder.AddByte();

            WorldSprite = new WorldSprite(
             specs: _spritesModule.Specs,
             spriteTileTable: _spriteTileTable,
             spriteDefinition: new SpriteDefinition(spriteType, memoryBuilder.Memory),
             memoryBuilder: memoryBuilder,
             spritesModule: _spritesModule,
             scroller: gameModule.WorldScroller,
             index: tileIndex,
             palette: _palette);

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

        public void InitializeSprite()
        {
            GameDebug.DebugLog($"Initialized sprite #{SpriteIndex} from {GetType().Name}", DebugLogFlags.SpriteSpawn);

            _collisionEnabled.Value = true;
            BeforeInitializeSprite();
            Status = WorldSpriteStatus.Active;
            var sprite = GetSprite();
            sprite.FlipY = false;
            sprite.FlipX = false;
            WorldSprite.ConfigureSprite(sprite);
            sprite.Palette = _palette.Value;
            Palette = _palette.Value;
            OnSpriteCreated(sprite);
            Motion.Stop();
        }

        protected virtual void OnSpriteCreated(Sprite sprite)
        {

        }

        private void HideOrDestroyIfOutOfBounds()
        {
            if (AlwaysActive)
                return;

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
                    InitializeSprite();
                }
            }
        }

        public Sprite GetSprite() => WorldSprite.GetSprite();

        public void Update()
        {
            if (WorldSprite.Status == WorldSpriteStatus.Dying)
            {
                UpdateDying();
                WorldSprite.UpdateSprite();
                return;
            }

            HideOrDestroyIfOutOfBounds();

            if (WorldSprite.Status == WorldSpriteStatus.Active)
            {
                UpdateActive();
                _animationController.Update(WorldSprite, Motion);
                WorldSprite.UpdateSprite();

                CheckFall();
            }
            else if(WorldSprite.Status == WorldSpriteStatus.Hidden)
            {
                UpdateHidden();
            }
        }

        protected virtual void UpdateHidden() { }

        private void CheckFall()
        {
            if (FallCheck == FallCheck.None)
                return;

            if(FallCheck == FallCheck.ScreenHeight && WorldSprite.Y > _spritesModule.Specs.ScreenHeight + 8)
            {
                HandleFall();
                return;
            }

            if (FallCheck == FallCheck.NametableHeight && WorldSprite.Y >= _spritesModule.Specs.NameTablePixelHeight)
            {
                HandleFall();
                return;
            }
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
            if (Status != WorldSpriteStatus.Active && Status != WorldSpriteStatus.Dying)
                return;

            GameDebug.DebugLog($"Sprite {SpriteIndex} destroyed by {GetType().Name}", DebugLogFlags.SpriteSpawn);
            WorldSprite.Destroy();
        }

        public void EnsureInFrontOf(ISpriteController other)
        {
            if (SpriteIndex >= other.SpriteIndex)
                return;

            var otherSpriteIndex = other.SpriteIndex;
            other.SpriteIndex = SpriteIndex;
            SpriteIndex = otherSpriteIndex;

            var thisSprite = GetSprite();
            thisSprite.Palette = Palette;
            WorldSprite.ConfigureSprite(thisSprite);
            WorldSprite.UpdateSprite();

            var otherSprite = other.GetSprite();
            otherSprite.Palette = other.Palette;
            other.WorldSprite.ConfigureSprite(otherSprite);
            other.WorldSprite.UpdateSprite();
        }

        protected abstract void UpdateActive();
        protected virtual void UpdateDying() { }
    }
}
