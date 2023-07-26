using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class RocketEnemyController : EnemyController
    {
        private const int Speed = 40;
        private const int VSpeed = 20;
        private const int Brake = 1;
        private WorldSprite _player;

        public RocketEnemyController(SpriteTileIndex index, ChompGameModule gameModule, WorldSprite player, SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Rocket, index, gameModule, memoryBuilder)
        {
            _player = player;
            Palette = 0;
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _state.Value = 0;
        }

        protected override void UpdateBehavior()
        {
            if (_state.Value == 0)
            {
                if (WorldSprite.X > _player.X)
                {
                    _motion.XSpeed = -Speed;
                    GetSprite().FlipX = false;
                }
                else
                {
                    _motion.XSpeed = Speed;
                    GetSprite().FlipX = true;
                }

                _motion.TargetXSpeed = 0;
                _motion.TargetYSpeed = 0;
                _motion.XAcceleration = Brake;
                _motion.YAcceleration = Brake;

                if (WorldSprite.Y < _player.Y)
                    _motion.YSpeed = VSpeed;
                else if (WorldSprite.Y > _player.Y + 8)
                    _motion.YSpeed = -VSpeed;
                
                _state.Value = 1;
            }
            else if (_state.Value == 1)
            {
                if (_motion.XSpeed == 0)
                {
                    _state.Value = 0;
                }
            }

            _motionController.Update();
        }
    }
}
