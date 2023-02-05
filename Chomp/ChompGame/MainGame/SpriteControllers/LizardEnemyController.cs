using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class LizardEnemyController : EnemyController
    {
        private readonly CollisionDetector _collisionDetector;
        private readonly IEnemyOrBulletSpriteControllerPool _lizardBulletControllers;

        public LizardEnemyController(
            IEnemyOrBulletSpriteControllerPool lizardBulletControllers,
            ChompGameModule chompGameModule,
            SystemMemoryBuilder memoryBuilder)
            : base(SpriteType.Lizard, chompGameModule, memoryBuilder)
        {
            _lizardBulletControllers = lizardBulletControllers;
            _collisionDetector = chompGameModule.CollissionDetector;
        }

        protected override void UpdateBehavior()
        {
            _movingSpriteController.Update();
            var collision = _collisionDetector.DetectCollisions(_movingSpriteController.WorldSprite);
            _movingSpriteController.AfterCollision(collision);

            if ((_levelTimer % 128) == SpriteIndex)
            {
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

            if (_levelTimer.IsMod(16))
                _state.Value++;

            if (_state.Value == 16 + SpriteIndex)
            {
                _state.Value = 0;
                var fireball = _lizardBulletControllers.TryAddNew(3);
                if (fireball != null)
                {
                    var thisSprite = _movingSpriteController.WorldSprite;
                    fireball.WorldSprite.X = thisSprite.X;
                    fireball.WorldSprite.Y = thisSprite.Y;
                    fireball.WorldSprite.FlipX = thisSprite.FlipX;
                }
            }


        }

      
    }
}
