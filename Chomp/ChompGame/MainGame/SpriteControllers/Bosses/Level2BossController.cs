using ChompGame.Data;
using ChompGame.Data.Memory;

namespace ChompGame.MainGame.SpriteControllers.Bosses
{
    class Level2BossController : LevelBossController
    {
        public Level2BossController(ChompGameModule gameModule, WorldSprite player, EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers, SystemMemoryBuilder memoryBuilder) : base(gameModule, player, bulletControllers, memoryBuilder)
        {
        }

        protected override string BossTiles { get; } =
            @"11111
              99999
              77777
              AAAAA";

        protected override string BlankBossTiles { get; } =
            @"00000
              00000
              00000
              00000";
        protected override void UpdateActive()
        {
            if(_stateTimer.Value == 0)
            {
                WorldSprite.X = 16;
                WorldSprite.Y = _player.Y - 16;
                SetBossBackgroundEnd(2);
                SetBossTiles();
                _stateTimer.Value = 1;
            }

            UpdatePartPositions();
        }

        protected override void UpdatePartPositions()
        {
            _position.X = (byte)(WorldSprite.X - 8 - _tileModule.Scroll.X);
            _position.Y = (byte)(WorldSprite.Y - 66);
        }
    }
}
