using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using System;

namespace ChompGame.MainGame.SpriteControllers
{
    class LizardEnemyController : EnemyController
    {
        private readonly CollisionDetector _collisionDetector;
        private readonly IEnemyOrBulletSpriteControllerPool _lizardBulletControllers;
        private readonly MovingWorldSprite _player;

        public LizardEnemyController(
            IEnemyOrBulletSpriteControllerPool lizardBulletControllers,
            ChompGameModule chompGameModule,
            MovingWorldSprite player,
            SystemMemoryBuilder memoryBuilder)
            : base(SpriteType.Lizard, chompGameModule, memoryBuilder)
        {
            _lizardBulletControllers = lizardBulletControllers;
            _player = player;
            _collisionDetector = chompGameModule.CollissionDetector;
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            Motion.TargetXSpeed = _movingSpriteController.WalkSpeed;
            Motion.XSpeed = _movingSpriteController.WalkSpeed;
            Motion.XAcceleration = _movingSpriteController.WalkAccel;
        }

        protected override void UpdateBehavior()
        {
            _movingSpriteController.Update();
            var collision = _collisionDetector.DetectCollisions(_movingSpriteController.WorldSprite);
            _movingSpriteController.AfterCollision(collision);

           
            if (_state.Value == SpriteIndex && !collision.LeftLedge && !collision.RightLedge)
            {
                _state.Value++;
                if (Motion.TargetXSpeed < 0)
                {
                    Motion.TargetXSpeed = _movingSpriteController.WalkSpeed;
                    Motion.XSpeed = _movingSpriteController.WalkSpeed;
                }
                else
                {
                    Motion.TargetXSpeed = -_movingSpriteController.WalkSpeed;
                    Motion.XSpeed = -_movingSpriteController.WalkSpeed;
                }
            }
            else if (_state.Value == 16 + SpriteIndex)
            {
                _state.Value++;
                int distanceToPlayer = Math.Abs(WorldSprite.X - _player.X);
                if (distanceToPlayer < 64)
                {
                    var fireball = _lizardBulletControllers.TryAddNew(3);
                    if (fireball != null)
                    {
                        _audioService.PlaySound(ChompAudioService.Sound.Fireball);
                        var thisSprite = _movingSpriteController.WorldSprite;
                        fireball.WorldSprite.X = thisSprite.X;
                        fireball.WorldSprite.Y = thisSprite.Y;
                        fireball.WorldSprite.FlipX = thisSprite.FlipX;
                    }
                }
            }
            else if (_state.Value == 32)
            {
                _state.Value = 0;
            }
            else if (_levelTimer.IsMod(8))
                _state.Value++;

        }
    }
}
