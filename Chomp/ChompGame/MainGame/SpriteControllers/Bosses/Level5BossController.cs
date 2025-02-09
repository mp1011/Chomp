using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Graphics;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers.Bosses
{
    class Level5BossController : LevelBossController
    {
        private NibbleEnum<Phase> _phase;
        private BossPart _eye1, _eye2, _horn1, _horn2, _horn3, _horn4;

        protected override bool AlwaysActive => true;
        protected override bool DestroyWhenFarOutOfBounds => false;
        protected override bool DestroyWhenOutOfBounds => false;

        public Level5BossController(ChompGameModule gameModule, WorldSprite player, EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers, SystemMemoryBuilder memoryBuilder) : base(gameModule, player, bulletControllers, memoryBuilder)
        {
            _phase = new NibbleEnum<Phase>(new LowNibble(memoryBuilder));
            memoryBuilder.AddByte();

            _eye1 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _eye2 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _horn1 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossHorn, memoryBuilder.Memory));
            _horn2 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossHorn, memoryBuilder.Memory));
            _horn3 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossHorn, memoryBuilder.Memory));
            _horn4 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossHorn, memoryBuilder.Memory));
        }

        private enum Phase : byte
        {
            BeforeBoss,
            Init,           
        }

        protected override int BossHP => 4;

        protected override string BossTiles { get; } =
            @"0BAAC000
              8IIII900
              GIIIIH00
              8IIII900
              GIIIIH00
              0JIIIH00
              006M5000";
           



        protected override void UpdatePartPositions()
        {
            _eye1.UpdatePosition(WorldSprite);
            _eye2.UpdatePosition(WorldSprite);
            _horn1.UpdatePosition(WorldSprite);
            _horn2.UpdatePosition(WorldSprite);
            _horn3.UpdatePosition(WorldSprite);
            _horn4.UpdatePosition(WorldSprite);
        }

        protected override string BlankBossTiles => "0";

        protected override void BeforeInitializeSprite()
        {
            SetPhase(Phase.BeforeBoss);
            base.BeforeInitializeSprite();
        }

        private void SetPhase(Phase p)
        {
            GameDebug.Watch1 = new DebugWatch("Boss X", () => WorldSprite.X);
            GameDebug.Watch2 = new DebugWatch("Boss Y", () => WorldSprite.Y);
            GameDebug.Watch3 = new DebugWatch("State Timer", () => _stateTimer.Value);

            _phase.Value = p;
            _stateTimer.Value = 0;

            if(p == Phase.BeforeBoss)
            {
                _gameModule.BossBackgroundHandler.ShowCoins = false;
                _paletteModule.BgColor = ColorIndex.Black;
                HideBoss();
            }
            else if(p == Phase.Init)
            {               

                SetBossTiles();              
                SetupBossParts();
            }
        }

        private void SetupBossParts()
        {
            var eye1Sprite = _eye1.PrepareSprite(SpriteTileIndex.Enemy1);
            eye1Sprite.Tile2Offset = 1;
            eye1Sprite.FlipX = true;
            _eye1.XOffset = 0;
            _eye1.YOffset = 0;
            eye1Sprite.Visible = true;

            var eye2Sprite = _eye2.PrepareSprite(SpriteTileIndex.Enemy1);
            eye2Sprite.Tile2Offset = 1;
            eye2Sprite.FlipX = false;
            _eye2.XOffset = 10;
            _eye2.YOffset = 0;
            eye2Sprite.Visible = true;

            var horn1Sprite = _horn1.PrepareSprite(SpriteTileIndex.Enemy2);
            horn1Sprite.Tile2Offset = 1;
            horn1Sprite.FlipX = false;
            _horn1.XOffset = -4;
            _horn1.YOffset = -18;
            horn1Sprite.Priority = false;
            horn1Sprite.Visible = true;

            var horn2Sprite = _horn2.PrepareSprite(SpriteTileIndex.Enemy2);
            horn2Sprite.Tile2Offset = 1;
            horn2Sprite.FlipX = true;
            _horn2.XOffset = 12;
            _horn2.YOffset = -18;
            horn2Sprite.Priority = false;
            horn2Sprite.Visible = true;

            var horn3Sprite = _horn3.PrepareSprite(SpriteTileIndex.Enemy2);
            horn3Sprite.Tile2Offset = 1;
            horn3Sprite.FlipX = false;
            horn3Sprite.FlipY = true;
            _horn3.XOffset = -2;
            _horn3.YOffset =10;
            horn3Sprite.Priority = false;
            horn3Sprite.Visible = true;

            var horn4Sprite = _horn4.PrepareSprite(SpriteTileIndex.Enemy2);
            horn4Sprite.Tile2Offset = 1;
            horn4Sprite.FlipX = true;
            horn4Sprite.FlipY = true;
            _horn4.XOffset = 10;
            _horn4.YOffset = 10;
            horn4Sprite.Priority = false;
            horn4Sprite.Visible = true;
        }

        protected override void UpdateActive()
        {
            if (_phase.Value == Phase.BeforeBoss)
                SetPhase(Phase.Init);

            if (_phase.Value == Phase.Init)
            {
                WorldSprite.X = 16;
                WorldSprite.Y = 220;
                _position.X = (byte)(WorldSprite.X + 123 - _tileModule.Scroll.X);
                _position.Y = (byte)(WorldSprite.Y - 77);

                FadeIn();
            }

            if (_phase.Value >= Phase.Init)
                UpdatePartPositions();

        }

        private void FadeIn()
        {
            Theme theme = new Theme(_graphicsModule.GameSystem.Memory, ThemeType.CityBoss);
            var targetSpritePalette = _paletteModule.GetPalette(theme.Enemy1);
            var targetBossPalette = _paletteModule.GetPalette(theme.Background2);

            var spritePalette = _graphicsModule.GetSpritePalette(SpritePalette.Enemy1);
            _paletteModule.FadeColor(spritePalette, targetSpritePalette, 1);
            _paletteModule.FadeColor(spritePalette, targetSpritePalette, 2);
            _paletteModule.FadeColor(spritePalette, targetSpritePalette, 3);

            var bossPalette = _paletteModule.BgPalette2;
            _paletteModule.FadeColor(bossPalette, targetBossPalette, 1);
            _paletteModule.FadeColor(bossPalette, targetBossPalette, 2);
            _paletteModule.FadeColor(bossPalette, targetBossPalette, 3);
        }

        protected override void UpdateDying()
        {
            
        }

        private void CreateExplosions()
        {
            var x = _rng.Generate(5);
            var y = _rng.Generate(4);
            CreateExplosion((WorldSprite.X - 8) + x, WorldSprite.Y + y);
        }

        public override BombCollisionResponse HandleBombCollision(WorldSprite player)
        {
            _audioService.PlaySound(ChompAudioService.Sound.Lightning);           
            _hitPoints.Value--;
            if (_hitPoints.Value == 0)
                WorldSprite.Status = WorldSpriteStatus.Dying;
           // else
             //   SetPhase(Phase.Hurt);

            return BombCollisionResponse.Destroy;
        }


        public override bool CollidesWithPlayer(PlayerController player)
        {
            if (_phase.Value <= Phase.Init)
                return false;

            return false; //todo
        }
    }
}
