using ChompGame.Data.Memory;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class Boss6BulletController : BossBulletController
    {
        private WorldScroller _scroller;
       
        public Boss6BulletController(
            ChompGameModule gameModule,
            SystemMemoryBuilder memoryBuilder,
            SpriteType spriteType = SpriteType.BossBullet) : base(gameModule, memoryBuilder, true, spriteType)
        {
            _scroller = gameModule.WorldScroller;
        }


        protected override void UpdateActive() 
        {
            if (WorldSprite.Y > 160)
                Destroy();

            if (WorldSprite.X > _scroller.ViewPane.Right + 16)
                Destroy();

            base.UpdateActive();
        }
    }
}
