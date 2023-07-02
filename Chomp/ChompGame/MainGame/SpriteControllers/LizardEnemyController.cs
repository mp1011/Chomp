using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Helpers;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using System;

namespace ChompGame.MainGame.SpriteControllers
{
    class LizardEnemyController : EnemyController
    {
        private readonly CollisionDetector _collisionDetector;
        private readonly IEnemyOrBulletSpriteControllerPool _lizardBulletControllers;
        private readonly WorldSprite _player;

        public LizardEnemyController(
            IEnemyOrBulletSpriteControllerPool lizardBulletControllers,
            SpriteTileIndex tileIndex,
            ChompGameModule chompGameModule,
            WorldSprite player,
            SystemMemoryBuilder memoryBuilder)
            : base(SpriteType.Lizard, tileIndex, chompGameModule, memoryBuilder)
        {
            _lizardBulletControllers = lizardBulletControllers;
            _player = player;
            _collisionDetector = chompGameModule.CollissionDetector;
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _motion.TargetXSpeed = _motionController.WalkSpeed;
            _motion.XSpeed = _motionController.WalkSpeed;
            _motion.XAcceleration = _motionController.WalkAccel;
            _hitPoints.Value = 0;
            _state.Value = 0;
        }

        protected override void UpdateBehavior()
        {
            _motionController.Update();
            var collision = _collisionDetector.DetectCollisions(WorldSprite, _motion);
            _motionController.AfterCollision(collision);

           
            if (_state.Value == SpriteIndex && !collision.LeftLedge && !collision.RightLedge)
            {
                _state.Value++;
                if (_motion.TargetXSpeed < 0)
                {
                    _motion.TargetXSpeed = _motionController.WalkSpeed;
                    _motion.XSpeed = _motionController.WalkSpeed;
                }
                else
                {
                    _motion.TargetXSpeed = -_motionController.WalkSpeed;
                    _motion.XSpeed = -_motionController.WalkSpeed;
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
                        var thisSprite = WorldSprite;
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
