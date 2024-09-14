using ChompGame.Data;
using ChompGame.MainGame.SceneModels.SceneParts;
using ChompGame.MainGame.SpriteControllers.Base;

namespace ChompGame.MainGame.SpriteControllers.Platforms
{
    class MovingPlatformHandler : IPlatformHandler
    {
        private PlatformController _platformController;
        private IMotionController _motionController; 

        private GameBit _direction;
        private GameBit _movedBack;
        private GameBit _movedForward;
        private TwoBitEnum<PlatformDistance> _distance;
        private GameByte _startPosition;

        private WorldSprite WorldSprite => _platformController.WorldSprite;

        public bool IsPlatformSolid => true;


        public MovingPlatformHandler(PlatformController controller, IMotionController motionController)
        {
            _platformController = controller;
            _motionController = motionController;
        }

        public void InitMemory(SystemMemory memory, int address)
        {
            //3 bits taken, 13 bits available
            _distance = new TwoBitEnum<PlatformDistance>(memory, address, 3);
            _direction = new GameBit(address, Bit.Bit5, memory);
            _movedBack = new GameBit(address, Bit.Bit6, memory);
            _movedForward = new GameBit(address, Bit.Bit7, memory);

            _startPosition = new GameByte(address + 1, memory);
        }

        public void UpdateActive(GameByte levelTimer)
        {
            if (_platformController.PlatformType == PlatformType.LeftRight)
                Update_LeftRight();
            else if (_platformController.PlatformType == PlatformType.UpDown)
                Update_UpDown();
        }

        private void Update_LeftRight()
        {
            int startX = WorldSprite.X;
            _motionController.Update();

            _movedBack.Value = WorldSprite.X < startX;
            _movedForward.Value = WorldSprite.X > startX;

            if (WorldSprite.X <= _startPosition.Value)
            {
                _direction.Value = true;
                _motionController.Motion.XSpeed = 8;
            }
            else if (WorldSprite.X > _startPosition.Value + GetTravelDistance())
            {
                _direction.Value = true;
                _motionController.Motion.XSpeed = -8;
            }
        }

        private void Update_UpDown()
        {
            int startY = WorldSprite.Y;
            _motionController.Update();

            // GameDebug.DebugLog($"Platform Y {startY} - {WorldSprite.Y}", DebugLogFlags.Misc);
            _movedBack.Value = WorldSprite.Y < startY;
            _movedForward.Value = WorldSprite.Y > startY;

            if (WorldSprite.Y <= _startPosition.Value)
            {
                _direction.Value = true;
                _motionController.Motion.YSpeed = 8;
            }
            else if (WorldSprite.Y > _startPosition.Value + GetTravelDistance())
            {
                _direction.Value = true;
                _motionController.Motion.YSpeed = -8;
            }
        }
        private int GetTravelDistance()
        {
            return _distance.Value switch {
                PlatformDistance.Len16 => 16,
                PlatformDistance.Len24 => 24,
                PlatformDistance.Len32 => 32,
                PlatformDistance.Len48 => 48,
                _ => 0
            };
        }

        public void SetInitialPosition(int spawnX, int spawnY, PlatformDistance length)
        {
            if (_platformController.PlatformType == PlatformType.UpDown)
                _startPosition.Value = (byte)spawnY;
            else
                _startPosition.Value = (byte)spawnX;
            _distance.Value = length;
        }

        public void BeforeGetPlayerCollisionInfo(PlayerController playerController)
        {
            if (!_platformController.IsPlayerOnPlatform)
                return;

            if (_platformController.PlatformType == PlatformType.LeftRight)
            {
                int xMove = 0;
                if (_movedBack.Value)
                    xMove = -1;
                else if (_movedForward.Value)
                    xMove = 1;

                playerController.WorldSprite.X += xMove;
                playerController.WorldSprite.UpdateSprite();
                _platformController.WorldScroller.Update();
            }
            else if (_platformController.PlatformType == PlatformType.UpDown)
            {
                int yMove = 0;
                if (_movedBack.Value)
                    yMove = -1;
                else if (_movedForward.Value)
                    yMove = 1;

                playerController.WorldSprite.Y += yMove;
                playerController.WorldSprite.UpdateSprite();
                _platformController.WorldScroller.Update();
            }
        }

        public void OnSpriteCreated()
        {
            if (_platformController.PlatformType == PlatformType.UpDown)
                WorldSprite.Y = _startPosition.Value;
            else
                WorldSprite.X = _startPosition.Value;
        }
    }
}
