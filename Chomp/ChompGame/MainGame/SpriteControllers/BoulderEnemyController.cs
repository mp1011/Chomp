using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class BoulderEnemyController : EnemyController
    {
        private readonly CollisionDetector _collisionDetector;
        private readonly WorldSprite _player;
        private readonly EnemyOrBulletSpriteControllerPool<BossBulletController> _bulletControllers;
        protected override int PointsForEnemy => 500;
        public BoulderEnemyController(EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers, SpriteTileIndex index, ChompGameModule gameModule, SystemMemoryBuilder memoryBuilder, WorldSprite player) 
            : base(SpriteType.Boulder, index, gameModule, memoryBuilder)
        {
            Palette = SpritePalette.Enemy1;
            _collisionDetector = gameModule.CollisionDetector;
            _player = player;
            _bulletControllers = bulletControllers;
        }

        protected override void BeforeInitializeSprite()
        {
            _stateTimer.Value = 0;
            _hitPoints.Value = 2;
        }

        private void ThrowFireball(int xSpeed)
        {
            var bullet = _bulletControllers.TryAddNew();
            if (bullet == null)
                return;

            bullet.AcceleratedMotion.SetXSpeed(xSpeed);
            bullet.AcceleratedMotion.YSpeed = -30;
            bullet.AcceleratedMotion.TargetYSpeed = 80;
            bullet.AcceleratedMotion.YAcceleration = 10;

            bullet.WorldSprite.X = WorldSprite.X;
            bullet.WorldSprite.Y = WorldSprite.Y;
            bullet.WorldSprite.FlipX = WorldSprite.FlipX;
        }

        protected override void UpdateHidden()
        {
            _worldScroller.OffsetCamera(0, 0);
        }

        protected override void UpdateActive()
        {
            _motion.XAcceleration = _motionController.WalkAccel;
            _motion.YAcceleration = _motionController.GravityAccel;

            _motionController.Update();
            var collision = _collisionDetector.DetectCollisions(WorldSprite, _motion);
            _motionController.AfterCollision(collision);
            _worldScroller.OffsetCamera(0, 0);

            if (_stateTimer.Value == 0)
            {
                if (_levelTimer < 96)
                {                   
                    if (WorldSprite.X > _player.X)
                        _motion.TargetXSpeed = -_motionController.WalkSpeed;
                    else
                        _motion.TargetXSpeed = _motionController.WalkSpeed;
                }
                else
                {
                    _motion.TargetXSpeed = 0;
                }
            }

            bool doJump = _levelTimer.Value == 0 &&
                        _player.Center.X > WorldSprite.Center.X - 20 &&
                        _player.Center.X < WorldSprite.Center.X + 20;

            doJump = doJump || _player.Bounds.Right > WorldSprite.Bounds.Left && _player.Bounds.Left < WorldSprite.Bounds.Right;

            if (_stateTimer.Value > 0)
                doJump = false;

            if (_stateTimer.Value == 0 && collision.IsOnGround && doJump)
            {
                _stateTimer.Value = 1;
                _motion.TargetXSpeed = 0;
                _motion.YSpeed = -80;
            }
            else if(_stateTimer.Value == 1 && _motion.YSpeed >= 0 && collision.IsOnGround)
            {
                _stateTimer.Value = 2;
                _motion.SetXSpeed(0);
                ThrowFireball(-40);
                ThrowFireball(40);
                ThrowFireball(-60);
                ThrowFireball(60);


                _audioService.PlaySound(ChompAudioService.Sound.Rumble);
            }
            else if(_stateTimer.Value >= 2 && _stateTimer.Value < 8)
            {
                if (_levelTimer.IsMod(8))
                    _stateTimer.Value++;

                if(_levelTimer.Value.IsMod(2))
                {
                    _worldScroller.OffsetCamera(_rng.Generate(1), 1);
                }
                else
                {
                    _worldScroller.OffsetCamera(0, 0);
                }
            }
            else if (_stateTimer.Value >= 8)
            {
                _worldScroller.OffsetCamera(0, 0);
                _stateTimer.Value = 0;
            }          
        }
    }
}
