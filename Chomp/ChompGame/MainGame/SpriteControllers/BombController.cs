using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
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
        private readonly GameBit _isThrown;

        enum BombState
        {
            Idle=0,
            RiseBegin=10,
            RiseEnd=14,
            Explode=20,
            Destroy=30

        }

        public BombController(
                ChompGameModule gameModule,
                PlayerController playerController,
                SystemMemoryBuilder memoryBuilder)
            : base(SpriteType.Bomb, gameModule, memoryBuilder, Bit.Right5)
        {
            _collisionDetector = gameModule.CollissionDetector;
            _playerController = playerController;
            _isThrown = new GameBit(_state.Address, Bit.Bit5, memoryBuilder.Memory);
        }

        protected override void UpdateActive()
        {
            if(_state.Value >= (int)BombState.Explode)
            {
                Motion.Stop();

                if (WorldSprite.Status == WorldSpriteStatus.Active)
                {
                    var sprite = GetSprite();
                    sprite.Palette = 3;
                    sprite.Tile = (byte)(6 + (_levelTimer.Value % 3));
                }

                if (_levelTimer.IsMod(2))
                    _state.Value++;

                if(_state.Value >= (int)BombState.Destroy)
                {
                    WorldSprite.Destroy();
                }
            }
            else if (_state.Value < (int)BombState.RiseBegin)
            {
                _movingSpriteController.Update();

                int ySpeed = Motion.YSpeed;
                var collisionInfo = _collisionDetector.DetectCollisions(WorldSprite);
                if (_state.Value < 2 && collisionInfo.YCorrection < 0)
                {
                    _isThrown.Value = false;
                    Motion.XSpeed = 0;
                    Motion.TargetXSpeed = 0;
                    Motion.YSpeed = (int)(-(ySpeed * 0.5));
                    _state.Value++;
                }
            }
            else if(_state < (int)BombState.RiseEnd)
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
            _isThrown.Value = true;

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

        public void CheckEnemyCollisions(IEnemyOrBulletSpriteControllerPool sprites)
        {
            if (!_isThrown)
                return;

            sprites.Execute(p =>
            {
                if (p.WorldSprite.Bounds.Intersects(WorldSprite.Bounds)
                    && p.HandleBombCollision(WorldSprite))
                {
                    _isThrown.Value = false;
                    _state.Value = (int)BombState.Explode;
                }                
            });
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
