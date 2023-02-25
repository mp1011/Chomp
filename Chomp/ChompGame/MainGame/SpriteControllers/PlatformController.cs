using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Helpers;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{

    public enum PlatformType : byte
    {
        LeftRight,
        UpDown,
        Falling,
        Vanishing
    }


    class PlatformController : ActorController
    {
        private GameBit _direction;
        private GameBit _movedBack;
        private GameBit _movedForward;
        private GameBit _playerOnPlatform;
        private MaskedByte _timer;

        private TwoBitEnum<PlatformType> _platformType;

        public PlatformType PlatformType
        {
            get => _platformType.Value;
            set => _platformType.Value = value;
        }

        protected override bool DestroyWhenFarOutOfBounds => false;

        public PlatformController(ChompGameModule gameModule, SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Platform, gameModule, memoryBuilder)
        {
            _timer = new MaskedByte(_state.Address, Bit.Right2, memoryBuilder.Memory);
            _playerOnPlatform = new GameBit(_state.Address, Bit.Bit2, memoryBuilder.Memory);
            _direction = new GameBit(_state.Address, Bit.Bit3, memoryBuilder.Memory);
            _movedBack = new GameBit(_state.Address, Bit.Bit4, memoryBuilder.Memory);
            _movedForward = new GameBit(_state.Address, Bit.Bit5, memoryBuilder.Memory);

            _platformType = new TwoBitEnum<PlatformType>(memoryBuilder.Memory,
                    _state.Address,
                    6);
        }

        protected override void UpdateActive()
        {
            if (_platformType.Value == PlatformType.LeftRight)
                Update_LeftRight();
            else if (_platformType.Value == PlatformType.UpDown)
                Update_UpDown();
            else if (_platformType.Value == PlatformType.Vanishing)
                Update_Vanishing();
            else if (_platformType.Value == PlatformType.Falling)
                Update_Falling();
        }

        private void Update_LeftRight()
        {
            int startX = WorldSprite.X;
            _movingSpriteController.Update();

            _movedBack.Value = WorldSprite.X < startX;
            _movedForward.Value = WorldSprite.X > startX;

            if (_timer.Value == 0)
            {
                _direction.Value = !_direction.Value;

                if (_direction.Value)
                    Motion.SetXSpeed(8);
                else
                    Motion.SetXSpeed(-8);

                _timer.Value++;
            }

            if ((_levelTimer.Value % 32) == 0)
            {
                _timer.Value++;
            }
        }
        private void Update_UpDown()
        {
            int startY = WorldSprite.Y;
            _movingSpriteController.Update();

            _movedBack.Value = WorldSprite.Y < startY;
            _movedForward.Value = WorldSprite.Y > startY;

            if (_timer.Value == 0)
            {
                _direction.Value = !_direction.Value;

                if (_direction.Value)
                    Motion.SetYSpeed(8);
                else
                    Motion.SetYSpeed(-8);

                _timer.Value++;
            }

            if ((_levelTimer.Value % 32) == 0)
            {
                _timer.Value++;
            }
        }

        private void Update_Vanishing()
        {
            throw new System.NotImplementedException();
        }

        private void Update_Falling()
        {
            throw new System.NotImplementedException();
        }

        public void CheckPlayerCollision(PlayerController playerController)
        {
            var collisionInfo = GetPlayerCollisionInfo(playerController);

            if(collisionInfo.IsOnGround 
                || collisionInfo.XCorrection != 0 
                || collisionInfo.YCorrection != 0)
            {
                _playerOnPlatform.Value = true;
                playerController.OnPlatformCollision(collisionInfo);
            }    
            else
            {
                _playerOnPlatform.Value = false;
            }
        }

        private CollisionInfo GetPlayerCollisionInfo(PlayerController playerController)
        {
            if(_playerOnPlatform.Value)
            {
                if (_platformType.Value == PlatformType.LeftRight)
                {
                    int xMove = 0;
                    if (_movedBack.Value)
                        xMove = -1;
                    else if (_movedForward.Value)
                        xMove = 1;

                    playerController.WorldSprite.X += xMove;
                }
                else if (_platformType.Value == PlatformType.UpDown)
                {
                    int yMove = 0;
                    if (_movedBack.Value)
                        yMove = -1;
                    else if (_movedForward.Value)
                        yMove = 1;

                    playerController.WorldSprite.Y += yMove;
                }
            }

            var playerBounds = playerController.WorldSprite.Bounds;
            var platformBounds = WorldSprite.Bounds;

            //check on top
            if (playerController.Motion.YSpeed >= 0
                && playerBounds.Bottom == platformBounds.Top
                && playerBounds.Right >= platformBounds.Left
                && playerBounds.Left <= platformBounds.Right)
            {               
                return new CollisionInfo { IsOnGround = true };
            }

            if(playerController.Motion.YSpeed >= 0
                && playerBounds.Intersects(platformBounds))
            {
                int yOverlap = playerBounds.Bottom - platformBounds.Top;
                if(yOverlap > 0)
                {
                    playerController.WorldSprite.Y -= yOverlap;
                }

                return new CollisionInfo { IsOnGround = true };
            }


            return new CollisionInfo();
        }
    }
}
