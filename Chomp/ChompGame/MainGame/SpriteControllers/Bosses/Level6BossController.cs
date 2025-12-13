using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Graphics;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers.Bosses
{
    class Level6BossController : LevelBossController
    {
        private NibbleEnum<Phase> _phase;
        private BossPart _eye1, _eye2, _eye3, _eye4;
        private BossPart _leg1, _leg2, _leg3, _leg4, _leg5, _leg6;
        private DenseTwoBitArray _legPos;
      
        protected override bool AlwaysActive => true;
        protected override bool DestroyWhenFarOutOfBounds => false;
        protected override bool DestroyWhenOutOfBounds => false;

        private GameBit _forceScrollOn;
        private MaskedByte _scrollLock;

        public Level6BossController(ChompGameModule gameModule, WorldSprite player, EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers, SystemMemoryBuilder memoryBuilder) : base(gameModule, player, bulletControllers, memoryBuilder)
        {
            _phase = new NibbleEnum<Phase>(new LowNibble(memoryBuilder));           
            memoryBuilder.AddByte();
          
            _eye1 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _eye2 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _eye3 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _eye4 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));

            _leg1 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossLeg, memoryBuilder.Memory));
            _leg2 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossLeg, memoryBuilder.Memory));
            _leg3 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossLeg, memoryBuilder.Memory));
            _leg4 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossLeg, memoryBuilder.Memory));
            _leg5 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossLeg, memoryBuilder.Memory));
            _leg6 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossLeg, memoryBuilder.Memory));

            _forceScrollOn = new GameBit(_worldScroller.Extra.Address, Bit.Bit7, memoryBuilder.Memory);
            _scrollLock = new MaskedByte(_worldScroller.Extra.Address, Bit.Right6, memoryBuilder.Memory);

            _legPos = new DenseTwoBitArray(memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            memoryBuilder.AddBytes(2);
        }

        private enum Phase : byte
        {
            BeforeBoss,
            GroundFall,
            ForceScroll,
            BossAppear,
            Attack,
            ForceScrollChase,
            Hurt,
            Dying1,
            Dying2
        }

        protected override int BossHP => GameDebug.BossTest ? 1 : 5;

        protected override string BossTiles { get; } =
            @"08AAAAAC00000
              8IIIIIIIAC000
              GIIIIIIIII900
              8IIIIIFFFF400
              GIIII90000000
              8IIIIIAAAA900
              GIIIIIIIII400
              03IIIIIF45000
              006FFF4000000";
           
        protected override void UpdatePartPositions()
        {

            _eye1.UpdatePosition(WorldSprite);
            _eye2.UpdatePosition(WorldSprite);
            _eye3.UpdatePosition(WorldSprite);
            _eye4.UpdatePosition(WorldSprite);
            _leg1.UpdatePosition(WorldSprite);
            _leg2.UpdatePosition(WorldSprite);
            _leg3.UpdatePosition(WorldSprite);
            _leg4.UpdatePosition(WorldSprite);
            _leg5.UpdatePosition(WorldSprite);
            _leg6.UpdatePosition(WorldSprite);
        }

        protected override string BlankBossTiles => "0";

        protected override void BeforeInitializeSprite()
        {
            SetPhase(Phase.BeforeBoss);
            base.BeforeInitializeSprite();
        }

        private void SetPhase(Phase p)
        {
            _phase.Value = p;
            _stateTimer.Value = 0;

            if(p == Phase.BeforeBoss)
            {
                _gameModule.BossBackgroundHandler.ShowCoins = false;
                _paletteModule.BgColor = ColorIndex.Black;
                HideBoss();
                _scrollLock.Value = 6;
            }
            else if(p == Phase.BossAppear)
            {
                _musicModule.CurrentSong = GameSystem.MusicModule.SongName.Nemesis;
                SetBossTiles();              
                SetupBossParts();
                WorldSprite.Visible = false;
                WorldSprite.X = 320;
                WorldSprite.Y = 88;

                _motion.SetYSpeed(0);
                _motion.SetXSpeed(20);
            }    
            else if(p == Phase.ForceScroll)
            {
                _musicModule.CurrentSong = GameSystem.MusicModule.SongName.Nemesis;
                _scrollLock.Value = 45;
                _forceScrollOn.Value = true;
            }
            else if(p == Phase.Attack)
            {
                _bossBackgroundHandler.ShowCoins = true;
            }
            else if (p == Phase.Hurt)
            {
                _motion.TargetXSpeed = 0;
                _motion.TargetYSpeed = 0;

                var bossPalette = _paletteModule.BgPalette2;
                bossPalette.SetColor(1, ColorIndex.Red3);
                bossPalette.SetColor(2, ColorIndex.Red2);
                bossPalette.SetColor(3, ColorIndex.Red1);
            }
            else if (p == Phase.ForceScrollChase)
            {
                _bossBackgroundHandler.ShowCoins = false;
                _dynamicBlockController.ResetCoinsForLevelBoss();
                _scrollLock.Value = 45;
                _forceScrollOn.Value = true;
            }
        }

        private void RepositionScreen()
        {
            var playerScreenX = _player.X - _worldScroller.ViewPane.Left;
            var bossScreenX = WorldSprite.X - _worldScroller.ViewPane.Left;
            _forceScrollOn.Value = false;
            _scrollLock.Value = 1;

            _player.X = playerScreenX;
            WorldSprite.X = bossScreenX;

            _worldScroller.ModifyTiles((t, _) =>
            {
                int bottom = t.Height - 1;

                for (int x = 0; x < t.Width; x++)
                {
                    if (x < 6)
                    {
                        t[x, bottom] = 0;
                        t[x, bottom - 1] = 0;
                    }
                    else
                    {
                        int tile2 = x.IsMod(2) ? 9 : 8;

                        t[x, bottom - 1] = 11;
                        t[x, bottom] = (byte)tile2;
                    }
                }
            });
        }

        private void SetupBossParts()
        {
            var eye1Sprite = _eye1.PrepareSprite(SpriteTileIndex.Enemy1);
            eye1Sprite.Tile2Offset = 1;
            eye1Sprite.FlipX = true;
            eye1Sprite.FlipY = false;
            _eye1.XOffset = 0;
            _eye1.YOffset = -8;
            eye1Sprite.Visible = true;

            var eye2Sprite = _eye2.PrepareSprite(SpriteTileIndex.Enemy1);
            eye2Sprite.Tile2Offset = 1;
            eye2Sprite.FlipX = true;
            eye2Sprite.FlipY = false;
            _eye2.XOffset = 9;
            _eye2.YOffset = -5;
            eye2Sprite.Visible = true;

            var eye3Sprite = _eye3.PrepareSprite(SpriteTileIndex.Enemy1);
            eye3Sprite.Tile2Offset = 1;
            eye3Sprite.FlipX = true;
            eye3Sprite.FlipY = true;
            _eye3.XOffset = 0;
            _eye3.YOffset = 8;
            eye1Sprite.Visible = true;

            var eye4Sprite = _eye4.PrepareSprite(SpriteTileIndex.Enemy1);
            eye4Sprite.Tile2Offset = 1;
            eye4Sprite.FlipX = true;
            eye4Sprite.FlipY = true;
            _eye4.XOffset = 8;
            _eye4.YOffset = 4;
            eye4Sprite.Visible = true;


            var leg1Sprite = _leg1.PrepareSprite(SpriteTileIndex.Enemy2);
            leg1Sprite.Tile2Offset = 1;
            leg1Sprite.FlipX = false;
            leg1Sprite.FlipY = false;
            leg1Sprite.Priority = false;
            _leg1.XOffset = 2;
            _leg1.YOffset = 16;
            leg1Sprite.Visible = true;

            var leg2Sprite = _leg2.PrepareSprite(SpriteTileIndex.Enemy2);
            leg2Sprite.Tile2Offset = 1;
            leg2Sprite.FlipX = false;
            leg2Sprite.FlipY = false;
            leg2Sprite.Priority = false;
            _leg2.XOffset = 8;
            _leg2.YOffset = 20;
            leg2Sprite.Visible = true;

            var leg3Sprite = _leg3.PrepareSprite(SpriteTileIndex.Enemy2);
            leg3Sprite.Tile2Offset = 1;
            leg3Sprite.FlipX = false;
            leg3Sprite.FlipY = false;
            leg3Sprite.Priority = false;
            _leg3.XOffset = 16;
            _leg3.YOffset = 21;
            leg3Sprite.Visible = true;

            var leg4Sprite = _leg4.PrepareSprite(SpriteTileIndex.Enemy2);
            leg4Sprite.Tile2Offset = 1;
            leg4Sprite.FlipX = false;
            leg4Sprite.FlipY = true;
            leg4Sprite.Priority = false;
            _leg4.XOffset = 2;
            _leg4.YOffset = -16;
            leg4Sprite.Visible = true;

            var leg5Sprite = _leg5.PrepareSprite(SpriteTileIndex.Enemy2);
            leg5Sprite.Tile2Offset = 1;
            leg5Sprite.FlipX = false;
            leg5Sprite.FlipY = true;
            leg5Sprite.Priority = false;
            _leg5.XOffset = 8;
            _leg5.YOffset = -20;
            leg5Sprite.Visible = true;

            var leg6Sprite = _leg6.PrepareSprite(SpriteTileIndex.Enemy2);
            leg6Sprite.Tile2Offset = 1;
            leg6Sprite.FlipX = false;
            leg6Sprite.FlipY = true;
            leg6Sprite.Priority = false;
            _leg6.XOffset = 16;
            _leg6.YOffset = -18;
            leg6Sprite.Visible = true;

        }

        protected override void UpdateActive()
        {
            if(_phase.Value >= Phase.BossAppear)
                UpdateLegTargets();

            if(_phase.Value == Phase.BeforeBoss)
            {
                WorldSprite.Visible = false;

                if (_player.X > 64)
                {
                    _gameModule.RewardsModule.GiveHealth(_gameModule.SceneSpriteControllers);
                    SetPhase(Phase.GroundFall);
                }
            }
            else if(_phase.Value == Phase.GroundFall)
            {
                if(_levelTimer.IsMod(16))
                {
                    _worldScroller.ModifyTiles(DestroyNextGround);
                    _stateTimer.Value++;
                }

                if (_stateTimer.Value == 12)
                    SetPhase(Phase.ForceScroll);
            }
            else if(_phase.Value == Phase.ForceScroll)
            {
                if (_levelTimer.IsMod(18))
                    RainBullet();

                if(_levelTimer.IsMod(24))
                {
                    _worldScroller.ModifyTiles(RefreshTilesForForcedScroll);
                    _stateTimer.Value++;
                }

                if (_player.X >= 400)
                    SetPhase(Phase.BossAppear);
            }
            else if(_phase.Value == Phase.BossAppear)
            {
                _motionController.Update();
                FadeIn();

                PositionBoss();

                if (WorldSprite.X > 360)
                {
                    _motion.TargetXSpeed = 0;
                    _motion.XAcceleration = 2;
                }

                if (_motion.XSpeed == 0)
                {
                    RepositionScreen();
                    SetPhase(Phase.Attack);
                }
            }
            else if(_phase.Value == Phase.Attack)
            {
                PositionFreeCoinBlocksNearPlayer();

                BossMotion(0, 88);

                _motion.YAcceleration = 1;
                _motion.XAcceleration = 1;

                _motionController.Update();
                PositionBoss();

                if(_stateTimer.Value < 6)
                {
                    if (_levelTimer.IsMod(8))
                    {
                        _stateTimer.Value++;
                        FireFastBullet();
                    }
                }
                else if (_stateTimer.Value < 11)
                {
                    if (_levelTimer.IsMod(16))
                    {
                        _stateTimer.Value++;
                    }
                }
                else
                {
                    if (_levelTimer.IsMod(24))
                    {
                        _stateTimer.Value++;
                        FireBulletFromTop();
                    }
                }

            }
            else if (_phase.Value == Phase.Hurt)
            {
                PositionBoss();
                if (_levelTimer.IsMod(8))
                {
                    FadeIn();
                    _stateTimer.Value++;
                }

                if (_stateTimer.Value == 10)
                {
                    if (_hitPoints.Value == 3)
                        SetPhase(Phase.ForceScrollChase);
                    else
                        SetPhase(Phase.Attack);
                }
            }
            else if (_phase.Value == Phase.ForceScrollChase)
            {
                if (_levelTimer.IsMod(8))
                {
                    _stateTimer.Value++;
                    if(_stateTimer.Value <= 4)
                    {
                        FireArcBullet();
                    }
                }

                if (_levelTimer.IsMod(24))
                {
                    _worldScroller.ModifyTiles(RefreshTilesForForcedScroll);
                    _stateTimer.Value++;
                }

                BossMotion(_worldScroller.ViewPane.Left - 4, 80);
                
                _motionController.Update();
                PositionBoss();

                if (_player.X >= 400)
                {
                    RepositionScreen();
                    SetPhase(Phase.Attack);
                }
            }
        }

        private void BossMotion(int targetX, int targetY)
        {
            if (_levelTimer.IsMod(16))
            {
                if (WorldSprite.Y < targetY)
                    _motion.TargetYSpeed = 10;
                else
                    _motion.TargetYSpeed = -10;
            }

            if (_levelTimer.IsMod(24))
            {
                if (WorldSprite.X < targetX)
                    _motion.TargetXSpeed = 20;
                else
                    _motion.TargetXSpeed = -10;
            }
        }

        private void UpdateLegTargets()
        {
            if (_levelTimer.IsMod(32))
            {
                for (int i = 0; i < 6; i++)
                {
                    _legPos[i] = _rng.Generate(2);
                }
            }

            if (_levelTimer.IsMod(2))
            {
                UpdateLeg(_leg1, 16, _legPos[0]);
                UpdateLeg(_leg2, 20, _legPos[1]);
                UpdateLeg(_leg3, 21, _legPos[2]);
                UpdateLeg(_leg4, -16, _legPos[3]);
                UpdateLeg(_leg5, -20, _legPos[4]);
                UpdateLeg(_leg6, -16, _legPos[5]);
            }
        }

        private void UpdateLeg(BossPart leg, int baseY, byte offset)
        {            
            int target = baseY + offset;
            if (leg.YOffset < target)
                leg.YOffset++;
            else if (leg.YOffset > target)
                leg.YOffset--;
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

        private void FireBulletFromTop()
        {
            var bullet = _bulletControllers.TryAddNew();
            if (bullet == null)
                return;

            bullet.DestroyOnCollision = true;
            bullet.DestroyOnTimer = true;

            bullet.WorldSprite.X = 24 + (4 * _rng.Generate(4));
            bullet.WorldSprite.Y = 64;


            bullet.AcceleratedMotion.SetYSpeed(60);
            bullet.AcceleratedMotion.SetXSpeed(2);
        }

        private void FireFastBullet()
        {
            var bullet = _bulletControllers.TryAddNew();
            if (bullet == null)
                return;

            bullet.DestroyOnCollision = true;
            bullet.DestroyOnTimer = true;

            bullet.WorldSprite.X = WorldSprite.X + 16 + _rng.Generate(3)*2;
            bullet.WorldSprite.Y = WorldSprite.Y + 4;
            _audioService.PlaySound(ChompAudioService.Sound.Fireball);


            bullet.AcceleratedMotion.SetYSpeed(-100);
            bullet.AcceleratedMotion.SetXSpeed(0);
        }

        private void FireArcBullet()
        {
            var bullet = _bulletControllers.TryAddNew();
            if (bullet == null)
                return;

            bullet.DestroyOnCollision = true;
            bullet.DestroyOnTimer = true;

            bullet.WorldSprite.X = WorldSprite.X + 16;
            bullet.WorldSprite.Y = WorldSprite.Y + 4;
            _audioService.PlaySound(ChompAudioService.Sound.Fireball);


            bullet.AcceleratedMotion.YAcceleration = 3;
            bullet.AcceleratedMotion.TargetYSpeed = 80;

            bullet.AcceleratedMotion.SetXSpeed(10 + (5 * _rng.Generate(4)));
        }


        private void RefreshTilesForForcedScroll(NBitPlane tileMap, NBitPlane attributeMap)
        {
            int screenTileLeft = ((_worldScroller.ViewPane.Left / _gameModule.Specs.TileWidth)-0).NMod(tileMap.Width);
            int screenTileRight = (_worldScroller.ViewPane.Right / _gameModule.Specs.TileWidth).NMod(tileMap.Width);

            int column = (screenTileRight + 1).NMod(tileMap.Width);
            int bottom = tileMap.Height - 1;

            int pit = 0;

            while (column != screenTileLeft)
            {
                int tile2 = column.IsMod(2) ? 9 : 8;

                tileMap[column, bottom - 1] = 11;
                tileMap[column, bottom] = (byte)tile2;
                
                column = (column + 1).NMod(tileMap.Width);               
            }

            _audioService.PlaySound(ChompAudioService.Sound.Break);

            tileMap[screenTileLeft, bottom] = 0;
            tileMap[screenTileLeft + 1, bottom] = 0;
            tileMap[screenTileLeft, bottom - 1] = 0;
            tileMap[screenTileLeft + 1, bottom - 1] = 0;

            tileMap[screenTileLeft + 2, bottom] = 0;
            tileMap[screenTileLeft + 3, bottom] = 0;
            tileMap[screenTileLeft + 2, bottom - 1] = 0;
            tileMap[screenTileLeft + 3, bottom - 1] = 0;



            return;

        }

        private void DestroyNextGround(NBitPlane tileMap, NBitPlane attributeMap)
        {
          //  int screenTileLeft = _worldScroller.ViewPane.Left / _gameModule.Specs.TileWidth;

            int bottom = tileMap.Height-1;

            for(int x = 0; x < tileMap.Width; x += 2)
            {
                if (tileMap[x, bottom] != 0)
                {
                    _audioService.PlaySound(ChompAudioService.Sound.Break);

                    tileMap[x, bottom] = 0;
                    tileMap[x+1, bottom] = 0;
                    tileMap[x, bottom-1] = 0;
                    tileMap[x+1, bottom-1] = 0;
                    return;
                }
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

        protected override void UpdateDying()
        {
            if (_phase.Value < Phase.Dying1)
            {
                SetPhase(Phase.Dying1);
                _statusBar.AddToScore((uint)PointsForEnemy);
                _musicModule.CurrentSong = GameSystem.MusicModule.SongName.None;
            }

            PositionBoss();

            if (_phase.Value == Phase.Dying1)
            {
                if (_levelTimer.IsMod(4))
                {
                    FadeIn();
                    CreateExplosion();
                }

                if (_levelTimer.IsMod(16))
                {
                    var bossPalette = _paletteModule.BgPalette2;
                    bossPalette.SetColor(1, ColorIndex.Red3);
                    bossPalette.SetColor(2, ColorIndex.Red2);
                    bossPalette.SetColor(3, ColorIndex.Red1);

                    _stateTimer.Value++;

                    switch(_stateTimer.Value)
                    {
                        case 1:
                            _leg1.Sprite.Visible = false;
                            break;
                        case 7:
                            _leg2.Sprite.Visible = false;
                            break;
                        case 3:
                            _leg3.Sprite.Visible = false;
                            break;
                        case 15:
                            _leg4.Sprite.Visible = false;
                            break;
                        case 5:
                            _leg5.Sprite.Visible = false;
                            break;
                        case 6:
                            _leg6.Sprite.Visible = false;
                            break;
                        case 14:
                            _eye1.Sprite.Visible = false;
                            break;
                        case 8:
                            _eye2.Sprite.Visible = false;
                            break;
                        case 9:
                            _eye3.Sprite.Visible = false;
                            break;
                        case 10:
                            _eye4.Sprite.Visible = false;
                            break;
                    }

                    if (_stateTimer.Value == 0)
                        SetPhase(Phase.Dying2);
                }
            }
            else
            {
                _bossBackgroundHandler.BossBgEffectType = BackgroundEffectType.DissolveFromBottom;

                if(_levelTimer.Value.IsMod(4))
                    _bossBackgroundHandler.BossBgEffectValue++;

                if(_stateTimer.Value <= 5)
                {
                    if (_levelTimer.IsMod(16))
                    {
                        _stateTimer.Value++;

                        _audioService.PlaySound(ChompAudioService.Sound.Lightning);
                    }
                }
                else
                {
                    if (_levelTimer.IsMod(32))
                    {
                        _stateTimer.Value++;

                        if (_stateTimer.Value == 0)
                            _exitsModule.GotoNextLevel();
                    }
                }
            }
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

            if(_hitPoints.Value == 0)
                WorldSprite.Status = WorldSpriteStatus.Dying;
            else
                SetPhase(Phase.Hurt);
         
            return BombCollisionResponse.Destroy;
        }

        public override bool CollidesWithBomb(WorldSprite bomb)
        {
            if (_phase.Value <= Phase.BossAppear)
                return false;

            if (_phase.Value == Phase.ForceScrollChase)
                return false;

            return bomb.Bounds.Intersects(_eye1.WorldSprite.Bounds) || bomb.Bounds.Intersects(_eye2.WorldSprite.Bounds) ||
                   bomb.Bounds.Intersects(_eye3.WorldSprite.Bounds) || bomb.Bounds.Intersects(_eye4.WorldSprite.Bounds);
        }

        public override bool CollidesWithPlayer(PlayerController player)
        {
            if (_phase.Value <= Phase.BossAppear)
                return false;

            return false; //todo
        }
    }
}
