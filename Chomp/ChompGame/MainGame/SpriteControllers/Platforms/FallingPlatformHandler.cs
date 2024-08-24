using ChompGame.Data;
using ChompGame.MainGame.SceneModels.SceneParts;
using ChompGame.MainGame.SpriteControllers.Base;

namespace ChompGame.MainGame.SpriteControllers.Platforms
{
    class FallingPlatformHandler : IPlatformHandler
    {
        private PlatformController _platformController;
        private IMotionController _motionController;
        private GameBit _movedDown;

        private GameByte _startPosition;

        private WorldSprite WorldSprite => _platformController.WorldSprite;

        public bool IsPlatformSolid => true;


        public FallingPlatformHandler(PlatformController controller, IMotionController motionController)
        {
            _platformController = controller;
            _motionController = motionController;
        }

        public void InitMemory(SystemMemory memory, int address)
        {
            _movedDown = new GameBit(address, Bit.Bit6, memory);
            _startPosition = new GameByte(address+1, memory);
        }

        public void UpdateActive(GameByte levelTimer)
        {
            if (_platformController.IsPlayerOnPlatform && _platformController.Motion.YSpeed == 0)
                _platformController.Motion.YSpeed = 1;

            if (_platformController.Motion.YSpeed > 0)
                _platformController.Motion.YSpeed++;
 
            if (WorldSprite.Y > _startPosition + 60)
                _platformController.WorldSprite.Destroy();

            int startY = WorldSprite.Y;
            _motionController.Update();
            _movedDown.Value = WorldSprite.Y > startY;
        }

        public void SetInitialPosition(int spawnX, int spawnY, PlatformDistance length)
        {
            _startPosition.Value = (byte)spawnY;
        }

        public void OnSpriteCreated()
        {
            if (_platformController.PlatformType == PlatformType.UpDown)
                WorldSprite.Y = _startPosition.Value;
            else
                WorldSprite.X = _startPosition.Value;

            _platformController.Motion.YSpeed = 0;
        }

        public void BeforeGetPlayerCollisionInfo(PlayerController playerController)
        {
            if (!_platformController.IsPlayerOnPlatform)
                return;

            int yMove = _movedDown.Value ? 1 : 0;
            playerController.WorldSprite.Y += yMove;
            playerController.WorldSprite.UpdateSprite();
            _platformController.WorldScroller.Update();
            
        }
    }
}
