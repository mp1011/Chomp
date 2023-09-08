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
        private readonly ICollidableSpriteControllerPool _lizardBulletControllers;
        private readonly WorldSprite _player;
      
        public LizardEnemyController(
            ICollidableSpriteControllerPool lizardBulletControllers,
            SpriteTileIndex tileIndex,
            ChompGameModule chompGameModule,
            WorldSprite player,
            SystemMemoryBuilder memoryBuilder)
            : base(SpriteType.Lizard, tileIndex, chompGameModule, memoryBuilder)
        {
            _lizardBulletControllers = lizardBulletControllers;
            _player = player;
            _collisionDetector = chompGameModule.CollissionDetector;
            Palette = 2;
        }


        protected override void OnSpriteCreated(Sprite sprite)
        {
            _motion.TargetXSpeed = _motionController.WalkSpeed;
            _motion.XSpeed = _motionController.WalkSpeed;
            _motion.XAcceleration = _motionController.WalkAccel;
            _hitPoints.Value = 1;
            _stateTimer.Value = 0;
        }

        protected override void UpdateActive()
        {
            _motionController.Update();
            var collision = _collisionDetector.DetectCollisions(WorldSprite, _motion);
            _motionController.AfterCollision(collision);

            if (_motion.TargetXSpeed == 0 || _levelTimer.IsMod(16))
            {
                _stateTimer.Value++;

                if (_stateTimer.Value == 8 && !collision.LeftLedge && !collision.RightLedge)
                {
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
                else if (_stateTimer.Value == 15 && _rng.RandomChance(50))
                {
                    int distanceToPlayer = Math.Abs(WorldSprite.X - _player.X);
                    if (distanceToPlayer < 64)
                    {
                        var fireball = _lizardBulletControllers.TryAddNew();
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
            }
        }
    }
}
