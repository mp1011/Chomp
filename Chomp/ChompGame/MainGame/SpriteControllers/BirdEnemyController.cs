using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class BirdEnemyController : EnemyController
    {
        private const int _hoverSpeed = 20;
        private readonly MovingWorldSprite _player;

        public BirdEnemyController(
            MovingWorldSprite player,
            ChompGameModule gameModule,
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Bird,
                    gameModule,                  
                    memoryBuilder)
        {
            _player = player;
        }

        protected override void UpdateBehavior() 
        {
            _movingSpriteController.Update();

            if((_levelTimer % 32) == 0)
            {
                _state.Value++;

                int hoverTarget = _player.Y - 16;

                if (_state < 8)
                {
                    if (WorldSprite.Y < hoverTarget)
                        Motion.TargetYSpeed = _hoverSpeed;
                    else
                        Motion.TargetYSpeed = -_hoverSpeed;

                    WorldSprite.FlipX = _player.X < WorldSprite.X;
                }
                else if(_state >= 8 && _state < 14)
                {
                    if (WorldSprite.Y < _player.Y)
                        Motion.TargetTowards(WorldSprite, _player, _movingSpriteController.WalkSpeed);
                    else
                        _state.Value = 0;
                }
                else if (_state >= 14)
                {
                    Motion.TargetXSpeed = 0;
                    Motion.TargetYSpeed = -_hoverSpeed;

                    if (WorldSprite.Y <= hoverTarget)
                    {
                        _state.Value = 0;
                        Motion.TargetYSpeed = 0;
                    }
                }
            }
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            Motion.TargetXSpeed = 0;
            Motion.TargetYSpeed = 0;
            Motion.XSpeed = 0;
            Motion.YSpeed = 0;

            Motion.XAcceleration = _movingSpriteController.WalkAccel;
            Motion.YAcceleration = _movingSpriteController.WalkAccel;
        }
    }

}
