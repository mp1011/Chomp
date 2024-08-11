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

        private readonly CollisionDetector _collisionDetector;
        private readonly ChompAudioService _audioService;
        private readonly DynamicBlockController _dynamicBlockController;
        private readonly Specs _specs;
        public override IMotion Motion => AcceleratedMotion;

        public AcceleratedMotion AcceleratedMotion { get; }

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
            _state = memoryBuilder.AddMaskedByte(Bit.Right7);
            _destroyOnCollision = new GameBit(_state.Address, Bit.Bit7, memoryBuilder.Memory);
           
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
            Palette = SpritePalette.Fire;
            GetSprite().Palette = SpritePalette.Fire;
        }

        protected override void UpdateDying()
        {
            if (_levelTimer.Value.IsMod(4))
                _state.Value++;

            var sprite = GetSprite();
            if (_state.Value == 50)
            {
                var spriteBounds = WorldSprite.Bounds;

                _dynamicBlockController.SpawnCoins(
                    new Rectangle(
                        spriteBounds.X,
                        spriteBounds.Y - _specs.TileHeight,
                        spriteBounds.Width,
                        spriteBounds.Height * 2));

                Destroy();
            }
            else if (_state.Value > 40)
            {
                var baseTile = _spriteTileTable.GetTile(SpriteTileIndex.Explosion);
                sprite.Tile = (byte)(baseTile + (_levelTimer.Value % 2));
            }
        }

        public void Explode()
        {
            if (_state.Value >= 40)
                return;

            Palette = SpritePalette.Fire;
            GetSprite().Palette = SpritePalette.Fire;

            _audioService.PlaySound(ChompAudioService.Sound.Break);

            WorldSprite.Status = WorldSpriteStatus.Dying;
            _state.Value = 41;
            Motion.XSpeed = 0;
            Motion.YSpeed = 0;
        }

        public void Smoke()
        {
            if (_state.Value >= 40)
                return;

            Palette = SpritePalette.Platform;
            GetSprite().Palette = SpritePalette.Platform;

            WorldSprite.Status = WorldSpriteStatus.Dying;
            _state.Value = 41;
            Motion.XSpeed = 0;
            Motion.YSpeed = 0;
        }

        public CollisionResult HandlePlayerCollision(WorldSprite player)
        {
            Explode();
            return CollisionResult.HarmPlayer;
        }

        public BombCollisionResponse HandleBombCollision(WorldSprite player) => BombCollisionResponse.None;
        public bool CollidesWithPlayer(PlayerController player) => player.CollidesWith(WorldSprite);
        public bool CollidesWithBomb(WorldSprite bomb) => false;
    }
}
