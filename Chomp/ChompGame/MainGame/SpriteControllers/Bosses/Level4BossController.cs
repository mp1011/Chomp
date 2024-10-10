using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Graphics;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers.Bosses
{
    class Level4BossController : LevelBossController
    {
        private NibbleEnum<Phase> _phase;

        protected override bool AlwaysActive => true;
        protected override bool DestroyWhenFarOutOfBounds => false;
        protected override bool DestroyWhenOutOfBounds => false;

        public Level4BossController(ChompGameModule gameModule, WorldSprite player, EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers, SystemMemoryBuilder memoryBuilder) : base(gameModule, player, bulletControllers, memoryBuilder)
        {
            _phase = new NibbleEnum<Phase>(new LowNibble(memoryBuilder));
            memoryBuilder.AddByte();
        }

        private enum Phase : byte
        {
            BeforeBoss,
            Init,
        }

        protected override int BossHP => 5;

        protected override string BossTiles { get; } =
            @"IIIIIIIIIIIIIIII
              IIIIIIIIIIIIIIII
              IIIIIIIIIIIIIIII
              IIIIIIIIIIIIIIII
              IIIIIIIIIIIIIIII
              IIIIIIIIIIIIIIII";


        protected override string BlankBossTiles => "0";

        protected override void BeforeInitializeSprite()
        {
            SetPhase(Phase.BeforeBoss);
            base.BeforeInitializeSprite();
        }

        private void SetPhase(Phase p)
        {
            _bossBackgroundHandler.ShowCoins = true;
            _phase.Value = p;
            _stateTimer.Value = 0;

            if(p == Phase.BeforeBoss)
            {
              //  SetupBossParts();
                _paletteModule.BgColor = ColorIndex.Black;
               // HideBoss();
            }
            else if(p == Phase.Init)
            {
                GameDebug.Watch1 = new DebugWatch("Boss X", () => WorldSprite.X);
                GameDebug.Watch2 = new DebugWatch("Boss Y", () => WorldSprite.Y);
                GameDebug.Watch3 = new DebugWatch("State Timer", () => _stateTimer.Value);

                SetBossTiles();
              
              //  SetupBossParts();
                WorldSprite.X = 20;
                WorldSprite.Y = 80;
                HideBoss();
            }
        }

        protected override void UpdateActive()
        {
            if(_phase.Value == Phase.BeforeBoss)
            {
                _musicModule.CurrentSong = MusicModule.SongName.None;
                if (_player.X >= 40)
                    SetPhase(Phase.Init);
                WorldSprite.X = 20;
                WorldSprite.Y = 80;
            }
            else if(_phase.Value == Phase.Init)
            {
                _musicModule.CurrentSong = MusicModule.SongName.Nemesis;
                FadeIn();
            }
           
            _motionController.Update();
            _position.X = (byte)(WorldSprite.X - 8 - _tileModule.Scroll.X);
            _position.Y = (byte)(WorldSprite.Y - 66);

            UpdatePartPositions();
        }

        protected override void UpdateDying()
        {
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
        private void CreateExplosions()
        {
            var x = _rng.Generate(5);
            var y = _rng.Generate(4);
            CreateExplosion(WorldSprite.X + 8 + x, WorldSprite.Y + y);
        }

        public override bool CollidesWithBomb(WorldSprite bomb)
        {
            return false;
        }

        public override BombCollisionResponse HandleBombCollision(WorldSprite player)
        {
            _audioService.PlaySound(ChompAudioService.Sound.Lightning);           
            _hitPoints.Value--;
            if (_hitPoints.Value == 0)
                WorldSprite.Status = WorldSpriteStatus.Dying;
            return BombCollisionResponse.Destroy;
        }

        protected override void UpdatePartPositions()
        {

        }
    }
}
