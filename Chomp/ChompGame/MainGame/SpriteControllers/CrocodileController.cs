using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Helpers;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class CrocodileController : EnemyController
    {
        private readonly WorldSprite _player;
        private readonly CollisionDetector _collisionDetector;

        public CrocodileController(
            WorldSprite player,
            SpriteTileIndex index, 
            ChompGameModule gameModule, 
            SystemMemoryBuilder memoryBuilder) : base(SpriteType.Crocodile, index, gameModule, memoryBuilder)
        {
            _player = player;
            Palette = 2;
            _collisionDetector = gameModule.CollissionDetector;
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
            if (collision.HitLeftWall)
            {
                _stateTimer.Value = 15;
                _motion.TargetXSpeed = _motionController.WalkSpeed;
            }
            else if(collision.HitRightWall)
            {
                _stateTimer.Value = 15;
                _motion.TargetXSpeed = -_motionController.WalkSpeed;
            }

            _motionController.AfterCollision(collision);

            if (_levelTimer.Value.IsMod(32))
                _audioService.PlaySound(ChompAudioService.Sound.CrocodileBark);

            if(_stateTimer.Value > 0 )
            {
                if (_levelTimer.IsMod(2))
                    _stateTimer.Value--;         
            }
            else if (_motion.XSpeed == 0 || _levelTimer.Value.IsMod(32))
            {
                _motion.XAcceleration = _motionController.WalkAccel;
                if (_player.X < WorldSprite.X)                
                    _motion.TargetXSpeed = -_motionController.WalkSpeed;
                else
                    _motion.TargetXSpeed = _motionController.WalkSpeed;
            }
        }
    }
}
