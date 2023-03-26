using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class ExplosionController : ActorController
    {
        public ExplosionController(ChompGameModule gameModule, SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Explosion, gameModule, memoryBuilder)
        {
        }

        protected override void UpdateActive()
        {
            _movingSpriteController.Update();
            _state.Value++;
            if(_state.Value >= 16)
            {
                Destroy();
                return;
            }
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _state.Value = 0;
        }

        public void SetMotion(int xMod, int yMod)
        {
            if (yMod == 0)
                Motion.YSpeed = -_movingSpriteController.JumpSpeed;
            else
                Motion.YSpeed = 0;

            if (xMod == 0)
                Motion.SetXSpeed(-_movingSpriteController.WalkSpeed);
            else
                Motion.SetXSpeed(_movingSpriteController.WalkSpeed);


            Motion.TargetYSpeed = _movingSpriteController.FallSpeed;
            Motion.YAcceleration = _movingSpriteController.GravityAccel;
        }
    }
}
