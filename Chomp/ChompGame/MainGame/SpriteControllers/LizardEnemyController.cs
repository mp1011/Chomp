using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class LizardEnemyController : EnemyController
    {
        private readonly CollisionDetector _collisionDetector;
        private readonly SpriteControllerPool<BulletController> _lizardBulletControllers;

        public LizardEnemyController(
            SpriteControllerPool<BulletController> lizardBulletControllers,
            SpritesModule spritesModule,
            ChompAudioService audioService,
            CollisionDetector collisionDetector,
            GameByte levelTimer,
            SystemMemoryBuilder memoryBuilder)
            : base(SpriteType.Lizard, spritesModule, audioService, memoryBuilder, levelTimer)
        {
            _lizardBulletControllers = lizardBulletControllers;
            _collisionDetector = collisionDetector;
        }

        protected override void UpdateBehavior()
        {
            _movingSpriteController.Update();
            _collisionDetector.DetectCollisions(_movingSpriteController.WorldSprite, 14); //todo, hard-coding

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
                var fireball = _lizardBulletControllers.TryAddNew();
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
