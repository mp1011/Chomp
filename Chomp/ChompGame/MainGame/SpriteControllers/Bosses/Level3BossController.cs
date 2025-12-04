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
    class Level3BossController : LevelBossController
    {
        private BossPart _leftEye, _rightEye, _leftJaw, _rightJaw;
        private NibbleEnum<Phase> _phase;
        private GameBit _leftJawOpen, _rightJawOpen;
        private MaskedByte _eyeFlashTimer;

        protected override bool AlwaysActive => true;
        protected override bool DestroyWhenFarOutOfBounds => false;
        protected override bool DestroyWhenOutOfBounds => false;

        public Level3BossController(ChompGameModule gameModule, WorldSprite player, EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers, SystemMemoryBuilder memoryBuilder) : base(gameModule, player, bulletControllers, memoryBuilder)
        {
            _leftEye = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _rightEye = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _leftJaw = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossJaw, memoryBuilder.Memory));
            _rightJaw = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossJaw, memoryBuilder.Memory));

            _phase = new NibbleEnum<Phase>(new LowNibble(memoryBuilder));
            _leftJawOpen = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit4, memoryBuilder.Memory);
            _rightJawOpen = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit5, memoryBuilder.Memory);
            _eyeFlashTimer = new MaskedByte(memoryBuilder.CurrentAddress, Bit.Left2, memoryBuilder.Memory, leftShift:6);
            memoryBuilder.AddByte();
        }

        private enum Phase : byte
        {
            BeforeBoss,
            Init,
            RightHook,
            LeftHook,
            LeftDive,
            RightDive,
            Bombard,
            Explosions,
            Sink,
            Finish
        }

        protected override int BossHP => GameDebug.BossTest ? 1 : 5;

        protected override string BossTiles { get; } =
            @"BAAAAA9008AAAAAC
              6MMMJII98IIHMMM5
              00BAAIIIIIIAAAC0
              006MJIIIIII4M500
              00000GIIII400000
              000000DE12000000";


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
                SetupBossParts();
                _leftEye.Sprite.Visible = false;
                _rightEye.Sprite.Visible = false;
                _leftJaw.Sprite.Visible = false;
                _rightJaw.Sprite.Visible = false;

                _paletteModule.BgColor = ColorIndex.Black;
                HideBoss();
            }
            else if(p == Phase.Init)
            {
                _gameModule.RewardsModule.GiveHealth(_gameModule.SceneSpriteControllers);

                SetBossTiles();
                SetupBossParts();
                WorldSprite.X = 20;
                WorldSprite.Y = 80;
                HideBoss();
            }
            else if(p == Phase.RightHook)
            {
                WorldSprite.Y = 62;
                WorldSprite.X = _player.X - 48;
                HideBoss();

                _motion.TargetXSpeed = -80;
                _motion.XSpeed = 80;
                _motion.XAcceleration = 3;

                _motion.YSpeed = 20;
                _motion.TargetYSpeed = 1;
                _motion.YAcceleration = 1;
                _leftJawOpen.Value = true;
                _rightJawOpen.Value = false;
            }
            else if (p == Phase.LeftHook)
            {
                WorldSprite.Y = 62;
                WorldSprite.X = _player.X;
                HideBoss();

                _motion.TargetXSpeed = 80;
                _motion.XSpeed = -80;
                _motion.XAcceleration = 3;

                _motion.YSpeed = 20;
                _motion.TargetYSpeed = 1;
                _motion.YAcceleration = 1;
                _leftJawOpen.Value = false;
                _rightJawOpen.Value = true;
            }
            else if( p== Phase.RightDive)
            {
                WorldSprite.Y = 62;
                WorldSprite.X = _player.X - 48;
                HideBoss();

                _motion.TargetXSpeed = 100;
                _motion.XSpeed = 0;
                _motion.XAcceleration = 3;

                _motion.YSpeed = 0;
                _motion.TargetYSpeed = 20;
                _motion.YAcceleration = 2;
                _leftJawOpen.Value = true;
                _rightJawOpen.Value = false;
                _leftJaw.YOffset = JawClosed;
            }
            else if(p == Phase.LeftDive)
            {
                WorldSprite.Y = 62;
                WorldSprite.X = _player.X;
                HideBoss();

                _motion.TargetXSpeed = -100;
                _motion.XSpeed = 0;
                _motion.XAcceleration = 3;

                _motion.YSpeed = 0;
                _motion.TargetYSpeed = 20;
                _motion.YAcceleration = 2;
                _leftJawOpen.Value = true;
                _rightJawOpen.Value = false;
                _rightJaw.YOffset = JawClosed;
            }
            else if (p == Phase.Bombard)
            {
                WorldSprite.Y = 62;
                WorldSprite.X = _player.X-24;
                HideBoss();

                _motion.SetXSpeed(0);

                _motion.SetYSpeed(10);

                _leftJawOpen.Value = true;
                _rightJawOpen.Value = true;
                _rightJaw.YOffset = JawClosed;
                _leftJaw.YOffset = JawClosed;
            }
        }

        protected override void UpdateActive()
        {
            if(_eyeFlashTimer.Value > 0)
            {
                if (_levelTimer.IsMod(16))
                    _eyeFlashTimer.Value--;

                if(_eyeFlashTimer.Value == 0)
                {
                    _leftEye.Sprite.Palette = SpritePalette.Enemy1;
                    _rightEye.Sprite.Palette = SpritePalette.Enemy1;
                }
            }
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
                // debug
                //FadeIn();
                //var ks = Microsoft.Xna.Framework.Input.Keyboard.GetState();
                //if (ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.B))
                //{
                //    if (ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                //        WorldSprite.X--;
                //    else if (ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
                //        WorldSprite.X++;
                //    else if (ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.C))
                //        SetBossTiles();
                //}

                //   WorldSprite.Status = WorldSpriteStatus.Dying;
                //   _hitPoints.Value = 0;
                _musicModule.CurrentSong = MusicModule.SongName.Nemesis;
                SetPhase(Phase.RightHook);
            }
            else if(_phase.Value == Phase.RightHook)
            {
                PositionFreeCoinBlocksNearPlayer();
                if (_stateTimer.Value == 15 || WorldSprite.X <= -55)
                {
                    GoNextPhase();
                }
                else if (_levelTimer.IsMod(24))
                {                   
                    if (_stateTimer.Value < 4)
                        FadeIn();
                    else if (_stateTimer.Value >= 8)
                        FadeOut();
                    
                    _stateTimer.Value++;
                }

                if (_stateTimer.Value == 5)
                {
                    _leftJawOpen.Value = false;
                    FireBullet(_leftJaw, -1);
                    FireBullet(_leftJaw, -25);
                    FireBullet(_leftJaw, -40);
                    _stateTimer.Value++;
                }
            }
            else if (_phase.Value == Phase.LeftHook)
            {
                PositionFreeCoinBlocksNearPlayer();
                if (_stateTimer.Value == 15 || WorldSprite.X <= -55)
                {
                    GoNextPhase();
                }
                else if (_levelTimer.IsMod(24))
                {
                    if (_stateTimer.Value < 4)
                        FadeIn();
                    else if (_stateTimer.Value >= 8)
                        FadeOut();

                    _stateTimer.Value++;
                }
                if (_stateTimer.Value == 5)
                {
                    _rightJawOpen.Value = false;
                    FireBullet(_rightJaw, 1);
                    FireBullet(_rightJaw, 25);
                    FireBullet(_rightJaw, 40);
                    _stateTimer.Value++;
                }
            }
            else if (_phase.Value == Phase.RightDive)
            {
                PositionFreeCoinBlocksNearPlayer();
                if (WorldSprite.X > 130)
                    WorldSprite.X = 130;
                if (WorldSprite.Y > 94)
                    WorldSprite.Y = 94;

                if (_levelTimer.IsMod(24))
                {
                    if (_stateTimer.Value < 4)
                        FadeIn();
                    else if (_stateTimer.Value >= 8)
                        FadeOut();

                    _stateTimer.Value++;
                    if (_stateTimer.Value == 15)
                        GoNextPhase();
                }

                if(_stateTimer.Value >= 3 && _stateTimer.Value < 7 && _levelTimer.IsMod(16))
                    FireStraightBullet(_leftJaw, -40);
            }
            else if (_phase.Value == Phase.LeftDive)
            {
                PositionFreeCoinBlocksNearPlayer();
                if (WorldSprite.X < -55)
                    WorldSprite.X = -55;
                if (WorldSprite.Y > 94)
                    WorldSprite.Y = 94;

                if (_levelTimer.IsMod(24))
                {
                    if (_stateTimer.Value < 4)
                        FadeIn();
                    else if (_stateTimer.Value >= 8)
                        FadeOut();

                    _stateTimer.Value++;
                    if (_stateTimer.Value == 15)
                        GoNextPhase();
                }

                if (_stateTimer.Value >= 3 && _stateTimer.Value < 7 && _levelTimer.IsMod(16))
                    FireStraightBullet(_leftJaw, 40);
            }
            else if(_phase.Value == Phase.Bombard)
            {
                PositionFreeCoinBlocksNearPlayer();

                if (_levelTimer.IsMod(32))
                {
                    _stateTimer.Value++;
                    if (_stateTimer.Value == 0)
                        GoNextPhase();
                }
                
                if (_stateTimer.Value < 4)
                    FadeIn();
                else if (_stateTimer.Value >= 12)
                    FadeOut();

                if(_stateTimer.Value >= 2)
                {
                    _motion.TargetYSpeed = -1;
                    _motion.YAcceleration = 1;
                }

                if (_stateTimer.Value > 2 
                    && _stateTimer.Value.IsMod(2) 
                    && _stateTimer.Value < 12 
                    && _levelTimer.IsMod(16))
                {
                    if (_rng.RandomChance(50))
                        FireBulletFalling(_leftJaw, (_rng.Generate(5)) - 16);
                    else
                        FireBulletFalling(_rightJaw, (_rng.Generate(5)) - 16);
                }
            }

            _motionController.Update();
            _position.X = (byte)(WorldSprite.X - 8 - _tileModule.Scroll.X);
            _position.Y = (byte)(WorldSprite.Y - 66);

            UpdateJaw(_leftJaw, _leftJawOpen.Value);
            UpdateJaw(_rightJaw, _rightJawOpen.Value);

            UpdatePartPositions();

            if (_phase.Value > Phase.Init)
            {
                HideOffscreenBossTiles(7, 71, 16);
                _leftJaw.WorldSprite.SetVisibleWhenInBounds();
                _rightJaw.WorldSprite.SetVisibleWhenInBounds();
                _leftEye.WorldSprite.SetVisibleWhenInBounds();
                _rightEye.WorldSprite.SetVisibleWhenInBounds();
            }
        }

        protected override void UpdateDying()
        {
            if(_phase.Value < Phase.Explosions)
            {
                _statusBar.AddToScore((uint)PointsForEnemy);
                _musicModule.CurrentSong = MusicModule.SongName.None;
                _phase.Value = Phase.Explosions;
                _stateTimer.Value = 0;
                _motion.SetXSpeed(0);
                _motion.SetYSpeed(0);
                
            }

            if(_phase.Value == Phase.Explosions)
            {
                FadeIn();

                if (_stateTimer.Value < 4)
                {
                    _leftJaw.XOffset -= 1;
                    _leftJaw.YOffset++;
                    _rightJaw.XOffset++;
                    _rightJaw.YOffset++;
                }
                else
                {
                    _leftJaw.Sprite.Visible = false;
                    _rightJaw.Sprite.Visible = false;
                }

                if(_levelTimer.IsMod(8))
                {
                    _stateTimer.Value++;
                    if (_stateTimer.Value == 0)
                        _phase.Value = Phase.Sink;

                    CreateExplosions();
                }
            }
            else if(_phase.Value == Phase.Sink)
            {
                if(_stateTimer.Value==0)
                {
                    TurnRed();
                    _stateTimer.Value++;
                }

                _motion.TargetYSpeed = 1;
                _motion.YSpeed = 5;
                _motion.YAcceleration = 1;

                if(_levelTimer.IsMod(64))
                    FadeOut();

                if (_levelTimer.IsMod(8))
                {
                    _stateTimer.Value++;
                    if (_stateTimer.Value == 0)
                    {
                        _phase.Value = Phase.Finish;
                    }

                    CreateExplosions();
                }
            }
            else if (_phase.Value == Phase.Finish)
            {
                if (_stateTimer.Value == 0)
                {
                    _stateTimer.Value++;
                    _leftEye.Sprite.Visible = false;
                    _rightEye.Sprite.Visible = false;
                    HideBoss();
                }

                if(_levelTimer.IsMod(16))
                    _stateTimer.Value++;

                if (_stateTimer.Value == 0)
                {
                    
                    Destroy();

                    _exitsModule.GotoNextLevel();
                }
            }

            if (WorldSprite.Y > 90)
                WorldSprite.Y = 90;

            _motionController.Update(false);
            _position.X = (byte)(WorldSprite.X - 8 - _tileModule.Scroll.X);
            _position.Y = (byte)(WorldSprite.Y - 66);
            UpdatePartPositions();
        }

        private void CreateExplosions()
        {
            var x = _rng.Generate(5);
            var y = _rng.Generate(4);
            CreateExplosion(WorldSprite.X + 8 + x, WorldSprite.Y + y);
        }

        private void GoNextPhase()
        {
            if (_hitPoints.Value == 5)
            {
                SetPhase(_phase.Value switch {
                    Phase.LeftHook => Phase.RightHook,
                    _ => Phase.LeftHook
                });
            }
            else if (_hitPoints.Value >= 3)
            {
                SetPhase(_phase.Value switch {
                    Phase.LeftDive => Phase.RightDive,
                    Phase.RightDive => Phase.Bombard,
                    _ => Phase.LeftDive
                });
            }
            else
            {
                SetPhase(_phase.Value switch {
                    Phase.RightHook => Phase.LeftDive,
                    Phase.LeftDive => Phase.LeftHook,
                    Phase.LeftHook => Phase.RightDive,
                    Phase.RightDive => Phase.Bombard,
                    _ => Phase.RightHook
                });
            }
        }

        private void FireBullet(BossPart origin, int xSpeed)
        {
            var bullet = _bulletControllers.TryAddNew();
            if (bullet == null)
                return;

            bullet.DestroyOnTimer = false;
            bullet.WorldSprite.X = origin.WorldSprite.X;
            bullet.WorldSprite.Y = origin.WorldSprite.Y - 4;
            bullet.WorldSprite.FlipX = xSpeed < 0;

            bullet.AcceleratedMotion.XSpeed = xSpeed;
            bullet.AcceleratedMotion.TargetXSpeed = 0;
            bullet.AcceleratedMotion.XAcceleration = 1;

            bullet.Motion.YSpeed = -10;
            bullet.AcceleratedMotion.TargetYSpeed = 20;
            bullet.AcceleratedMotion.YAcceleration = 2;
            _audioService.PlaySound(ChompAudioService.Sound.Fireball);
        }

        private void FireStraightBullet(BossPart origin, int xSpeed)
        {
            var bullet = _bulletControllers.TryAddNew();
            if (bullet == null)
                return;

            bullet.DestroyOnTimer = true;
            bullet.WorldSprite.X = origin.WorldSprite.X;
            bullet.WorldSprite.Y = origin.WorldSprite.Y - 4;
            bullet.WorldSprite.FlipX = xSpeed < 0;

            bullet.AcceleratedMotion.XSpeed = 0;
            bullet.AcceleratedMotion.TargetXSpeed = xSpeed;
            bullet.AcceleratedMotion.XAcceleration = 5;

            bullet.AcceleratedMotion.SetYSpeed(0);
            _audioService.PlaySound(ChompAudioService.Sound.Fireball);
        }

        private void FireBulletFalling(BossPart origin, int xSpeed)
        {
            var bullet = _bulletControllers.TryAddNew();
            if (bullet == null)
                return;

            bullet.DestroyOnTimer = false;
            bullet.WorldSprite.X = origin.WorldSprite.X;
            bullet.WorldSprite.Y = origin.WorldSprite.Y - 4;
            bullet.WorldSprite.FlipX = xSpeed < 0;

            bullet.AcceleratedMotion.SetXSpeed(xSpeed);
            bullet.AcceleratedMotion.YSpeed = -20;
            bullet.AcceleratedMotion.TargetYSpeed = 40;
            bullet.AcceleratedMotion.YAcceleration = 2;
            _audioService.PlaySound(ChompAudioService.Sound.Fireball);
        }

        public override bool CollidesWithBomb(WorldSprite bomb)
        {
            var x = WorldSprite.X;
            var y = WorldSprite.Y;

            bool eyeHit = CheckEyeHit(_leftEye, bomb) || CheckEyeHit(_rightEye, bomb);

            WorldSprite.X = x;
            WorldSprite.Y = y;
            return eyeHit;
        }

        private bool CheckEyeHit(BossPart eye, WorldSprite bomb)
        {
            WorldSprite.X = eye.WorldSprite.X;
            WorldSprite.Y = eye.WorldSprite.Y;
            return base.CollidesWithBomb(bomb);
        }
        public override BombCollisionResponse HandleBombCollision(WorldSprite player)
        {
            _audioService.PlaySound(ChompAudioService.Sound.Lightning);
            _leftEye.Sprite.Palette = SpritePalette.Fire;
            _rightEye.Sprite.Palette = SpritePalette.Fire;
            _eyeFlashTimer.Value = 3;
            _hitPoints.Value--;
            if (_hitPoints.Value == 0)
                WorldSprite.Status = WorldSpriteStatus.Dying;
            return BombCollisionResponse.Destroy;
        }

        private const int JawOpen = 24;
        private const int JawClosed = 20;

        private void UpdateJaw(BossPart jaw, bool open)
        {
            if (_levelTimer.IsMod(16))
            {
                var targetOffset = open ? JawOpen : JawClosed;
                if (jaw.YOffset > targetOffset)
                    jaw.YOffset--;
                else if (jaw.YOffset < targetOffset)
                    jaw.YOffset++;
            }
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

        private void FadeOut()
        {
            Theme theme = new Theme(_graphicsModule.GameSystem.Memory, ThemeType.CityBoss);
            var spritePalette = _graphicsModule.GetSpritePalette(SpritePalette.Enemy1);
            _paletteModule.Darken(spritePalette, 1) ;
            _paletteModule.Darken(spritePalette, 2);
            _paletteModule.Darken(spritePalette, 3);

            var bossPalette = _paletteModule.BgPalette2;
            _paletteModule.Darken(bossPalette, 1);
            _paletteModule.Darken(bossPalette, 2);
            _paletteModule.Darken(bossPalette, 3);
        }

        private void TurnRed()
        {
            var targetPalette = _paletteModule.GetPalette(PaletteKey.Bullet);
           
            var spritePalette = _graphicsModule.GetSpritePalette(SpritePalette.Enemy1);
            spritePalette.SetColor(1, new ColorIndex(targetPalette.GetColorIndex(0)).Value);
            spritePalette.SetColor(2, new ColorIndex(targetPalette.GetColorIndex(1)).Value);
            spritePalette.SetColor(3, new ColorIndex(targetPalette.GetColorIndex(2)).Value);

            var bossPalette = _paletteModule.BgPalette2;
            bossPalette.SetColor(1, new ColorIndex(targetPalette.GetColorIndex(0)).Value);
            bossPalette.SetColor(2, new ColorIndex(targetPalette.GetColorIndex(1)).Value);
            bossPalette.SetColor(3, new ColorIndex(targetPalette.GetColorIndex(2)).Value);
        }

        private void SetupBossParts()
        {
            var leftEyeSprite = _leftEye.PrepareSprite(SpriteTileIndex.Enemy1);
            leftEyeSprite.Tile2Offset = 1;
            leftEyeSprite.FlipX = true;
            leftEyeSprite.FlipY = false;

            _leftEye.XOffset = 14;
            _leftEye.YOffset = 8;

            var rightEyeSprite = _rightEye.PrepareSprite(SpriteTileIndex.Enemy1);
            rightEyeSprite.Tile2Offset = 1;
            rightEyeSprite.FlipX = false;
            rightEyeSprite.FlipY = false;
            _rightEye.XOffset = 26;
            _rightEye.YOffset = 8;

            var leftJawSprite = _leftJaw.PrepareSprite(SpriteTileIndex.Enemy2);
            leftJawSprite.Tile2Offset = 0;
            leftJawSprite.FlipX = false;
            leftJawSprite.FlipY = false;
            _leftJaw.XOffset = 16;
            _leftJaw.YOffset = 20;

            var rightJawSprite = _rightJaw.PrepareSprite(SpriteTileIndex.Enemy2);
            rightJawSprite.Tile2Offset = 0;
            rightJawSprite.FlipX = true;
            rightJawSprite.FlipY = false;
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
