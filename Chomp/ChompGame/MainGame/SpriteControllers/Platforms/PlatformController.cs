using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SceneModels.SceneParts;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteControllers.MotionControllers;
using ChompGame.MainGame.SpriteControllers.Platforms;
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

    interface IPlatformHandler
    {
        void InitMemory(SystemMemory memory, int address);
        void OnSpriteCreated();
        void UpdateActive(GameByte levelTimer);
        void BeforeGetPlayerCollisionInfo(PlayerController playerController);
        void SetInitialPosition(int spawnX, int spawnY, PlatformDistance length);
        bool IsPlatformSolid { get; }
    }

    class PlatformController : ActorController
    {
        private IPlatformHandler _platformHandler;
        private IMotionController _motionController;
        private GameBit _playerOnPlatform;
        private TwoBitEnum<PlatformType> _platformType;

        public WorldScroller WorldScroller { get; }
        public override IMotion Motion => _motionController.Motion;

        public PlatformType PlatformType
        {
            get => _platformType.Value;
        }

        public bool IsPlayerOnPlatform => _playerOnPlatform.Value;

        protected override bool DestroyWhenFarOutOfBounds => false;
        protected override bool AlwaysActive => _platformType.Value != PlatformType.Vanishing;

        public PlatformController(ChompGameModule gameModule, SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Platform, gameModule, memoryBuilder, SpriteTileIndex.Platform)
        {
            int address = memoryBuilder.CurrentAddress;
            memoryBuilder.AddByte();
            memoryBuilder.AddByte();
            _playerOnPlatform = new GameBit(address, Bit.Bit0, memoryBuilder.Memory);
            _platformType = new TwoBitEnum<PlatformType>(memoryBuilder.Memory,
                    address,
                    1);
            
            _motionController = new SimpleMotionController(memoryBuilder, WorldSprite, 
                new SpriteDefinition(SpriteType.Platform, memoryBuilder.Memory));
            WorldScroller = gameModule.WorldScroller;
            Palette = SpritePalette.Platform;
        }

        public void Initialize(PlatformType platformType, int initX, int initY, PlatformDistance distance)
        {
            _platformType.Value = platformType;
            _platformHandler = platformType switch {
                PlatformType.LeftRight => new MovingPlatformHandler(this, _motionController),
                PlatformType.UpDown => new MovingPlatformHandler(this, _motionController),
                PlatformType.Vanishing => new VanishingPlatformHandler(this, _motionController),
                PlatformType.Falling => new FallingPlatformHandler(this, _motionController),
                _ => throw new System.NotImplementedException()
            };

            _platformHandler.InitMemory(_spritesModule.GameSystem.Memory, _playerOnPlatform.Address);
            _platformHandler.SetInitialPosition(initX, initY, distance);
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            GameDebug.DebugLog("Platform created", DebugLogFlags.SpriteSpawn);
            Motion.Stop();

            _platformHandler?.OnSpriteCreated();
        }

        protected override void UpdateActive()
        {
            _platformHandler.UpdateActive(_levelTimer);
        }

        public void CheckPlayerCollision(PlayerController playerController)
        {
            if (!_platformHandler.IsPlatformSolid)
                return;

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
            if (playerController.Motion.YSpeed < 0)
                return new CollisionInfo();

            _platformHandler.BeforeGetPlayerCollisionInfo(playerController);
          
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
                return new CollisionInfo { IsOnGround = true };
            }

            if (bottomYOverlap > 0 && playerController.Motion.YSpeed <= 0)
            {
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
