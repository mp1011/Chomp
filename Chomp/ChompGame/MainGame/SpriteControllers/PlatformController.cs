using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Helpers;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class PlatformController : ActorController
    {
        private GameBit _direction;
        private GameBit _movedLeft;
        private GameBit _movedRight;
        private MaskedByte _timer;

        public PlatformController(ChompGameModule gameModule, SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Platform, gameModule, memoryBuilder)
        {
            _timer = new MaskedByte(_state.Address, Bit.Right4, memoryBuilder.Memory);
            _direction = new GameBit(_state.Address, Bit.Bit4, memoryBuilder.Memory);
            _movedLeft = new GameBit(_state.Address, Bit.Bit5, memoryBuilder.Memory);
            _movedRight = new GameBit(_state.Address, Bit.Bit6, memoryBuilder.Memory);

        }

        protected override void UpdateActive()
        {
            int startX = WorldSprite.X;
            _movingSpriteController.Update();

            _movedLeft.Value = WorldSprite.X < startX;
            _movedRight.Value = WorldSprite.X > startX;

            if (_timer.Value == 0)
            {
                _direction.Value = !_direction.Value;

                if(_direction.Value)
                    Motion.SetXSpeed(8);
                else
                    Motion.SetXSpeed(-8);

                _timer.Value++;
            }

            if ((_levelTimer.Value % 8) == 0)
            {
                _timer.Value++;
            }
        }

        public void CheckPlayerCollision(PlayerController playerController)
        {
            var collisionInfo = GetPlayerCollisionInfo(playerController);

            if(collisionInfo.IsOnGround 
                || collisionInfo.XCorrection != 0 
                || collisionInfo.YCorrection != 0)
            {
                playerController.OnPlatformCollision(collisionInfo);
            }            
        }

        private CollisionInfo GetPlayerCollisionInfo(PlayerController playerController)
        {
            var playerBounds = playerController.WorldSprite.Bounds;
            var platformBounds = WorldSprite.Bounds;

            //check on top
            if (playerController.Motion.YSpeed >= 0
                && playerBounds.Bottom == platformBounds.Top
                && playerBounds.Right >= platformBounds.Left
                && playerBounds.Left <= platformBounds.Right)
            {
                int xMove = 0;
                if (_movedLeft.Value)
                    xMove = -1;
                else if (_movedRight.Value)
                    xMove = 1;

                playerController.WorldSprite.X += xMove;
                return new CollisionInfo { IsOnGround = true };
            }

            if(!playerBounds.Intersects(platformBounds))
            {
                return new CollisionInfo();
            }


            return new CollisionInfo();
        }
    }
}
