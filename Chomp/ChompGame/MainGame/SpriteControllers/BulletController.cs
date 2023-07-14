using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteControllers.MotionControllers;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class BulletController : ActorController, ICollidesWithPlayer, IEnemyOrBulletSpriteController
    {
        private GameByte _state;
        private IMotionController _motionController;

        public BulletController(
            ChompGameModule gameModule,
            SystemMemoryBuilder memoryBuilder, 
            SpriteType spriteType) : base(spriteType, gameModule, memoryBuilder, SpriteTileIndex.Extra1)
        {
            _motionController = new SimpleMotionController(memoryBuilder, WorldSprite, 
                new SpriteDefinition(spriteType, memoryBuilder.Memory));
            _state = memoryBuilder.AddByte();
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _state.Value = 0;
        }

        protected override bool DestroyWhenOutOfBounds => true;

        protected override void UpdateActive() 
        {
            if (WorldSprite.Status != WorldSpriteStatus.Active)
                return;

            var sprite = GetSprite();

            if (_state.Value == 0)
            {
                _motionController.Motion.YSpeed = 0;

                if (sprite.FlipX)
                    _motionController.Motion.XSpeed = -_motionController.Speed;
                else
                    _motionController.Motion.XSpeed = _motionController.Speed;
            }

            _motionController.Update();

            if (_levelTimer.Value.IsMod(4))
                _state.Value++;

            if (_state.Value == 40 || _state.Value == 60)
                Destroy();
        }

        protected override void UpdateDying()
        {
            var sprite = GetSprite();
            
            if (_levelTimer.Value.IsMod(4))
                _state.Value++;

            if (_state.Value == 40 || _state.Value == 60)
                Destroy();
            else if (_state.Value > 40)
            {
                sprite.Tile = (byte)(6 + (_levelTimer.Value % 2));
            }
        }



        public void HandlePlayerCollision(WorldSprite player)
        {
            if (_state.Value >= 40)
                return;

            WorldSprite.Status = WorldSpriteStatus.Dying;
            _state.Value = 41;
            _motionController.Motion.XSpeed = 0;
            _motionController.Motion.YSpeed = 0;
        }

        public bool HandleBombCollision(WorldSprite player) => false;
    }
}
