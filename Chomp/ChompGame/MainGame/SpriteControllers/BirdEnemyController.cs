using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class BirdEnemyController : EnemyController
    {
        private const int _hoverSpeed = 20;
        private readonly WorldSprite _player;

        public BirdEnemyController(
            WorldSprite player,
            ChompGameModule gameModule,
            SystemMemoryBuilder memoryBuilder,
            SpriteTileIndex tileIndex) 
            : base(SpriteType.Bird,
                    tileIndex,
                    gameModule,                  
                    memoryBuilder)
        {
            _player = player;
        }

        protected override void UpdateBehavior() 
        {
            _motionController.Update();

            if((_levelTimer % 32) == 0)
            {
                _state.Value++;

                int hoverTarget = _player.Y - 16;

                if (_state < 8)
                {
                    if (WorldSprite.Y < hoverTarget)
                        _motion.TargetYSpeed = _hoverSpeed;
                    else
                        _motion.TargetYSpeed = -_hoverSpeed;

                    WorldSprite.FlipX = _player.X < WorldSprite.X;
                }
                else if(_state >= 8 && _state < 14)
                {
                    if (WorldSprite.Y < _player.Y)
                        _motion.TargetTowards(WorldSprite, _player, _motionController.WalkSpeed);
                    else
                        _state.Value = 0;
                }
                else if (_state >= 14)
                {
                    _motion.TargetXSpeed = 0;
                    _motion.TargetYSpeed = -_hoverSpeed;

                    if (WorldSprite.Y <= hoverTarget)
                    {
                        _state.Value = 0;
                        _motion.TargetYSpeed = 0;
                    }
                }
            }
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _motion.TargetXSpeed = 0;
            _motion.TargetYSpeed = 0;
            _motion.XSpeed = 0;
            _motion.YSpeed = 0;           
            _motion.XAcceleration = _motionController.WalkAccel;
            _motion.YAcceleration = _motionController.WalkAccel;

            _hitPoints.Value = 0;
            _state.Value = 0;
        }
    }

}
