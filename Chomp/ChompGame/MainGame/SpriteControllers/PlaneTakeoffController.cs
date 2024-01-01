using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteControllers.MotionControllers;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class PlaneTakeoffController : ActorController, ICollidableSpriteController
    {
        public const int HoverSpeed = 8;
        public const int TakeoffSpeed = 64;
        public const int TakeoffAccel = 8;
        public const int HoverAccel = 1;

        private PlayerController _player;
        private MaskedByte _yPos;
        private GameBit _takeOff;
        private ActorMotionController _motionController;
    
        public PlaneTakeoffController(
            ChompGameModule gameModule, 
            SystemMemoryBuilder memoryBuilder,
            PlayerController player) 
            : base(SpriteType.Plane, gameModule, memoryBuilder, SpriteTileIndex.Plane)
        {
            _player = player;
            _yPos = new MaskedByte(memoryBuilder.CurrentAddress, Bit.Right7, memoryBuilder.Memory);
            _takeOff = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit7, memoryBuilder.Memory);
            memoryBuilder.AddByte();
            _motionController = new ActorMotionController(gameModule, memoryBuilder, SpriteType.Plane, WorldSprite);
        }

        protected override void BeforeInitializeSprite()
        {
            _yPos.Value = 0;
            _takeOff.Value = false;
        }

        public bool HandleBombCollision(WorldSprite player) => false;
        public CollisionResult HandlePlayerCollision(WorldSprite player)
        {
            if(_player.Motion.YSpeed > 0)
            {
                _takeOff.Value = true;
                _player.OnPlaneEnter();
            }

            return CollisionResult.None;
        }

        protected override void UpdateActive()
        {
            if(_yPos.Value == 0)
            {
                _yPos.Value = (byte)WorldSprite.Y;
                _motionController.Motion.SetXSpeed(0);
                _motionController.Motion.SetYSpeed(0);
            }

            if (!_takeOff.Value
                    && _player.WorldSprite.X > WorldSprite.X + 8)
            {
                _player.WorldSprite.X = WorldSprite.X + 8;
            }

            if (_takeOff.Value)
            {
                _motionController.Motion.TargetYSpeed = -HoverSpeed;
                _motionController.Motion.XAcceleration = TakeoffAccel;
                _motionController.Motion.TargetXSpeed = TakeoffSpeed;
            }
            else if (_levelTimer.Value.IsMod(12))
            {               
                if (WorldSprite.Y < _yPos)
                {
                    _motionController.Motion.TargetYSpeed = HoverSpeed;
                    _motionController.Motion.YAcceleration = HoverAccel;
                }
                else
                {
                    _motionController.Motion.TargetYSpeed = -HoverSpeed;
                    _motionController.Motion.YAcceleration = HoverAccel;
                }
            }

            _motionController.Update();
            if (_takeOff.Value)
            {
                _player.WorldSprite.X = WorldSprite.X + 2;
                _player.WorldSprite.Y = WorldSprite.Y - 4;
                _player.WorldSprite.UpdateSprite();
            }
        }
        public bool CollidesWithPlayer(PlayerController player) => player.CollidesWith(WorldSprite);
    }
}
