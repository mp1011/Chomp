using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Helpers;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteControllers.MotionControllers;
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
        private IMotionController _motionController;
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
            : base(SpriteType.Platform, gameModule, memoryBuilder, SpriteTileIndex.Platform)
        {
            int address = memoryBuilder.CurrentAddress;
            memoryBuilder.AddByte();
            _timer = new MaskedByte(address, Bit.Right2, memoryBuilder.Memory);
            _playerOnPlatform = new GameBit(address, Bit.Bit2, memoryBuilder.Memory);
            _direction = new GameBit(address, Bit.Bit3, memoryBuilder.Memory);
            _movedBack = new GameBit(address, Bit.Bit4, memoryBuilder.Memory);
            _movedForward = new GameBit(address, Bit.Bit5, memoryBuilder.Memory);

            _platformType = new TwoBitEnum<PlatformType>(memoryBuilder.Memory,
                    address,
                    6);

            _motionController = new SimpleMotionController(memoryBuilder, WorldSprite, 
                new SpriteDefinition(SpriteType.Platform, memoryBuilder.Memory));
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
            _motionController.Update();

            _movedBack.Value = WorldSprite.X < startX;
            _movedForward.Value = WorldSprite.X > startX;

            if (_timer.Value == 0)
            {
                _direction.Value = !_direction.Value;

                if (_direction.Value)
                    _motionController.Motion.XSpeed = 8;
                else
                    _motionController.Motion.XSpeed = -8;

                _timer.Value++;
            }

            if ((_levelTimer.Value % 64) == 0)
            {
                _timer.Value++;
            }
        }
        private void Update_UpDown()
        {
            int startY = WorldSprite.Y;
            _motionController.Update();

            _movedBack.Value = WorldSprite.Y < startY;
            _movedForward.Value = WorldSprite.Y > startY;

            if (_timer.Value == 0)
            {
                _direction.Value = !_direction.Value;

                if (_direction.Value)
                    _motionController.Motion.YSpeed = 8;
                else
                    _motionController.Motion.YSpeed = -8;

                _timer.Value++;
            }

            if (_levelTimer.Value.IsMod(64))
            {
                _timer.Value++;
            }
        }

        public void Dephase(int y)
        {
            _direction.Value = ((y/4) % 2) == 0;
        }

        private void Update_Vanishing()
        {
            throw new System.NotImplementedException();
        }

        private void Update_Falling()
        {
        }

        public void CheckPlayerCollision(PlayerController playerController)
        {
            var collisionInfo = GetPlayerCollisionInfo(playerController);

            if(collisionInfo.IsOnGround 
                || collisionInfo.XCorrection != 0 
                || collisionInfo.YCorrection != 0)
            {
                _playerOnPlatform.Value = collisionInfo.XCorrection == 0 
                    && (collisionInfo.YCorrection <= 0 || collisionInfo.IsOnGround);
                playerController.OnPlatformCollision(collisionInfo);
            }    
            else
            {
                _playerOnPlatform.Value = false;
            }
        }

        private CollisionInfo GetPlayerCollisionInfo(PlayerController playerController)
        {
            if (_playerOnPlatform.Value)
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

            if (!playerBounds.Intersects(platformBounds))
                return new CollisionInfo();
            
            int leftXOverlap = playerBounds.Right - platformBounds.Left;
            int rightXOverlap = platformBounds.Right - playerBounds.Left;
            int topYOverlap = playerBounds.Bottom - platformBounds.Top;
            int bottomYOverlap = platformBounds.Bottom - playerBounds.Top;

            if (leftXOverlap > 3)
                leftXOverlap = -1;
            if ( rightXOverlap > 3)
                rightXOverlap = -1;
            if (topYOverlap > 3)
                topYOverlap = -1;
            if (bottomYOverlap > 3)
                bottomYOverlap = -1;

            if (topYOverlap > 0)
            {
                leftXOverlap = -1;
                rightXOverlap = -1;
            }

            if(topYOverlap >= 0 && playerController.Motion.YSpeed >= 0)
            {
                playerController.WorldSprite.Y -= topYOverlap;
                return new CollisionInfo { IsOnGround = true };
            }

            if (bottomYOverlap > 0 && playerController.Motion.YSpeed <= 0)
            {
                playerController.WorldSprite.Y += bottomYOverlap;
                return new CollisionInfo { IsOnGround = true, YCorrection = bottomYOverlap };
            }

            if (leftXOverlap > 0 && playerController.Motion.XSpeed >= 0)
            {
                playerController.WorldSprite.X -= leftXOverlap;
                return new CollisionInfo { XCorrection = leftXOverlap };
            }

            if (rightXOverlap > 0 && playerController.Motion.XSpeed <= 0)
            {
                playerController.WorldSprite.X += rightXOverlap;
                return new CollisionInfo { XCorrection = rightXOverlap };
            }

            return new CollisionInfo();
        }
    }
}
