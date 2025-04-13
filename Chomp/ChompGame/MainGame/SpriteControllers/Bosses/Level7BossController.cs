﻿using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Graphics;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers.Bosses
{
    class Level7BossController : LevelBossController
    {
        private NibbleEnum<Phase> _phase;
        private BossPart _eye1, _eye2;
      
        protected override bool AlwaysActive => true;
        protected override bool DestroyWhenFarOutOfBounds => false;
        protected override bool DestroyWhenOutOfBounds => false;


        public Level7BossController(ChompGameModule gameModule, WorldSprite player, EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers, SystemMemoryBuilder memoryBuilder) : base(gameModule, player, bulletControllers, memoryBuilder)
        {
            _phase = new NibbleEnum<Phase>(new LowNibble(memoryBuilder));           
            memoryBuilder.AddByte();
          
            _eye1 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _eye2 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
        }

        private enum Phase : byte
        {
            BeforeBoss,
            EyesAppear,
            EnemySpawn
        }

        protected override int BossHP => GameDebug.BossTest ? 1 : 5;

        protected override string BossTiles { get; } =
            @"0000";
           
        protected override void UpdatePartPositions()
        {

            _eye1.UpdatePosition(WorldSprite);
            _eye2.UpdatePosition(WorldSprite);
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

            if (p == Phase.BeforeBoss)
            {
                _gameModule.BossBackgroundHandler.ShowCoins = false;
                _paletteModule.BgColor = ColorIndex.Black;
                HideBoss();
            }
            else if (p == Phase.EyesAppear)
            {
                _musicModule.CurrentSong = GameSystem.MusicModule.SongName.FinalBossPart1;

                SetBossTiles();
                SetupBossParts();
                WorldSprite.Visible = false;

                _eye1.Sprite.Visible = true;
                _eye2.Sprite.Visible = true;
            }
            else if (p == Phase.EnemySpawn)
            {
                _eye1.Sprite.Visible = false;
                _eye2.Sprite.Visible = false;
                _gameModule.CollissionDetector.BossBgHandling = false;
                SetBackgroundForEnemySpawn();
            }
        }

        private void SetBackgroundForEnemySpawn()
        {
            _worldScroller.ModifyTiles((t, a) =>
            {
                t.ForEach((x, y, b) =>
                {
                    if (x <= 1 || x >= t.Width - 2)
                    {
                        t[x, y] = 14;
                        a[x / 2, y / 2] = 1;
                    }


                    if( x >= 4 && x <= 11 && (y == t.Height - 7 || y == t.Height - 8))
                    {
                        t[x, y] = 14;
                        a[x / 2, y / 2] = 1;
                    }

                    if (x >= 14 && x <= 17 && (y == t.Height - 3 || y == t.Height - 4))
                    {
                        t[x, y] = 14;
                        a[x / 2, y / 2] = 1;
                    }

                    if (x >= 20 && x <= 27 && (y == t.Height - 7 || y == t.Height - 8))
                    {
                        t[x, y] = 14;
                        a[x / 2, y / 2] = 1;
                    }

                });
            });
        }

        private void SetupBossParts()
        {
            var eye1Sprite = _eye1.PrepareSprite(SpriteTileIndex.Enemy1);
            eye1Sprite.Tile2Offset = 1;
            eye1Sprite.FlipX = true;
            eye1Sprite.FlipY = false;
            _eye1.XOffset = 0;
            _eye1.YOffset = 0;
            eye1Sprite.Visible = true;

            var eye2Sprite = _eye2.PrepareSprite(SpriteTileIndex.Enemy1);
            eye2Sprite.Tile2Offset = 1;
            eye2Sprite.FlipX = false;
            eye2Sprite.FlipY = false;
            _eye2.XOffset = 16;
            _eye2.YOffset = 0;
            eye2Sprite.Visible = true;
        }

        protected override void UpdateActive()
        {
            if(_phase.Value == Phase.BeforeBoss)
            {
                WorldSprite.Visible = false;
                if (_player.X > 64)
                    SetPhase(Phase.EyesAppear);
            }
            else if(_phase.Value == Phase.EyesAppear)
            {
                WorldSprite.X = (_player.X - 10).Clamp(22,86);
                WorldSprite.Y = 90;
                PositionBoss();

                if(_levelTimer.IsMod(32))
                {
                    _stateTimer.Value++;
                    if (_stateTimer.Value == 0)
                        SetPhase(Phase.EnemySpawn);

                    if (_stateTimer.Value < 8)
                        FadeIn(false);
                    else
                        FadeOut();
                }
            }
            else if(_phase.Value == Phase.EnemySpawn)
            {
                FadeIn(true);
            }
        }

        private void PositionBoss()
        {
            _position.X = (byte)(WorldSprite.X + 123 - _tileModule.Scroll.X);
            _position.Y = (byte)(WorldSprite.Y - 77);
            UpdatePartPositions();
        }

        private void RainBullet()
        {
            var bullet = _bulletControllers.TryAddNew();
            if (bullet == null)
                return;

            bullet.DestroyOnCollision = true;
            bullet.DestroyOnTimer = true;

            bullet.WorldSprite.X = _player.X + _rng.Generate(4) * 4;
            bullet.WorldSprite.Y = 64;
            _audioService.PlaySound(ChompAudioService.Sound.Fireball);


            bullet.AcceleratedMotion.YAcceleration = 3;
            bullet.AcceleratedMotion.TargetYSpeed = 80; 
        }

        private void FadeIn(bool includeFg)
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

            if(includeFg)
            {
                var targetFgPalette = _paletteModule.GetPalette(PaletteKey.FinalFg);
                var fgPalette = _graphicsModule.GetBackgroundPalette(BgPalette.Foreground);
                _paletteModule.FadeColor(fgPalette, targetFgPalette, 1);
                _paletteModule.FadeColor(fgPalette, targetFgPalette, 2);
                _paletteModule.FadeColor(fgPalette, targetFgPalette, 3);
            }
        }

        private void FadeOut()
        {
            Theme theme = new Theme(_graphicsModule.GameSystem.Memory, ThemeType.CityBoss);
            var spritePalette = _graphicsModule.GetSpritePalette(SpritePalette.Enemy1);
            _paletteModule.DarkenToBlack(spritePalette, 1);
            _paletteModule.DarkenToBlack(spritePalette, 2);
            _paletteModule.DarkenToBlack(spritePalette, 3);

            var bossPalette = _paletteModule.BgPalette2;
            _paletteModule.DarkenToBlack(bossPalette, 1);
            _paletteModule.DarkenToBlack(bossPalette, 2);
            _paletteModule.DarkenToBlack(bossPalette, 3);

            var fgPalette = _graphicsModule.GetBackgroundPalette(BgPalette.Foreground);
            _paletteModule.DarkenToBlack(fgPalette, 1);
            _paletteModule.DarkenToBlack(fgPalette, 2);
            _paletteModule.DarkenToBlack(fgPalette, 3);
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
            //_audioService.PlaySound(ChompAudioService.Sound.Lightning);           
            //_hitPoints.Value--;

            //if(_hitPoints.Value == 0)
            //    WorldSprite.Status = WorldSpriteStatus.Dying;
            //else
            //    SetPhase(Phase.Hurt);
         
            return BombCollisionResponse.Destroy;
        }

        public override bool CollidesWithBomb(WorldSprite bomb)
        {
            return false;
            //if (_phase.Value <= Phase.BossAppear)
            //    return false;

            //return bomb.Bounds.Intersects(_eye1.WorldSprite.Bounds) || bomb.Bounds.Intersects(_eye2.WorldSprite.Bounds);
        }

        public override bool CollidesWithPlayer(PlayerController player)
        {
            //if (_phase.Value <= Phase.BossAppear)
            //    return false;

            return false; //todo
        }
    }
}
