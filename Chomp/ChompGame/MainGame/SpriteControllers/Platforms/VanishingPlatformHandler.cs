using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.MainGame.SceneModels.SceneParts;
using ChompGame.MainGame.SpriteControllers.Base;

namespace ChompGame.MainGame.SpriteControllers.Platforms
{
    class VanishingPlatformHandler : IPlatformHandler
    {
        private GameByte _vanishTimer;
        private TwoBitEnum<PlatformDistance> _onOffTime;

        private PlatformController _platformController;
        private IMotionController _motionController;
       
        private WorldSprite WorldSprite => _platformController.WorldSprite;


        public VanishingPlatformHandler(PlatformController controller, IMotionController motionController)
        {
            _platformController = controller;
            _motionController = motionController;
        }

        public void BeforeGetPlayerCollisionInfo(PlayerController playerController)
        { }

        public void InitMemory(SystemMemory memory, int address)
        {
            //3 bits taken, 13 bits available
            _onOffTime = new TwoBitEnum<PlatformDistance>(memory, address, 3);
            _vanishTimer = new GameByte(address + 1, memory);
        }

        public void OnSpriteCreated()
        {

        }

        public void SetInitialPosition(int spawnX, int spawnY, PlatformDistance length)
        {
            _onOffTime.Value = length;
        }

        private int OnPeriod => _onOffTime.Value switch
        {
            PlatformDistance.Len16 => 32,
            PlatformDistance.Len24 => 48,
            PlatformDistance.Len32 => 64,
            _ => 96
        };

        private int OffPeriod => _onOffTime.Value switch {
            PlatformDistance.Len16 => 16,
            PlatformDistance.Len24 => 24,
            PlatformDistance.Len32 => 32,
            _ => 48
        };

        private int BlinkDuration = 16;

        public bool IsPlatformSolid => _vanishTimer.Value <= OnPeriod;


        public void UpdateActive(GameByte levelTimer)
        {
            if (_vanishTimer.Value > OnPeriod-BlinkDuration && _vanishTimer.Value < OnPeriod)
                WorldSprite.Visible = !WorldSprite.Visible;
            else if (_vanishTimer.Value <= OnPeriod - BlinkDuration)
                WorldSprite.Visible = true;
            else
                WorldSprite.Visible = false;

            if (levelTimer.Value.IsMod(4))
                _vanishTimer.Value++;

            if (_vanishTimer.Value == OnPeriod + OffPeriod)
                _vanishTimer.Value = 0;
        }
    }
}
