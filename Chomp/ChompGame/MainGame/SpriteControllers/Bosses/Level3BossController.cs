using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Graphics;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers.Bosses
{
    class Level3BossController : LevelBossController
    {
        private BossPart _leftEye, _rightEye, _leftJaw, _rightJaw;

        public Level3BossController(ChompGameModule gameModule, WorldSprite player, EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers, SystemMemoryBuilder memoryBuilder) : base(gameModule, player, bulletControllers, memoryBuilder)
        {
            _leftEye = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _rightEye = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _leftJaw = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossJaw, memoryBuilder.Memory));
            _rightJaw = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossJaw, memoryBuilder.Memory));
        }

        protected override int BossHP => 5;

        protected override string BossTiles { get; } =
            @"BAAAAA9008AAAAAC
              6MMMJII98IIHMMM5
              00BAAIIIIIIAAAC0
              006MJIIIIII4M500
              00000GIIII400000
              000000DE12000000";


        protected override string BlankBossTiles => "0";

        protected override void UpdateActive()
        {
            if(_stateTimer.Value == 0)
            {
                _paletteModule.BgColor = ColorIndex.Black;
                _stateTimer.Value = 1;
                SetupBossParts();
                SetBossTiles();
                WorldSprite.X = 10;
                WorldSprite.Y = 80;
            }

            _position.X = (byte)(WorldSprite.X - 8 - _tileModule.Scroll.X);
            _position.Y = (byte)(WorldSprite.Y - 66);
            UpdatePartPositions();
        }

        private void SetupBossParts()
        {
            var leftEyeSprite = _leftEye.PrepareSprite(SpriteTileIndex.Enemy1);
            leftEyeSprite.Tile2Offset = 1;
            leftEyeSprite.FlipX = true;
            _leftEye.XOffset = 14;
            _leftEye.YOffset = 8;

            var rightEyeSprite = _rightEye.PrepareSprite(SpriteTileIndex.Enemy1);
            rightEyeSprite.Tile2Offset = 1;
            rightEyeSprite.FlipX = false;
            _rightEye.XOffset = 26;
            _rightEye.YOffset = 8;

            var leftJawSprite = _leftJaw.PrepareSprite(SpriteTileIndex.Enemy2);
            leftJawSprite.Tile2Offset = 0;
            leftJawSprite.FlipX = false;
            _leftJaw.XOffset = 16;
            _leftJaw.YOffset = 20;

            var rightJawSprite = _rightJaw.PrepareSprite(SpriteTileIndex.Enemy2);
            rightJawSprite.Tile2Offset = 0;
            rightJawSprite.FlipX = true;
            _rightJaw.XOffset = 26;
            _rightJaw.YOffset = 20;

        }


        protected override void UpdatePartPositions()
        {
            WorldSprite.Visible = false;
            _leftEye.UpdatePosition(WorldSprite);
            _rightEye.UpdatePosition(WorldSprite);
            _leftJaw.UpdatePosition(WorldSprite);
            _rightJaw.UpdatePosition(WorldSprite);
        }
    }
}
