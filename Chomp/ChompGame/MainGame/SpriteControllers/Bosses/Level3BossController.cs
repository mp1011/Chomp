using ChompGame.Data;
using ChompGame.Data.Memory;

namespace ChompGame.MainGame.SpriteControllers.Bosses
{
    class Level3BossController : LevelBossController
    {
        public Level3BossController(ChompGameModule gameModule, WorldSprite player, EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers, SystemMemoryBuilder memoryBuilder) : base(gameModule, player, bulletControllers, memoryBuilder)
        {
        }

        protected override int BossHP => 5;

        protected override string BossTiles { get; } =
            @"1BBBBBBBBBBBBBBBBBBBBBBBB
              BBBBBBBBBBBBBBBBBBBBBBBBB
              BBBBBBBBBBBBBBBBBBBBBBBBB
              BBBBBBBBBBBBBBBBBBBBBBBBB
              BBBBBBBBBBBBBBBBBBBBBBBBB";

        protected override string BlankBossTiles => "0";

        protected override void UpdateActive()
        {
            if(_stateTimer.Value == 0)
            {
                _stateTimer.Value = 1;
                SetBossTiles();
                WorldSprite.X = 10;
                WorldSprite.Y = 80;
            }

            _position.X = (byte)(WorldSprite.X - 8 - _tileModule.Scroll.X);
            _position.Y = (byte)(WorldSprite.Y - 66);
        }

        protected override void UpdatePartPositions()
        {

        }
    }
}
