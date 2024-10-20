using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteControllers.MotionControllers;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SpriteControllers
{
    class BossBulletController : ActorController, ICollidesWithPlayer, ICollidableSpriteController
    {
        private GameByte _state;
        private GameBit _destroyOnCollision;
        private GameBit _destroyOnTimer;
        private GameBit _decorative;

        private readonly CollisionDetector _collisionDetector;
        private readonly ChompAudioService _audioService;
        private readonly DynamicBlockController _dynamicBlockController;
        private readonly Specs _specs;
        public override IMotion Motion => AcceleratedMotion;

        public AcceleratedMotion AcceleratedMotion { get; }

        public bool DestroyOnTimer
        {
            get => _destroyOnTimer.Value;
            set => _destroyOnTimer.Value = value;
        }

        public bool DestroyOnCollision
        {
            get => _destroyOnCollision.Value;
            set => _destroyOnCollision.Value = value;
        }

        public BossBulletController(
            ChompGameModule gameModule,
            SystemMemoryBuilder memoryBuilder,
            bool destroyOnCollision,
            SpriteType spriteType = SpriteType.BossBullet) : base(spriteType, gameModule, memoryBuilder, SpriteTileIndex.Extra1)
        {
            _collisionDetector = gameModule.CollissionDetector;
            _audioService = gameModule.AudioService;
            _dynamicBlockController = gameModule.DynamicBlocksController;
            _specs = gameModule.Specs;
            _state = memoryBuilder.AddMaskedByte(Bit.Right5);
            _destroyOnCollision = new GameBit(_state.Address, Bit.Bit7, memoryBuilder.Memory);
            _destroyOnTimer = new GameBit(_state.Address, Bit.Bit6, memoryBuilder.Memory);
            _decorative = new GameBit(_state.Address, Bit.Bit5, memoryBuilder.Memory);

            AcceleratedMotion = new AcceleratedMotion(gameModule.LevelTimer, memoryBuilder);
            _destroyOnCollision.Value = destroyOnCollision;
            Palette = SpritePalette.Fire;
        }

        protected override bool DestroyWhenFarOutOfBounds => true;
        protected override bool DestroyWhenOutOfBounds => false;
        protected override bool AlwaysActive => true;

        protected override void UpdateActive() 
        {
            AcceleratedMotion.Apply(WorldSprite);

            if (_destroyOnTimer && _state.Value < 20 && _levelTimer.IsMod(8))
            {
                _state.Value++;
                if (_state.Value == 20)
                    Destroy();
            }

            if (_destroyOnCollision.Value)
            {
                var collisionInfo = _collisionDetector.DetectCollisions(WorldSprite, Motion, checkEdges: false);
                if (collisionInfo.XCorrection != 0 || collisionInfo.YCorrection != 0)
                {
                    Explode();
                }
            }
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _state.Value = 0;
            _decorative.Value = false;
            Palette = SpritePalette.Fire;
            GetSprite().Palette = SpritePalette.Fire;
        }

        protected override void UpdateDying()
        {
            AcceleratedMotion.Apply(WorldSprite);

            if (_levelTimer.Value.IsMod(8))
                _state.Value++;

            var sprite = GetSprite();
            if (_state.Value == 25)
            {
                if (!_decorative.Value)
                {
                    var spriteBounds = WorldSprite.Bounds;

                    _dynamicBlockController.SpawnCoins(
                        new Rectangle(
                            spriteBounds.X,
                            spriteBounds.Y - _specs.TileHeight,
                            spriteBounds.Width,
                            spriteBounds.Height * 2));
                }

                Destroy();
            }
            else if (_state.Value > 20)
            {
                var baseTile = _spriteTileTable.GetTile(SpriteTileIndex.Explosion);
                sprite.Tile = (byte)(baseTile + (_levelTimer.Value % 2));
            }
        }

        public void Explode(bool decorativeOnly=false)
        {
            _decorative.Value = decorativeOnly;
            if (_state.Value >= 20)
                return;

            Palette = SpritePalette.Fire;
            GetSprite().Palette = SpritePalette.Fire;

            _audioService.PlaySound(ChompAudioService.Sound.Break);

            WorldSprite.Status = WorldSpriteStatus.Dying;
            _state.Value = 21;
            Motion.XSpeed = 0;
            Motion.YSpeed = 0;
        }

        public void Smoke()
        {
            if (_state.Value >= 20)
                return;

            Palette = SpritePalette.Platform;
            GetSprite().Palette = SpritePalette.Platform;

            WorldSprite.Status = WorldSpriteStatus.Dying;
            _state.Value = 21;
            Motion.XSpeed = 0;
            Motion.YSpeed = 0;
        }

        public CollisionResult HandlePlayerCollision(WorldSprite player)
        {
            Explode();
            return CollisionResult.HarmPlayer;
        }

        public BombCollisionResponse HandleBombCollision(WorldSprite player) => BombCollisionResponse.None;
        public bool CollidesWithPlayer(PlayerController player) => !_decorative.Value && player.CollidesWith(WorldSprite);
        public bool CollidesWithBomb(WorldSprite bomb) => false;
    }
}
