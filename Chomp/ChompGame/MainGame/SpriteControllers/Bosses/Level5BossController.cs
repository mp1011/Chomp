using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Graphics;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SpriteControllers.Bosses
{
    class Level5BossController : LevelBossController
    {
        public const int MaxBullets = 5;
        public const int BulletRadiusMin = 16;
        public const int BulletRadiusMax = 20;
        public const int GroundY = 160;

        private NibbleEnum<Phase> _phase;
        private GameShort _bulletAngle;
        private BossPart _eye1, _eye2, _horn1, _horn2, _horn3, _horn4;
        private NibbleArray _bullets;
        private ByteVector _orbitCenter;
        private GameBit _orbitDirection;
        private MaskedByte _stunTimer;

        protected override bool AlwaysActive => true;
        protected override bool DestroyWhenFarOutOfBounds => false;
        protected override bool DestroyWhenOutOfBounds => false;

        public Level5BossController(ChompGameModule gameModule, WorldSprite player, EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers, SystemMemoryBuilder memoryBuilder) : base(gameModule, player, bulletControllers, memoryBuilder)
        {
            _phase = new NibbleEnum<Phase>(new LowNibble(memoryBuilder));
            _orbitDirection = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit4, memoryBuilder.Memory);
            _stunTimer = new MaskedByte(memoryBuilder.CurrentAddress, Bit.Left3, memoryBuilder.Memory, 5);

            memoryBuilder.AddByte();
            _bulletAngle = memoryBuilder.AddShort();

            _eye1 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _eye2 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _horn1 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossHorn, memoryBuilder.Memory));
            _horn2 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossHorn, memoryBuilder.Memory));
            _horn3 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossHorn, memoryBuilder.Memory));
            _horn4 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossHorn, memoryBuilder.Memory));



            _orbitCenter = new ByteVector(memoryBuilder.AddByte(), memoryBuilder.AddByte());
            _bullets = new NibbleArray(memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            memoryBuilder.AddBytes(MaxBullets/2);

        }

        private enum Phase : byte
        {
            BeforeBoss,
            Init,
            Rise,
            BuildOrbit,
            Chase,
            LaunchOrbit,
            Attack2,
            Drop,
            Dying,
            Dying2
        }

        protected override int BossHP => GameDebug.BossTest ? 1 : 3;

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

            for(int i =0;i<MaxBullets;i++)
            {
                _bullets[i] = 0;
            }
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
                _musicModule.CurrentSong = GameSystem.MusicModule.SongName.Nemesis;
                SetBossTiles();              
                SetupBossParts();
                WorldSprite.Visible = false;
            }
            else if(p == Phase.Rise)
            {
                if(_player.X < 16)
                    WorldSprite.X = _player.X + 24;
                else if(_player.X > _gameModule.Specs.NameTablePixelWidth - 16)
                    WorldSprite.X = _player.X - 64;
                else
                {
                    if(_rng.Generate(1) == 0)
                        WorldSprite.X = _player.X + 16;
                    else
                        WorldSprite.X = _player.X - 20;
                }


                WorldSprite.Y = 140;
                _motion.TargetYSpeed = 16;
                _motion.YAcceleration = 3;
                _motion.YSpeed = -80;
            }
            else if(p == Phase.BuildOrbit)
            {
                _motion.TargetYSpeed = 0;
            }
            else if (p == Phase.LaunchOrbit)
            {
                _motion.TargetXSpeed = 0;
                _motion.YSpeed = -30;
                _motion.YAcceleration = 1;
                _motion.TargetYSpeed = 0;
            }
            else if(p == Phase.Dying2)
            {

                _bossBackgroundHandler.BossBgEffectValue = 0;
                _bossBackgroundHandler.BossBgEffectType = BackgroundEffectType.SineWave;
                _eye1.Sprite.Visible = false;
                _eye2.Sprite.Visible = false;
                _horn1.Sprite.Visible = false;
                _horn2.Sprite.Visible = false;
                _horn3.Sprite.Visible = false;
                _horn4.Sprite.Visible = false;

                var bossPalette = _paletteModule.BgPalette2;
                bossPalette.SetColor(1, ColorIndex.Yellow3);
                bossPalette.SetColor(2, ColorIndex.Yellow4);
                bossPalette.SetColor(3, ColorIndex.Yellow5);
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
            {
                if(_player.X > 32)
                    SetPhase(Phase.Init);
            }

            if (_phase.Value == Phase.Init)
            {
                WorldSprite.X = 16;
                WorldSprite.Y = 140;

                FadeIn();

                _stateTimer.Value++;
                if (_stateTimer.Value == 15)
                    SetPhase(Phase.Rise);
            }
            else if (_phase.Value == Phase.Rise)
            {
                FadeIn();
                if (WorldSprite.Y >= 91 && _motion.YSpeed > 0)
                    SetPhase(Phase.BuildOrbit);
            }
            else if (_phase.Value == Phase.BuildOrbit)
            {
                _orbitCenter.X = WorldSprite.X + 6;
                _orbitCenter.Y = WorldSprite.Y - 2;

                if (_levelTimer.IsMod(32))
                {
                    if (WorldSprite.X > _player.X)
                        _motion.TargetXSpeed = -40;
                    else
                        _motion.TargetXSpeed = 40;

                    _motion.XAcceleration = 4;

                    if (_stateTimer.Value < MaxBullets)
                        CreateBullet(_stateTimer.Value);

                    _stateTimer.Value++;
                    if (_stateTimer.Value == 10)
                        SetPhase(Phase.Chase);
                }

                BulletOrbit(2, BulletRadiusMin);
            }
            else if (_phase.Value == Phase.Chase)
            {
                _orbitCenter.X = WorldSprite.X + 6;
                _orbitCenter.Y = WorldSprite.Y - 2;

                if (WorldSprite.X < 16)
                    _motion.TargetXSpeed = 40;
                else if(WorldSprite.X > 90)
                    _motion.TargetXSpeed = -40;

                if (WorldSprite.X < _worldScroller.ViewPane.Left)
                    _motion.TargetXSpeed = 40;
                else if (WorldSprite.X > _worldScroller.ViewPane.Right)
                    _motion.TargetXSpeed = -40;

                 _motion.XAcceleration = 4;

                if (_stateTimer.Value < 8)
                {
                    if (_levelTimer.IsMod(8))
                        _stateTimer.Value++;
                }
                else
                {
                    if(_levelTimer.IsMod(32))
                    {
                        _stateTimer.Value++;
                        if (_stateTimer.Value == 0)
                            SetPhase(Phase.LaunchOrbit);
                    }
                }
                
                var radius = (BulletRadiusMin + _stateTimer.Value).Clamp(0, BulletRadiusMax);

                if(_motion.TargetXSpeed > 0)
                    BulletOrbit(-3, radius);
                else
                    BulletOrbit(3, radius);

            }
            else if (_phase.Value == Phase.LaunchOrbit)
            {
                PositionFreeCoinBlocksNearPlayer();
                _bossBackgroundHandler.ShowCoins = true;

                if (_motion.YSpeed != 0)
                {
                    _orbitDirection.Value = _player.X > _orbitCenter.X;
                    _orbitCenter.X = WorldSprite.X + 6;
                    _orbitCenter.Y = WorldSprite.Y - 2;
                    BulletOrbit(2, BulletRadiusMax);
                }
                else 
                {
                    BulletOrbit(2, BulletRadiusMax);

                    if (_levelTimer.IsMod(2))
                    {
                        _orbitCenter.Y++;
                    }

                    if (_levelTimer.IsMod(2))
                    {
                        if (_orbitDirection.Value)
                            _orbitCenter.X++;
                        else
                            _orbitCenter.X--;
                    }

                    bool anyLeft = false;

                    _bulletControllers.Execute(b =>
                    {
                        if (b.WorldSprite.Y > 114)
                            b.Explode();
                        else
                            anyLeft = true;
                    });

                    if (!anyLeft)
                        SetPhase(Phase.Attack2);
                }
            }
            else if(_phase.Value == Phase.Attack2)
            {
                if(_levelTimer.IsMod(16))
                {
                    if(_stateTimer.Value < 8)
                        CreateAimedBullet();
                    _stateTimer.Value++;

                    if (_stateTimer.Value == 0)
                        SetPhase(Phase.Drop);
                }
            }
            else if(_phase.Value == Phase.Drop)
            {
                _motion.TargetYSpeed = 80;
                _motion.YAcceleration = 2;

                if (WorldSprite.Y >= 100)
                {
                    _bossBackgroundHandler.ShowCoins = false;
                    _dynamicBlockController.ResetCoinsForLevelBoss();
                }

                if (WorldSprite.Y >= GroundY)
                {
                    WorldSprite.Y = GroundY;
                    if(_levelTimer.IsMod(8))
                    {
                        _stateTimer.Value++;
                        if (_stateTimer.Value == 0)
                            SetPhase(Phase.Rise);
                    }
                }
            }

            if (_phase.Value >= Phase.Init)
            {
                _position.X = (byte)(WorldSprite.X + 123 - _tileModule.Scroll.X);
                _position.Y = (byte)(WorldSprite.Y - 77);
                _bossBackgroundHandler.BossBgEffectType = BackgroundEffectType.VerticalStretch;

                if (_levelTimer.Value.IsMod(4))
                {
                    _bossBackgroundHandler.BossBgEffectValue++;
                    if (_bossBackgroundHandler.BossBgEffectValue == 16)
                        _bossBackgroundHandler.BossBgEffectValue = 1;

                    var modVal = _bossBackgroundHandler.BossBgEffectValue;
                    if (modVal > 8)
                        modVal = (byte)(modVal - 2 * (modVal - 8));

                    _horn3.YOffset = 10 - modVal;
                    _horn4.YOffset = 10 - modVal;

                    _eye1.YOffset = (modVal / 2)-8;
                    _eye2.YOffset = (modVal / 2)-8;

                }
                UpdatePartPositions();

                _motionController.Update();
            }

        }

        private void BulletOrbit(int increase, int radius)
        {
            _bulletAngle.Value = (ushort)(_bulletAngle.Value + increase).NMod(360);
            PlaceBullets(radius);
        }

        private BossBulletController CreateBullet(int index)
        {
            _audioService.PlaySound(ChompAudioService.Sound.Fireball);

            var bulletAndIndex = _bulletControllers.TryAddNewWithIndex();
            var bullet = bulletAndIndex.Item1;
            if (bullet == null)
                return null;

            bullet.DestroyOnTimer = false;
            bullet.DestroyOnCollision = true;

            _bullets[index] = (byte)(bulletAndIndex.Item2+1);
            return bullet;
        }

        private BossBulletController CreateAimedBullet()
        {
            _audioService.PlaySound(ChompAudioService.Sound.Fireball);

            var bullet = _bulletControllers.TryAddNew();
            if (bullet == null)
                return null;

            bullet.DestroyOnTimer = false;
            bullet.DestroyOnCollision = true;

            bullet.WorldSprite.X = WorldSprite.X + 8;
            bullet.WorldSprite.Y = WorldSprite.Y + 8;

            bullet.AcceleratedMotion.YAcceleration = 3;
            bullet.AcceleratedMotion.XAcceleration = 3;
            bullet.AcceleratedMotion.TargetTowards(bullet.WorldSprite, _player.Bounds.Center, 50);

            return bullet;
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
            _position.X = (byte)(WorldSprite.X + 123 - _tileModule.Scroll.X);
            _position.Y = (byte)(WorldSprite.Y - 77);
            UpdatePartPositions();

            if(_levelTimer.IsMod(2))
            {
                _eye1.Sprite.Palette = SpritePalette.Enemy1;
                _eye2.Sprite.Palette = SpritePalette.Enemy1;
            }
            else
            {
                _eye1.Sprite.Palette = SpritePalette.Enemy2;
                _eye2.Sprite.Palette = SpritePalette.Enemy2;
            }

            if (!GameDebug.BossTest && _hitPoints.Value > 0)
            {
                if (_levelTimer.IsMod(4))
                {
                    _stunTimer.Value++;
                    if (_stunTimer.Value == 0)
                    {
                        WorldSprite.Status = WorldSpriteStatus.Active;
                        _eye1.Sprite.Palette = SpritePalette.Enemy1;
                        _eye2.Sprite.Palette = SpritePalette.Enemy1;
                    }
                }
            }
            else
            {
                if (_phase.Value < Phase.Dying)
                {
                    _statusBar.AddToScore((uint)PointsForEnemy);

                    SetPhase(Phase.Dying);

                    _bulletControllers.Execute(b =>
                    {
                        b.Destroy();
                    });
                }
                else if (_phase.Value == Phase.Dying)
                { 
                    if (_levelTimer.IsMod(4))
                    {
                        CreateExplosion();
                        _stateTimer.Value++;
                        if (_stateTimer.Value == 0)
                        {
                            SetPhase(Phase.Dying2);
                            return;
                        }
                    }

                    if (_levelTimer.IsMod(2))
                    {
                        _horn1.Sprite.Palette = SpritePalette.Enemy1;
                        _horn2.Sprite.Palette = SpritePalette.Enemy1;
                        _horn3.Sprite.Palette = SpritePalette.Enemy1;
                        _horn4.Sprite.Palette = SpritePalette.Enemy1;


                        var bossPalette = _paletteModule.BgPalette2;
                        bossPalette.SetColor(1, ColorIndex.Red1);
                        bossPalette.SetColor(2, ColorIndex.Red2);
                        bossPalette.SetColor(3, ColorIndex.Red3);
                    }
                    else
                    {
                        _horn1.Sprite.Palette = SpritePalette.Enemy2;
                        _horn2.Sprite.Palette = SpritePalette.Enemy2;
                        _horn3.Sprite.Palette = SpritePalette.Enemy2;
                        _horn4.Sprite.Palette = SpritePalette.Enemy2;

                        var bossPalette = _paletteModule.BgPalette2;
                        bossPalette.SetColor(1, ColorIndex.Yellow1);
                        bossPalette.SetColor(2, ColorIndex.Yellow2);
                        bossPalette.SetColor(3, ColorIndex.Yellow3);
                    }
                }
                else if(_phase.Value == Phase.Dying2)
                {
                    if(_levelTimer.IsMod(2))
                        _bossBackgroundHandler.BossBgEffectValue++;

                    if (_levelTimer.IsMod(16))
                    {
                        var bossPalette = _paletteModule.BgPalette2;
                        var c1 = _paletteModule.Darken(bossPalette, 1);
                        var c2 = _paletteModule.Darken(bossPalette, 2);
                        var c3 = _paletteModule.Darken(bossPalette, 3);

                        if(c1 && c2 && c3)
                        {
                            bossPalette.SetColor(1, ColorIndex.Black);
                            bossPalette.SetColor(2, ColorIndex.Black);
                            bossPalette.SetColor(3, ColorIndex.Black);

                            _stateTimer.Value++;

                            if (_stateTimer.Value == 15)
                                _exitsModule.GotoNextLevel();
                        }
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
            WorldSprite.Status = WorldSpriteStatus.Dying;
            _stunTimer.Value = 0;

            return BombCollisionResponse.Destroy;
        }

        public override bool CollidesWithBomb(WorldSprite bomb)
        {
            if (_phase.Value <= Phase.Init)
                return false;

            return bomb.Bounds.Intersects(_eye1.WorldSprite.Bounds) || bomb.Bounds.Intersects(_eye2.WorldSprite.Bounds);
        }

        public override bool CollidesWithPlayer(PlayerController player)
        {
            if (_phase.Value <= Phase.Init)
                return false;

            return false; //todo
        }

        private BossBulletController GetBulletController(int index) =>
            _bulletControllers.GetByIndex(_bullets[index]-1);

        private void PlaceBullets(int radius)
        {
            var center = new Point(_orbitCenter.X, _orbitCenter.Y);
            var angleMod = 360 / MaxBullets;

            for(int i = 0; i < MaxBullets; i++)
            {
                if (_bullets[i] == 0)
                    break;

                var bullet = GetBulletController(i);
                if (bullet.Status != WorldSpriteStatus.Active)
                    continue;

                Point offset = new Point(0, radius).RotateDeg(_bulletAngle.Value - (i*angleMod));
                bullet.WorldSprite.X = center.X + offset.X;
                bullet.WorldSprite.Y = center.Y + offset.Y;               
            }
        }
    }
}
