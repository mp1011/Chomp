using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteControllers.MotionControllers;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SpriteControllers
{
    class BossBulletController : ActorController, ICollidesWithPlayer, IEnemyOrBulletSpriteController
    {
        private GameByte _state;
        private IMotionController _motionController; 
        private readonly CollisionDetector _collisionDetector;
        private readonly ChompAudioService _audioService;
        private readonly DynamicBlockController _dynamicBlockController;
        private readonly Specs _specs;
        public override IMotion Motion => _motionController.Motion;

        public PrecisionMotion PrecisionMotion { get; }

        public BossBulletController(
            ChompGameModule gameModule,
            SystemMemoryBuilder memoryBuilder,
            SpriteType spriteType = SpriteType.BossBullet) : base(spriteType, gameModule, memoryBuilder, SpriteTileIndex.Extra1)
        {
            _collisionDetector = gameModule.CollissionDetector;
            _audioService = gameModule.AudioService;
            _dynamicBlockController = gameModule.DynamicBlocksController;
            _specs = gameModule.Specs;
            _state = memoryBuilder.AddByte();

            var motionController = new SimpleMotionController(memoryBuilder, WorldSprite,
                new SpriteDefinition(spriteType, memoryBuilder.Memory));

            PrecisionMotion = motionController.Motion;
            _motionController = motionController;
            Palette = 3;
        }

        protected override bool DestroyWhenOutOfBounds => true;

        protected override void UpdateActive() 
        {
            var sprite = GetSprite();

            _motionController.Update();

            if(_levelTimer.Value.IsMod(4))
                _state.Value++;


            var collisionInfo = _collisionDetector.DetectCollisions(WorldSprite, _motionController.Motion);
            if(collisionInfo.XCorrection != 0 || collisionInfo.YCorrection != 0)
            {
                Explode();
            }
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _state.Value = 0;
        }

        protected override void UpdateDying()
        {
            if (_levelTimer.Value.IsMod(4))
                _state.Value++;

            var sprite = GetSprite();
            if (_state.Value == 60)
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
                sprite.Tile = (byte)(baseTile + (_levelTimer.Value % 3));
            }
        }

        public void Explode()
        {
            if (_state.Value >= 40)
                return;

            _audioService.PlaySound(ChompAudioService.Sound.Break);
            WorldSprite.Status = WorldSpriteStatus.Dying;
            _state.Value = 41;
            _motionController.Motion.XSpeed = 0;
            _motionController.Motion.YSpeed = 0;
        }

        public void HandlePlayerCollision(WorldSprite player)
        {
            Explode();
        }

        public bool HandleBombCollision(WorldSprite player) => false;

    }
}
