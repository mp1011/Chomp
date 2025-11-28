using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class ChompEnemyBulletController : BossBulletController
    {
        private WorldScroller _scroller;
        private GameByte _angle;

        public ChompEnemyBulletController(
            ChompGameModule gameModule,
            SystemMemoryBuilder memoryBuilder,
            SpriteType spriteType = SpriteType.BossBullet) : base(gameModule, memoryBuilder, false)
        {
            _scroller = gameModule.WorldScroller;
            _angle = memoryBuilder.AddByte();
        }

        public int Angle
        {
            get => _angle.Value * 2;
            set
            {
                _angle.Value = (byte)(value.NMod(360) / 2);                
            }
        }

        protected override void UpdateActive()
        {
            var angle = GameMathHelper.PointFromAngle(Angle, 40);
            AcceleratedMotion.SetXSpeed(angle.X);
            AcceleratedMotion.SetYSpeed(angle.Y);
            AcceleratedMotion.Apply(WorldSprite);

            if(_levelTimer.IsMod(8))
            {
                Angle -= 16;
            }

            if (_state.Value < 20 && _levelTimer.IsMod(8))
            {
                _state.Value++;
                if (_state.Value == 20)
                    Destroy();
            }
        }
    }
}
