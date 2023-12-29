using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SpriteControllers.Bosses
{
    class Level2BossController : LevelBossController
    {
        private BossPart _eye2, _eye3, _eye4, _arm1, _arm2;
        private Tentacle _tentacle1, _tentacle2;

        class Tentacle
        {
            private const int NumSections = 6;
            private ChompTail _arm;
            private GameByte _curl;

            public byte Curl
            {
                get => _curl.Value;
                set => _curl.Value = value;
            }

            public Tentacle(SystemMemoryBuilder memoryBuilder, 
                SpritesModule spritesModule, 
                SpriteTileTable spriteTileTable)
            {
                _arm = new ChompTail(memoryBuilder, NumSections, spritesModule, spriteTileTable);
                _curl = memoryBuilder.AddByte();
            }

            public void Initialize()
            {
                _arm.CreateTail(SpriteTileIndex.Enemy2);
            }

            public void Update(Sprite anchor, bool isLeft)
            {
                var target = anchor;
                for (int i = 0; i < NumSections; i++)
                {
                    target = UpdateSection(target, _arm.GetSprite(i), i, isLeft);
                }
            }

            private Sprite UpdateSection(Sprite anchor, Sprite section, int sectionNumber, bool isLeft)
            {
                var mod = isLeft ? 8 : -8;

                var offset = new Point(0, 4);
                offset = offset.RotateDeg((_curl.Value-128) + sectionNumber* mod);
                section.X = (byte)(anchor.X + offset.X);
                section.Y = (byte)(anchor.Y + offset.Y);
                return section;
            }
        }

        public Level2BossController(ChompGameModule gameModule, WorldSprite player, EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers, SystemMemoryBuilder memoryBuilder) 
            : base(gameModule, player, bulletControllers, memoryBuilder)
        {
            _eye2 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _eye3 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _eye4 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _arm1 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossArm, memoryBuilder.Memory));
            _arm2 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossArm, memoryBuilder.Memory));

            _tentacle1 = new Tentacle(memoryBuilder, _spritesModule, _spriteTileTable);
            _tentacle2 = new Tentacle(memoryBuilder, _spritesModule, _spriteTileTable);
        }

        protected override string BossTiles { get; } =
            @"1335004332
              9BBB33BBBA
              9BBBBBBBBA
              0CBBBBBBD0
              0CBBBBBBD0
              009BBBBA00
              000C88D000";

        protected override string BlankBossTiles { get; } =
            @"0000000000
              0000000000
              0000000000
              0000000000
              0000000000
              0000000000
              0000000000
              0000000000";
        protected override void UpdateActive()
        {
            if(_stateTimer.Value == 0)
            {
                SetupBoss();
            }

            _tentacle1.Curl++;
            _tentacle2.Curl--;


            UpdatePartPositions();
        }

        private void SetupBoss()
        {
            WorldSprite.X = 40;
            WorldSprite.Y = _player.Y - 32;
            SetBossBackgroundEnd(2);
            SetBossTiles();
            _stateTimer.Value = 1;

            var e2 =_eye2.PrepareSprite(SpriteTileIndex.Enemy1);
            e2.Tile2Offset = 1;
            e2.FlipX = true;
            _eye2.XPosition = -22;
            _eye2.YPosition = 0;

            var e3 = _eye3.PrepareSprite(SpriteTileIndex.Enemy1);
            e3.Tile2Offset = 1;
            e3.FlipX = true;
            _eye3.XPosition = -18;
            _eye3.YPosition = 8;

            var e4 = _eye4.PrepareSprite(SpriteTileIndex.Enemy1);
            e4.Tile2Offset = 1;
            e4.FlipX = false;
            _eye4.XPosition = -4;
            _eye4.YPosition = 8;

            var a1 = _arm1.PrepareSprite(SpriteTileIndex.Enemy2);
            a1.FlipX = false;
            _arm1.XPosition = -22;
            _arm1.YPosition = 20;

            var a2 = _arm2.PrepareSprite(SpriteTileIndex.Enemy2);
            a2.FlipX = false;
            _arm2.XPosition = 4;
            _arm2.YPosition = 20;

            _tentacle1.Initialize();
            _tentacle2.Initialize();


            _paletteModule.BgColor = 0;
        }

        protected override void UpdatePartPositions()
        {
            _position.X = (byte)(WorldSprite.X - 28 - _tileModule.Scroll.X);
            _position.Y = (byte)(WorldSprite.Y - 66);

            var bossSprite = GetSprite();
            _eye2.UpdatePosition(bossSprite);
            _eye3.UpdatePosition(bossSprite);
            _eye4.UpdatePosition(bossSprite);
            _arm1.UpdatePosition(bossSprite);
            _arm2.UpdatePosition(bossSprite);

            _tentacle1.Update(_arm1.Sprite, true);
            _tentacle2.Update(_arm2.Sprite, false);
        }
    }
}
