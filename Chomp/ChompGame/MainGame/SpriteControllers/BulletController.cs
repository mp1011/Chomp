using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class BulletController : ActorController, ICollidesWithPlayer, IEnemyOrBulletSpriteController
    {
        public BulletController(
            ChompGameModule gameModule,
            SystemMemoryBuilder memoryBuilder, 
            SpriteType spriteType) : base(spriteType, gameModule, memoryBuilder)
        { 
        }

        protected override void UpdateActive() 
        {
            if (WorldSprite.Status != WorldSpriteStatus.Active)
                return;

            var sprite = GetSprite();

            if (_state.Value == 0)
            {
                _movingSpriteController.Motion.YSpeed = 0;
                _movingSpriteController.Motion.TargetYSpeed = 0;

                if (sprite.FlipX)
                {
                    _movingSpriteController.Motion.XSpeed = -_movingSpriteController.WalkSpeed;
                    _movingSpriteController.Motion.TargetXSpeed = -_movingSpriteController.WalkSpeed;
                }
                else
                {
                    _movingSpriteController.Motion.XSpeed = _movingSpriteController.WalkSpeed;
                    _movingSpriteController.Motion.TargetXSpeed = _movingSpriteController.WalkSpeed;
                }
            }

            _movingSpriteController.Update();

            if(_levelTimer.Value.IsMod(4))
                _state.Value++;
            
            if (_state.Value == 40 || _state.Value == 60)
            {
                WorldSprite.Destroy();
            }
            else  if (_state.Value > 40)
            {
                sprite.Tile = (byte)(6 + (_levelTimer.Value % 3));
            }
        }

        public void HandlePlayerCollision(MovingWorldSprite player)
        {
            if (_state.Value >= 40)
                return;

            _state.Value = 41;
            _movingSpriteController.Motion.XSpeed = 0;
            _movingSpriteController.Motion.YSpeed = 0;
            _movingSpriteController.Motion.TargetXSpeed = 0;
            _movingSpriteController.Motion.TargetYSpeed = 0;
        }

        public bool HandleBombCollision(MovingWorldSprite player) => false;
    }
}
