using ChompGame.Data.Memory;
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
        public BoulderEnemyController(SpriteTileIndex index, ChompGameModule gameModule, SystemMemoryBuilder memoryBuilder, WorldSprite player) 
            : base(SpriteType.Boulder, index, gameModule, memoryBuilder)
        {
            Palette = SpritePalette.Enemy1;
            _collisionDetector = gameModule.CollissionDetector;
            _player = player;
        }

        protected override void BeforeInitializeSprite()
        {
            _stateTimer.Value = 0;
        }

        protected override void UpdateActive()
        {
            _motion.XAcceleration = _motionController.WalkAccel;
            _motion.YAcceleration = _motionController.GravityAccel;

            _motionController.Update();
            var collision = _collisionDetector.DetectCollisions(WorldSprite, _motion);
            _motionController.AfterCollision(collision);

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

            if(_stateTimer.Value == 0 && collision.IsOnGround && _player.Bounds.Right > WorldSprite.Bounds.Left && _player.Bounds.Left < WorldSprite.Bounds.Right)
            {
                _stateTimer.Value = 1;
                _motion.TargetXSpeed = 0;
                _motion.YSpeed = -80;
            }
            else if(_stateTimer.Value == 1 && _motion.YSpeed >= 0 && collision.IsOnGround)
            {
                _stateTimer.Value = 0;
            }
        }
    }
}
