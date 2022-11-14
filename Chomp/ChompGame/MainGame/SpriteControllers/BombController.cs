using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class BombController : ActorController
    {
        private readonly CollisionDetector _collisionDetector;
        private readonly PlayerController _playerController;

        public BombController(
            SpritesModule spritesModule, 
            PlayerController playerController,
            CollisionDetector collisionDetector, SystemMemoryBuilder memoryBuilder, GameByte levelTimer) 
            : base(SpriteType.Bomb, spritesModule, memoryBuilder, levelTimer)
        {
            _collisionDetector = collisionDetector;
            _playerController = playerController;
        }

        public void Update()
        {
            if (_state.Value < 10)
            {
                _movingSpriteController.Update();

                int ySpeed = Motion.YSpeed;
                var collisionInfo = _collisionDetector.DetectCollisions(WorldSprite, 14); //todo, hard-coding
                if (_state.Value < 2 && collisionInfo.YCorrection < 0)
                {
                    Motion.XSpeed = 0;
                    Motion.TargetXSpeed = 0;
                    Motion.YSpeed = (int)(-(ySpeed * 0.5));
                    _state.Value++;
                }
            }
            else if(_state < 14)
            {
                WorldSprite.X = _playerController.WorldSprite.X;
                WorldSprite.Y = _playerController.WorldSprite.Y - (_state - 10);
                _state.Value++;
            }
            else
            {
                WorldSprite.X = _playerController.WorldSprite.X;
                WorldSprite.Y = _playerController.WorldSprite.Y - 5;

                _playerController.CheckBombThrow(this);
            }
        }

        public void DoThrow()
        {
            _state.Value = 0;

            if (_playerController.Motion.YSpeed < 0)
            {
                Motion.YSpeed = -50;
                Motion.XSpeed = _playerController.WorldSprite.FlipX ? -30 : 30;
            }
            else
            {
                Motion.YSpeed = -10;
                Motion.XSpeed = _playerController.WorldSprite.FlipX ? -50 : 50;
            }

        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            Motion.XSpeed = 0;
            Motion.YSpeed = 0;
        }

        public void SetPickup()
        {
            _state.Value = 10;
            Motion.XSpeed = 0;
            Motion.YSpeed = 0;
            Motion.TargetXSpeed = 0;
            Motion.TargetYSpeed = 0;
        }
    }
}
