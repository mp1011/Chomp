using ChompGame.Data;
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
            SpritesModule spritesModule,
            WorldScroller scroller,
            PlayerController playerController,
            CollisionDetector collisionDetector, SystemMemoryBuilder memoryBuilder, GameByte levelTimer)
            : base(SpriteType.Bomb, spritesModule, scroller, memoryBuilder, levelTimer, Bit.Right5)
        {
            _collisionDetector = collisionDetector;
            _playerController = playerController;
            _isThrown = new GameBit(_state.Address, Bit.Bit5, memoryBuilder.Memory);
        }

        public void Update()
        {
            if (DestroyIfOutOfBounds())
                return;

            if(_state.Value >= (int)BombState.Explode)
            {
                Motion.Stop();

                var sprite = GetSprite();
                sprite.Palette = 3;
                sprite.Tile = (byte)(6 + (_levelTimer.Value % 3));

                if (_levelTimer.IsMod(2))
                    _state.Value++;

                if(_state.Value >= (int)BombState.Destroy)
                {
                    Destroy();
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

            WorldSprite.UpdateSpritePosition();
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

        public void CheckEnemyCollisions<T>(SpriteControllerPool<T> sprites)
          where T : class, ISpriteController, ICollidesWithBomb
        {
            if (!_isThrown)
                return;

            sprites.Execute(p =>
            {
                if (p.WorldSprite.Bounds.Intersects(WorldSprite.Bounds))
                {
                    _isThrown.Value = false;
                    p.HandleBombCollision(WorldSprite);
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
