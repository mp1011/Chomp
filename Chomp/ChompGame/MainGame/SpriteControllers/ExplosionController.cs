using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteControllers.MotionControllers;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class ExplosionController : ActorController
    {
        private GameByte _timer;
        private ActorMotionController _motionController;

        public ExplosionController(ChompGameModule gameModule, SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Explosion, gameModule, memoryBuilder, tileIndex: SpriteTileIndex.Explosion)
        {
            _timer = memoryBuilder.AddByte();
            _motionController = new ActorMotionController(gameModule, memoryBuilder, SpriteType.Explosion, WorldSprite);
            Palette = SpritePalette.Fire;
        }

        protected override void UpdateActive()
        {
            _motionController.Update();
            _timer.Value++;
            if(_timer.Value >= 16)
            {
                Destroy();
                return;
            }
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _timer.Value = 0;
        }

        public void SetMotion(int xMod, int yMod)
        {
            var motion = _motionController.Motion;

            if (yMod == 0)
                motion.YSpeed = -_motionController.JumpSpeed;
            else
                motion.YSpeed = 0;

            if (xMod == 0)
                motion.SetXSpeed(-_motionController.WalkSpeed);
            else
                motion.SetXSpeed(_motionController.WalkSpeed);


            motion.TargetYSpeed = _motionController.FallSpeed;
            motion.YAcceleration = _motionController.GravityAccel;
        }

        protected override void UpdateDying() { }
    }
}
