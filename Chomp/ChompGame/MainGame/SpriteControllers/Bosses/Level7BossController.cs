﻿using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Graphics;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;
using System;
 using System.ComponentModel.Design;

namespace ChompGame.MainGame.SpriteControllers.Bosses
{
    class Level7BossController : LevelBossController
    {
        private const bool Debug_StartAtPhase2 = false;

        private const int JawParts = 3;
        private NibbleEnum<Phase> _phase;
        private BossPart _eye1, _eye2;
        private ChompTail _leftJaw, _rightJaw;
        private ICollidableSpriteControllerPool _enemies;
        private SpriteControllerPool<BombController> _bombs;

        private GameBit _jawOpen;
        private MaskedByte _extraVar;
      
        protected override bool AlwaysActive => true;
        protected override bool DestroyWhenFarOutOfBounds => false;
        protected override bool DestroyWhenOutOfBounds => false;


        public Level7BossController(ChompGameModule gameModule, WorldSprite player, SpriteControllerPool<BombController> bombs, EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers, SystemMemoryBuilder memoryBuilder) 
            : base(gameModule, player, bulletControllers, memoryBuilder)
        {
            _bombs = bombs;
            _phase = new NibbleEnum<Phase>(new LowNibble(memoryBuilder));           
            memoryBuilder.AddByte();
          
            _eye1 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _eye2 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _leftJaw = new ChompTail(memoryBuilder, JawParts, gameModule);
            _rightJaw = new ChompTail(memoryBuilder, JawParts, gameModule);

            _jawOpen = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit0, memoryBuilder.Memory);
            _extraVar = new MaskedByte(memoryBuilder.CurrentAddress, Bit.Left7, memoryBuilder.Memory, leftShift: 1);
            memoryBuilder.AddByte();
        }

        private enum Phase : byte
        {
            BeforeBoss,
            EyesAppear,
            EnemySpawn,
            BossReappear,
            EyeAttack,
            Hurt,
            Transition,
            BeginPhase2,
            Attack1,
            BeamAttack,
            Attack2,
            BeamAttack2,
            Dying
        }

        protected override int BossHP => 7;

        private int Phase1BossHP => 3;

        protected override string BossTiles { get; } =
            @"00BAC00000008C00
              0BI1AII9BCC81I9C
              3I1II1I9II1IIII4
              0JIIII1II1II1I40
              0063FIIIIIIIFH00
              00000JIIIII40000
              000006GIII400000
              00000006F5000000
              0000000000000000
              003II900008I9000
              0003IIC00BII4000
              00006GIAAIIK0000
              000003IIII400000
              0000006G40000000";

        private string BossTilesNoJaw { get; } =
           @"00BAC00000008C00
              0BI1AII9BCC81I9C
              3I1II1I9II1IIII4
              0JIIII1II1II1I40
              0063FIIIIIIIFH00
              00000JIIIII40000
              000006GIII400000
              00000006F5000000
              0000000000000000
              0000000000000000
              0000000000000000
              0000000000000000
              0000000000000000";


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
         //   GameDebug.Watch4 = new DebugWatch("Extra Var", () => _extraVar.Value);
            GameDebug.Watch4 = new DebugWatch("HP", () => _hitPoints.Value);

            _phase.Value = p;
            _stateTimer.Value = 0;
            _extraVar.Value = 0;

            if (p == Phase.BeforeBoss)
            {                
                _gameModule.BossBackgroundHandler.ShowCoins = false;
                _paletteModule.BgColor = ColorIndex.Black;
                HideBoss();
            }
            else if (p == Phase.EyesAppear)
            {
                _hitPoints.Value = (byte)Phase1BossHP;
                _musicModule.CurrentSong = GameSystem.MusicModule.SongName.FinalBossPart1;

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
                _enemies = _gameModule.FinalBossHelper.SetEnemy(EnemyToSpawn());  
                
                _dynamicBlockController.RestoreCoins();
            }
            else if (p == Phase.BossReappear)
            {
                _eye1.Sprite.Visible = true;
                _eye2.Sprite.Visible = true;
                ResetBackground();
            }
            else if( p == Phase.Hurt)
            {
                _eye1.Sprite.Palette = SpritePalette.Fire;
                _eye2.Sprite.Palette = SpritePalette.Fire;
            }
            else if (p == Phase.Transition)
            {
                _bossBackgroundHandler.ShowCoins = false;
                _dynamicBlockController.ResetCoinsForLevelBoss();
                _hitPoints.Value = (byte)BossHP;
                _eye1.Sprite.Palette = SpritePalette.Fire;
                _eye2.Sprite.Palette = SpritePalette.Fire;
                _musicModule.CurrentSong = GameSystem.MusicModule.SongName.FinalBossPart1End;

                SetBackgroundForPhase2();
                _bossBackgroundHandler.ShowCoins = false;
                _gameModule.CollissionDetector.BossBgHandling = true;
                // GameDebug.Watch3 = new DebugWatch("Music Timer", () => (int)_musicModule.PlayPosition);
            }
            else if (p == Phase.BeginPhase2)
            {
                _musicModule.CurrentSong = GameSystem.MusicModule.SongName.FinalBossPart2;
            }
            else if(p == Phase.Attack1)
            {
                _bossBackgroundHandler.BossBgEffectType = BackgroundEffectType.FinalBossLower;
            }
            else if (p == Phase.Attack2)
            {
                _bossBackgroundHandler.BossBgEffectType = BackgroundEffectType.FinalBossLower;
            }
            else if (p == Phase.Dying)
            {
                _eye1.Sprite.Palette = SpritePalette.Fire;
                _eye2.Sprite.Palette = SpritePalette.Fire;
                _musicModule.CurrentSong = GameSystem.MusicModule.SongName.FinalBossPart2End;
            }
        }

        private EnemyIndex EnemyToSpawn()
        {
            if (_hitPoints.Value == 3)
                return EnemyIndex.Lizard;
            else if (_hitPoints.Value == 2)
                return EnemyIndex.Bird;
            else if (_hitPoints.Value == 1)
                return EnemyIndex.Ogre;


            return EnemyIndex.Ufo;

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

        private void ResetBackground()
        {
            _worldScroller.ModifyTiles((t, a) =>
            {
                t.ForEach((x, y, b) =>
                {
                    if (y < t.Height - 2)
                        t[x, y] = 0;
                });
            });
        }

        private void SetBackgroundForPhase2()
        {
            _worldScroller.ModifyTiles((t, a) =>
            {
                t.ForEach((x, y, b) =>
                {
                    if (y < t.Height - 2)
                        t[x, y] = 0;
                });

                a.ForEach((x, y, b) =>
                {
                    if(y == a.Height-1)
                        a[x, y] = 1;
                    else
                        a[x, y] = 0;
                });

            });
        }


        private void SetupBossParts()
        {
            var eye1Sprite = _eye1.PrepareSprite(SpriteTileIndex.Enemy1);
            eye1Sprite.Tile2Offset = 1;
            eye1Sprite.FlipX = true;
            eye1Sprite.FlipY = false;
            eye1Sprite.Priority = false;
            _eye1.XOffset = 0;
            _eye1.YOffset = 0;
            eye1Sprite.Visible = true;

            var eye2Sprite = _eye2.PrepareSprite(SpriteTileIndex.Enemy1);
            eye2Sprite.Tile2Offset = 1;
            eye2Sprite.FlipX = false;
            eye2Sprite.FlipY = false;
            eye2Sprite.Priority = false;
            _eye2.XOffset = 16;
            _eye2.YOffset = 0;
            eye2Sprite.Visible = true;
        }

        private void CreateJaw()
        {
            _leftJaw.CreateTail(SpriteTileIndex.Enemy2, 2);
            _rightJaw.CreateTail(SpriteTileIndex.Enemy2,2);

            for(int i = 0; i < JawParts; i++)
            {
                _leftJaw.GetSprite(i).Priority = false;
                _rightJaw.GetSprite(i).Priority = false;
            }
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
                SetEyePos();
                
                if(_levelTimer.IsMod(32))
                {
                    _stateTimer.Value++;
                    if (_stateTimer.Value == 0)
                        SetPhase(Phase.EnemySpawn);

                    if (_stateTimer.Value < 8)
                        FadeIn(false);
                    else
                    {
                        if (Debug_StartAtPhase2)
                        {
                            _hitPoints.Value = (byte)BossHP;
                            SetPhase(Phase.Transition);
                        }
                        else
                            FadeOut();
                    }
                }
            }
            else if(_phase.Value == Phase.EnemySpawn)
            {
                bool anyActive = false;

                _enemies.Execute(a => { anyActive = true; });

                if (!anyActive)
                {
                    if (_levelTimer.IsMod(16))
                    {
                        FadeOut();
                        _stateTimer.Value++;
                    }

                    if(_stateTimer.Value == 14)
                        SetPhase(Phase.BossReappear);
                }
                else if (_levelTimer.IsMod(16))
                {
                    FadeIn(true);
                }
            }
            else if (_phase.Value == Phase.BossReappear)
            {
                SetEyePos();
                if (_levelTimer.IsMod(16))
                {
                    _stateTimer.Value++;
                    FadeIn(true);
                }

                if (_stateTimer.Value == 10)
                    SetPhase(Phase.EyeAttack);
            }
            else if(_phase.Value == Phase.EyeAttack)
            {
                SetEyePos();
                if (_levelTimer.IsMod(32))
                {
                    CreateRandomAimedBullet();
                }
            }
            else if (_phase.Value == Phase.Hurt)
            {
                SetEyePos();
               
                if (_levelTimer.IsMod(32))
                {
                    _stateTimer.Value++;
                    if (_stateTimer.Value == 0)
                        SetPhase(Phase.EnemySpawn);

                    if (_stateTimer.Value >= 4)
                    {
                        _eye1.Sprite.Palette = SpritePalette.Enemy1;
                        _eye2.Sprite.Palette = SpritePalette.Enemy1;
                        FadeOut();
                    }
                }
            }
            else if(_phase.Value == Phase.Transition)
            {
                if (_extraVar.Value == 0)
                {
                    if (_levelTimer.IsMod(16))
                        _stateTimer.Value++;

                    if (_stateTimer.Value == 15)
                    {
                        _stateTimer.Value = 0;
                        _extraVar.Value++;
                    }
                    SetEyePos();
                }
                else if (_extraVar.Value == 1)
                {
                    _eye1.Sprite.Palette = SpritePalette.Enemy1;
                    _eye2.Sprite.Palette = SpritePalette.Enemy1;
                    if (_levelTimer.IsMod(32))
                    {
                        _stateTimer.Value++;
                        
                        if(_stateTimer.Value >= 4)
                            CreateRandomAimedBullet();
                    }

                    if (_stateTimer.Value == 15)
                    {
                        _stateTimer.Value = 0;
                        _extraVar.Value++;
                    }
                    SetEyePos();
                }
                if (_extraVar.Value == 2)
                {
                    if (_levelTimer.IsMod(8))
                        _stateTimer.Value++;

                    if (_stateTimer.Value == 15)
                    {
                        _stateTimer.Value = 0;
                        _extraVar.Value++;
                    }
                    SetEyePos();
                }
                else if (_extraVar.Value == 3)
                {
                    BossIntroMotion();

                    if (_levelTimer.IsMod(16))
                    {
                        _eye1.XOffset = _eye1.XOffset.MoveToward(-14, 1);
                        _eye1.YOffset = _eye1.YOffset.MoveToward(2, 1);
                        _eye2.XOffset = _eye2.XOffset.MoveToward(10, 1);
                        _eye2.YOffset = _eye2.YOffset.MoveToward(2, 1);

                        _stateTimer.Value++;
                    }

                    if (_stateTimer.Value == 15)
                    {
                        _stateTimer.Value = 0;
                        _extraVar.Value++;
                    }
                }
                if (_extraVar.Value == 4)
                {
                    BossIntroMotion();

                    if (_levelTimer.IsMod(32))
                    {
                        CreateRandomAimedBullet();
                    }

                    if (_levelTimer.IsMod(32))
                        _stateTimer.Value++;

                    if (_stateTimer.Value == 15)
                    {
                        _stateTimer.Value = 0;
                        _extraVar.Value++;
                    }
                }
                else if (_extraVar.Value == 5)
                {
                    BossIntroMotion();

                    if (_levelTimer.IsMod(8))
                        _stateTimer.Value++;

                    if (_stateTimer.Value == 15)
                    {
                        _stateTimer.Value = 0;

                        PrepBossTiles();
                        _eye1.Sprite.Priority = true;
                        _eye2.Sprite.Priority = true;
                        SetBossColor(ColorIndex.Black);
                        _paletteModule.BgColor = ColorIndex.White;
                        _audioService.PlaySound(ChompAudioService.Sound.Lightning);
                        _extraVar.Value++;
                    }
                }
                else if (_extraVar.Value >= 6 && _extraVar.Value <= 8)
                {
                    if (_levelTimer.IsMod(4))
                        FadeOut();

                    BossIntroMotion();


                    if (_levelTimer.IsMod(4))
                    {
                        _stateTimer.Value++;

                        if (_stateTimer.Value > 0 && _stateTimer.Value < 8)
                            _paletteModule.BgColor = ColorIndex.LightGray(7 - _stateTimer.Value).Value;
                        else if (_stateTimer.Value != 0)
                            _paletteModule.BgColor = ColorIndex.DarkGray(15 - _stateTimer.Value).Value;
                        else
                        {
                            _audioService.PlaySound(ChompAudioService.Sound.Lightning);
                            _extraVar.Value++;
                        }
                    }
                }
                else if (_extraVar.Value == 9)
                {
                    _paletteModule.BgColor = ColorIndex.Black;
                    BossIntroMotion();
                    if (_levelTimer.IsMod(4))
                        FadeIn(true);
                }
                    //   if(_gameModule.InputModule.Player1.AKey.IsDown() && Debug_StartAtPhase2)
                    //    SetPhase(Phase.BeginPhase2);

                if (_musicModule.PlayPosition >= 44100)
                    SetPhase(Phase.BeginPhase2);
            }
            else if(_phase.Value == Phase.BeginPhase2)
            {
                SetPhase(Phase.Attack1);
                var bomb = _bombs.TryAddNew();
                bomb.WorldSprite.X = _player.X;
                bomb.WorldSprite.Y = _player.Y - 16;
            }
            else if (_phase.Value == Phase.Attack1)
            {
                if (_levelTimer.IsMod(4))
                    FadeIn(false);

                BossMotion();
                if (_levelTimer.IsMod(8))
                {
                    if (_extraVar.Value < 16)
                    {
                        var angle = 300 + (_extraVar.Value * 8);

                        FireBulletAtAngle(_eye1.WorldSprite.Center, angle, 100);
                    }
                    else if (_extraVar.Value >= 20 && _extraVar.Value < 36)
                    {
                        var angle = (60 - ((_extraVar - 20) * 8)) % 360;

                        FireBulletAtAngle(_eye2.WorldSprite.Center, angle, 100);
                    }
                    else if (_extraVar == 40)
                    {
                        for (int angle = -40; angle <= 40; angle += 8)
                        {
                            FireBulletAtAngle(_eye1.WorldSprite.Center, angle.NMod(360), 30);
                            FireBulletAtAngle(_eye2.WorldSprite.Center, angle.NMod(360), 30);
                        }
                    }
                    else if (_extraVar.Value == 60)
                        SetPhase(Phase.BeamAttack);

                    _extraVar.Value++;


                }
            }
            else if (_phase.Value == Phase.BeamAttack || _phase.Value == Phase.BeamAttack2)
            {
                if(_levelTimer.IsMod(4))
                    FadeIn(false);

                _motion.TargetXSpeed = 0;
                if (_stateTimer.Value == 0)
                {
                    _jawOpen.Value = false;
                    MoveBossToY(70);

                    if (Math.Abs(WorldSprite.Y - 70) < 2)
                        _stateTimer.Value++;
                }
                else if (_stateTimer.Value == 1)
                {
                    if (_levelTimer.IsMod(16))
                        _jawOpen.Value = !_jawOpen.Value;

                    MoveBossToY(70);
                    _bossBackgroundHandler.BossBgEffectType = BackgroundEffectType.FinalBossLowerBeam;
                    if (_levelTimer.IsMod(16))
                    {
                        _audioService.PlaySound(ChompAudioService.Sound.Lightning);
                        _extraVar.Value++;

                        if (_extraVar.Value == 4)
                            _stateTimer.Value = 2;
                    }
                }
                else if (_stateTimer.Value == 2)
                {
                    _jawOpen.Value = false;
                    if (MoveBossToY(90))
                    {
                        if (_levelTimer.IsMod(16))
                            _extraVar.Value++;

                        if (_extraVar.Value >= 8)
                        {
                            _stateTimer.Value = 3;
                            _extraVar.Value = 0;
                        }
                    }
                    _bossBackgroundHandler.BossBgEffectType = BackgroundEffectType.FinalBossLower;
                }
                else if (_stateTimer.Value == 3)
                {
                    if (_levelTimer.IsMod(16))
                        _jawOpen.Value = !_jawOpen.Value;

                    MoveBossToY(90);
                    _bossBackgroundHandler.BossBgEffectType = BackgroundEffectType.FinalBossLowerBeam;
                    if (_levelTimer.IsMod(16))
                    {
                        _audioService.PlaySound(ChompAudioService.Sound.Lightning);
                        _extraVar.Value++;

                        if (_extraVar.Value >= 4)
                        {
                            if(_phase.Value == Phase.BeamAttack)
                                SetPhase(Phase.Attack1);
                            else if (_phase.Value == Phase.BeamAttack2)
                                SetPhase(Phase.Attack2);
                        }

                    }
                }

                PositionJaw2();
                PositionBoss();
                _motionController.Update();
            }
            else if (_phase.Value == Phase.Attack2)
            {
                if (_levelTimer.IsMod(4))
                    FadeIn(false);

                BossMotion();
                if (_levelTimer.IsMod(8))
                {
                    if (_extraVar.Value < 10)
                    {
                        var angle = 270 + (_extraVar.Value * 8);
                        FireBulletAtAngle(_eye1.WorldSprite.Center, angle, 100);

                        angle = (60 - (_extraVar * 8) % 360);
                        FireBulletAtAngle(_eye2.WorldSprite.Center, angle, 100);

                    }
                    else if (_extraVar == 26)
                    {
                        for (int angle = -40; angle <= 40; angle += 8)
                        {
                            FireBulletAtAngle(_eye1.WorldSprite.Center, angle.NMod(360), 30);
                            FireBulletAtAngle(_eye2.WorldSprite.Center, angle.NMod(360), 30);
                        }
                    }
                    else if (_extraVar == 36)
                    {
                        for (int angle = -60; angle <= 60; angle += 12)
                        {
                            FireBulletAtAngle(_eye1.WorldSprite.Center, angle.NMod(360), 30);
                            FireBulletAtAngle(_eye2.WorldSprite.Center, angle.NMod(360), 30);
                        }
                    }
                    else if (_extraVar.Value == 60)
                        SetPhase(Phase.BeamAttack2);

                    _extraVar.Value++;
                }
            }
            else if (_phase.Value == Phase.Dying)
            {
                if (_levelTimer.IsMod(64) && _stateTimer.Value < 15)
                    _stateTimer.Value++;

                int jawDestroyTime = 8;
                _motion.TargetXSpeed = 0;
                _motion.TargetYSpeed = 0;

                PositionJaw2();
                PositionBoss();
                _motionController.Update();

                if (_levelTimer.Value.IsMod(2))
                {
                    _worldScroller.OffsetCamera(_rng.Generate(1), 1);

                    if (_stateTimer.Value < jawDestroyTime)
                    {
                        _bossBackgroundHandler.FinalBossLowerY = _rng.Generate(2);
                        _bossBackgroundHandler.FinalBossLowerX = 1;
                    }
                }

                if (_stateTimer.Value >= 12)
                {
                    if (_levelTimer.IsMod(16))
                        FadeOut();
                }
                else if(_stateTimer.Value == 15)
                {
                    FadeOut();
                    EraseBossTiles();
                }
                else
                {
                    if (_levelTimer.IsMod(32))
                    {
                        _jawOpen.Value = !_jawOpen.Value;
                        var bossPalette = _paletteModule.BgPalette2;
                        bossPalette.SetColor(1, ColorIndex.Red3);
                        bossPalette.SetColor(2, ColorIndex.Red2);
                        bossPalette.SetColor(3, ColorIndex.Red1);
                    }
                    else if (_levelTimer.IsMod(4))
                    {
                        FadeIn(false);
                    }

                    if (_stateTimer.Value == jawDestroyTime)
                    {
                        EraseJawTiles();
                        _bossBackgroundHandler.BossBgEffectType = BackgroundEffectType.None;
                        _bossBackgroundHandler.BossBgEffectValue = 0;
                    }

                    if (_levelTimer.IsMod(3) || _levelTimer.IsMod(5))
                    {
                        _audioService.PlaySound(ChompAudioService.Sound.Break);
                        CreateExplosion(WorldSprite.X + -20 + _rng.Generate(5), WorldSprite.Y + -2 + _rng.Generate(4));

                        if (_stateTimer.Value < jawDestroyTime)
                            CreateExplosion(WorldSprite.X + -16 + _rng.Generate(5), WorldSprite.Y + 28 + _rng.Generate(2));
                    }
                }


                if (_musicModule.PlayPosition >= 26500)
                    _exitsModule.GotoNextLevel();
            }
        }

        private void EraseJawTiles()
        {
            var tileStart = 16;

            _worldScroller.ModifyTiles((nt, _) =>
            {
                nt.SetFromString(0, 13, tileStart,
                BossTilesNoJaw);
            });
        }

        private void PrepBossTiles()
        {
            SetBossTiles();
            _gameModule.FinalBossHelper.SetPhase2Sprite();
            _bossBackgroundHandler.BossBgEffectType = BackgroundEffectType.FinalBossLower;
            _bossBackgroundHandler.BossBgEffectValue = 0;

            CreateJaw();
        }

        private bool MoveBossToY(int y)
        {
            int distance = y - WorldSprite.Y;

            if (Math.Abs(distance) > 4)
            {
                // Move quickly toward the target
                _motion.TargetYSpeed = distance > 0 ? 16 : -16;
                _motion.YAcceleration = 2;
                return false;
            }
            else
            {
                // Slow down as we approach the target
                _motion.TargetYSpeed = distance > 0 ? 2 : -2;
                _motion.YAcceleration = 1;
                return true;
            }
        }

        private void FireBulletAtAngle(Point origin, int angle, int speed)
        {
            var xy = GameMathHelper.PointFromAngle(angle, speed);
            FireBullet(origin, xy.X, xy.Y);
        }

        private void FireBullet(Point origin, int x, int y)
        {
            var bullet = _bulletControllers.TryAddNew() as Boss7BulletController;
            if (bullet == null)
                return;

            bullet.Mode = Boss7BulletController.BulletMode.NoCoin;

            bullet.DestroyOnTimer = true;
            bullet.DestroyOnCollision = true;
            bullet.WorldSprite.Center = origin;
            bullet.AcceleratedMotion.SetXSpeed(x);
            bullet.AcceleratedMotion.SetYSpeed(y);
            _audioService.PlaySound(ChompAudioService.Sound.Fireball);
        }

        private void BossIntroMotion()
        {
            if (_levelTimer.Value.IsMod(5))
            {
                if (WorldSprite.X < 58)
                {
                    _motion.TargetXSpeed = 4;
                    _motion.XAcceleration = 2;
                }
                else
                {
                    _motion.TargetXSpeed = -4;
                    _motion.XAcceleration = 2;
                }
            }

            if (_levelTimer.Value.IsMod(3))
            {
                if (WorldSprite.Y < 78)
                {
                    _motion.TargetYSpeed = 4;
                    _motion.YAcceleration = 2;
                }
                else
                {
                    _motion.TargetYSpeed = -4;
                    _motion.YAcceleration = 2;
                }
            }

            PositionJaw();
            PositionBoss();
            _motionController.Update();
        }


        private void BossMotion()
        {
            if (_levelTimer.IsMod(8))
                _stateTimer.Value++;

            if (_stateTimer.Value.IsMod(5))
            {
                if (WorldSprite.X < 58)
                {
                    _motion.TargetXSpeed = 16;
                    _motion.XAcceleration = 2;
                }
                else
                {
                    _motion.TargetXSpeed = -16;
                    _motion.XAcceleration = 2;
                }
            }

            if (_stateTimer.Value.IsMod(3))
            {
                if (WorldSprite.Y < 78)
                {
                    _motion.TargetYSpeed = 16;
                    _motion.YAcceleration = 2;
                }
                else
                {
                    _motion.TargetYSpeed = -16;
                    _motion.YAcceleration = 2;
                }
            }

            PositionJaw();
            PositionBoss();
            _motionController.Update();
        }

        private void PositionJaw()
        {
            if (!_levelTimer.IsMod(4))
                return;

            byte targetX, targetY;

           // targetY = (byte)(_jawOpen.Value ? 5 : 0);
            if (_motion.XSpeed > 0)
                targetX = 0;
            else if (_motion.XSpeed < 0)
                targetX = 8;
            else
                targetX = 4;

            if (_motion.YSpeed > 0)
                targetY = 0;
            else if (_motion.YSpeed < 0)
                targetY = 5;
            else
                targetY = 2;


            _bossBackgroundHandler.FinalBossLowerY = _bossBackgroundHandler.FinalBossLowerY.MoveToward(targetY, 1);
            _bossBackgroundHandler.FinalBossLowerX = _bossBackgroundHandler.FinalBossLowerX.MoveToward(targetX, 1);
        }
        private void PositionJaw2()
        {
            if (!_levelTimer.IsMod(4))
                return;

            byte targetX, targetY;

            targetX = 1;
            targetY = _jawOpen.Value ? (byte)5 : (byte)0;

            _bossBackgroundHandler.FinalBossLowerY = _bossBackgroundHandler.FinalBossLowerY.MoveToward(targetY, 1);
            _bossBackgroundHandler.FinalBossLowerX = _bossBackgroundHandler.FinalBossLowerX.MoveToward(targetX, 1);
        }

        private void SetEyePos()
        {
            WorldSprite.X = (_player.X - 10).Clamp(22, 86);
            WorldSprite.Y = 90;
            PositionBoss();
        }

        private void PositionBoss()
        {
            _position.X = (byte)(WorldSprite.X - _tileModule.Scroll.X - 32);
            _position.Y = (byte)(WorldSprite.Y + 56);
            UpdatePartPositions();

            if (_phase.Value >= Phase.Attack1)
            {
                UpdateJaw(_leftJaw, -8, -18);
                UpdateJaw(_rightJaw, 8,12);
            }
        }

        private void UpdateJaw(ChompTail jaw, int xOffset1, int xOffset2)
        {
            int yOffset = 16;

            var startAnchor = new Point(WorldSprite.X + xOffset1, WorldSprite.Y + yOffset);
            var endAnchor = new Point(
                WorldSprite.X + _bossBackgroundHandler.FinalBossLowerX + xOffset2, 
                WorldSprite.Y + yOffset + 4 + _bossBackgroundHandler.FinalBossLowerY+ 4);
                       
            for (int i = 0; i < JawParts; i++)
            {
                var jawPart = jaw.GetWorldSprite(i);
                float t = (float)(i+1) / (JawParts + 1);

                // Interpolate the position between startAnchor and endAnchor
                int interpolatedX = (int)(startAnchor.X + t * (endAnchor.X - startAnchor.X));
                int interpolatedY = (int)(startAnchor.Y + t * (endAnchor.Y - startAnchor.Y));

                // Update the position of the jaw part
                jawPart.X = interpolatedX;
                jawPart.Y = interpolatedY;
            }
        }

        private void CreateRandomAimedBullet()
        {
            var bullet = _bulletControllers.TryAddNew() as Boss7BulletController;
            if (bullet == null)
                return;

           
            // Add randomness to the bullet's trajectory
            var randomOffsetX = -8 + (_rng.Generate(4)*2);
            var randomOffsetY = -16 + _rng.Generate(4);

            bullet.WorldSprite.Center = WorldSprite.Center.Add(randomOffsetX, randomOffsetY);
            bullet.Mode = Boss7BulletController.BulletMode.RandomAimed;


            _audioService.PlaySound(ChompAudioService.Sound.Fireball);
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
         //   _paletteModule.FadeColor(bossPalette, targetBossPalette, 3);

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
            if (_phase.Value == Phase.Dying)
                return BombCollisionResponse.None;

            _audioService.PlaySound(ChompAudioService.Sound.Break);         
            _hitPoints.Value--;

            if(_phase.Value < Phase.BeginPhase2)
            {
                if (_hitPoints.Value == 0)
                    SetPhase(Phase.Transition);
                else
                    SetPhase(Phase.Hurt);

                return BombCollisionResponse.Destroy;
            }
            else
            {
                if (_hitPoints.Value == 0)
                {
                    if (_phase.Value < Phase.Attack2)
                    {
                        _hitPoints.Value = (byte)BossHP;
                        SetPhase(Phase.Attack2);
                    }
                    else
                    {
                        SetPhase(Phase.Dying);
                        return BombCollisionResponse.Destroy;
                    }
                }
                 
                var bossPalette = _paletteModule.BgPalette2;
                bossPalette.SetColor(1, ColorIndex.Red3);
                bossPalette.SetColor(2, ColorIndex.Red2);
                bossPalette.SetColor(3, ColorIndex.Red1);
                return BombCollisionResponse.Bounce;
            }
        }            

        private void SetBossColor(byte color)
        {
            var bossPalette = _paletteModule.BgPalette2;
            bossPalette.SetColor(1, color);
            bossPalette.SetColor(2, color);
            bossPalette.SetColor(3, color);
        }

        public override bool CollidesWithBomb(WorldSprite bomb)
        {
            switch (_phase.Value)
            {
                case Phase.EyeAttack:
                case Phase.Attack1:
                case Phase.BeamAttack:
                case Phase.Attack2:
                case Phase.BeamAttack2:
                    return bomb.Bounds.Intersects(_eye1.WorldSprite.Bounds) || bomb.Bounds.Intersects(_eye2.WorldSprite.Bounds);
                default:
                    return false;
            }
        }

        public override bool CollidesWithPlayer(PlayerController player)
        {
            //if (_phase.Value <= Phase.BossAppear)
            //    return false;

            return false; //todo
        }
    }
}
