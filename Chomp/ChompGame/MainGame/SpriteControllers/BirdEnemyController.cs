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
            throw new System.NotImplementedException();
          // _movingSpriteController.Update();

            if((_levelTimer % 32) == 0)
            {
                _state.Value++;

                if (_state < 8)
                {
                    if ((_state % 2) == 0)
                        Motion.TargetYSpeed = _hoverSpeed;
                    else
                        Motion.TargetYSpeed = -_hoverSpeed;

                    WorldSprite.FlipX = _player.X < WorldSprite.X;
                }
                else if(_state >= 8 && _state < 14)
                {
                    Motion.TargetTowards(WorldSprite, _player, _movingSpriteController.WalkSpeed);
                }
                else if (_state >= 14)
                {
                    Motion.TargetXSpeed = 0;
                    Motion.TargetYSpeed = -_hoverSpeed;

                    if (WorldSprite.Y <= 32) //todo avoid hard coding
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
