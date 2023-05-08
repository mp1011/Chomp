﻿using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class BossBulletController : ActorController, ICollidesWithPlayer, IEnemyOrBulletSpriteController
    {
        private readonly CollisionDetector _collisionDetector;
        private readonly ChompAudioService _audioService;
        private readonly DynamicBlockController _dynamicBlockController;
        private readonly Specs _specs;

        public BossBulletController(
            ChompGameModule gameModule,
            SystemMemoryBuilder memoryBuilder) : base(SpriteType.BossBullet, gameModule, memoryBuilder)
        {
            _collisionDetector = gameModule.CollissionDetector;
            _audioService = gameModule.AudioService;
            _dynamicBlockController = gameModule.DynamicBlocksController;
            _specs = gameModule.Specs;
        }

        protected override bool DestroyWhenOutOfBounds => true;

        protected override void UpdateActive() 
        {
            var sprite = GetSprite();

            _movingSpriteController.Update();

            if(_levelTimer.Value.IsMod(4))
                _state.Value++;


            var collisionInfo = _collisionDetector.DetectCollisions(WorldSprite);
            if(collisionInfo.XCorrection != 0 || collisionInfo.YCorrection != 0)
            {
                Explode();
            }

            if (_state.Value == 60)
            {
                _dynamicBlockController.SpawnCoins(
                    WorldSprite.Bounds);

                Destroy();
            }
            else if (_state.Value > 40)
            {
                sprite.Tile = (byte)(16 + (_levelTimer.Value % 3));
            }
        }

        private void Explode()
        {
            if (_state.Value >= 40)
                return;

            _audioService.PlaySound(ChompAudioService.Sound.Break);
            _state.Value = 41;
            _movingSpriteController.Motion.XSpeed = 0;
            _movingSpriteController.Motion.YSpeed = 0;
            _movingSpriteController.Motion.TargetXSpeed = 0;
            _movingSpriteController.Motion.TargetYSpeed = 0;
        }

        public void HandlePlayerCollision(MovingWorldSprite player)
        {
            Explode();
        }

        public bool HandleBombCollision(MovingWorldSprite player) => false;
    }
}
