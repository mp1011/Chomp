using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class BombController : ActorController
    {
        private readonly ScenePartsDestroyed _scenePartsDestroyed;
        private readonly CollisionDetector _collisionDetector;
        private readonly PlayerController _playerController;
        private readonly DynamicBlockController _dynamicBlockController;
        private readonly GameBit _isThrown;

        private readonly GameByteEnum<BombState> _bombState;

        enum BombState : byte
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
            : base(SpriteType.Bomb, gameModule, memoryBuilder)
        {
            _scenePartsDestroyed = gameModule.ScenePartsDestroyed;
            _collisionDetector = gameModule.CollissionDetector;
            _playerController = playerController;
            _dynamicBlockController = gameModule.DynamicBlocksController;
            _bombState = new GameByteEnum<BombState>(new MaskedByte(_state.Address, Bit.Right5, memoryBuilder.Memory));
            _isThrown = new GameBit(_state.Address, Bit.Bit5, memoryBuilder.Memory);
        }

        public bool IsCarried => _bombState.Value == BombState.RiseEnd;
        protected override void UpdateActive()
        {
            if(_bombState.Value >= BombState.Explode)
            {
                Motion.Stop();

                if (WorldSprite.Status == WorldSpriteStatus.Active)
                {
                    var sprite = GetSprite();
                    sprite.Palette = 3;
                    sprite.Tile = (byte)(6 + (_levelTimer.Value % 3));
                }

                if (_levelTimer.IsMod(2))
                    _bombState.Value++;

                if(_bombState.Value >= BombState.Destroy)
                {
                    Destroy();
                }
            }
            else if (_bombState.Value < BombState.RiseBegin)
            {
                int ySpeed = Motion.YSpeed;
                _movingSpriteController.Update();

                var collisionInfo = _collisionDetector.DetectCollisions(WorldSprite);
                _movingSpriteController.AfterCollision(collisionInfo);

                if(collisionInfo.DynamicBlockCollision)
                {
                    _dynamicBlockController.HandleBombCollision(collisionInfo);
                }
                
                if ((int)_bombState.Value < 2 && (collisionInfo.YCorrection < 0 || collisionInfo.IsOnGround))
                {
                    _isThrown.Value = false;
                    Motion.XSpeed = 0;
                    Motion.TargetXSpeed = 0;
                    Motion.YSpeed = (int)(-(ySpeed * 0.5));
                    _bombState.Value++;
                }
            }
            else if(_bombState.Value < BombState.RiseEnd)
            {
                WorldSprite.X = _playerController.WorldSprite.X;
                WorldSprite.Y = _playerController.WorldSprite.Y - ((byte)_bombState.Value - 10);
                _bombState.Value++;
            }
            else
            {
                WorldSprite.X = _playerController.WorldSprite.X;
                WorldSprite.Y = _playerController.WorldSprite.Y - 5;

                _playerController.CheckBombThrow(this);
            }
        }

        public void SetCarried()
        {
            _bombState.Value = BombState.RiseEnd;
            WorldSprite.X = _playerController.WorldSprite.X;
            WorldSprite.Y = _playerController.WorldSprite.Y - 5;
        }

        public void DoThrow()
        {
            _bombState.Value = BombState.Idle;
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
            if (sprites == null)
                return;

            if (!_isThrown)
                return;

            sprites.Execute(p =>
            {
                if (p.WorldSprite.Bounds.Intersects(WorldSprite.Bounds)
                    && p.HandleBombCollision(WorldSprite))
                {
                    _isThrown.Value = false;
                    _bombState.Value = BombState.Explode;
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
            _bombState.Value = BombState.RiseBegin;
            Motion.XSpeed = 0;
            Motion.YSpeed = 0;
            Motion.TargetXSpeed = 0;
            Motion.TargetYSpeed = 0;

            if(DestructionBitOffset != 255)
                _scenePartsDestroyed.SetDestroyed(DestructionBitOffset);
        }
    }
}
