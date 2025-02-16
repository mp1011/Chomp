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

        private NibbleEnum<Phase> _phase;
        private GameShort _bulletAngle;
        private BossPart _eye1, _eye2, _horn1, _horn2, _horn3, _horn4;
        private NibbleArray _bullets;
        private ByteVector _orbitCenter;
        private GameBit _orbitDirection;

        protected override bool AlwaysActive => true;
        protected override bool DestroyWhenFarOutOfBounds => false;
        protected override bool DestroyWhenOutOfBounds => false;

        public Level5BossController(ChompGameModule gameModule, WorldSprite player, EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers, SystemMemoryBuilder memoryBuilder) : base(gameModule, player, bulletControllers, memoryBuilder)
        {
            _phase = new NibbleEnum<Phase>(new LowNibble(memoryBuilder));

            _orbitDirection = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit4, memoryBuilder.Memory);
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
            LaunchOrbit
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
                    WorldSprite.X = _player.X + 16;
                else if(_player.X > _gameModule.Specs.NameTablePixelWidth - 16)
                    WorldSprite.X = _player.X - 20;
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
                _orbitDirection.Value = _player.X > _orbitCenter.X;
                _motion.TargetXSpeed = 0;
                _motion.YSpeed = -20;
                _motion.YAcceleration = 4;
                _motion.TargetYSpeed = 80;
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
                WorldSprite.Y = 220;

                FadeIn();

                _stateTimer.Value++;
                if (_stateTimer.Value == 15)
                    SetPhase(Phase.Rise);
            }
            else if(_phase.Value == Phase.Rise)
            {
                FadeIn();
                if (WorldSprite.Y >= 91 && _motion.YSpeed > 0)
                    SetPhase(Phase.BuildOrbit);
            }
            else if(_phase.Value == Phase.BuildOrbit)
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

                    if(_stateTimer.Value < MaxBullets)
                        CreateBullet(_stateTimer.Value);

                    _stateTimer.Value++;
                    if(_stateTimer.Value == 10)
                        SetPhase(Phase.LaunchOrbit);
                }



                BulletOrbit(2);
            }
            else if(_phase.Value == Phase.LaunchOrbit)
            {
                PositionFreeCoinBlocksNearPlayer();
                _bossBackgroundHandler.ShowCoins = true;
                if(WorldSprite.Y >= 140)
                {
                    WorldSprite.Y = 140;
                    _motion.SetXSpeed(0);
                    _motion.SetYSpeed(0);

                    if(_levelTimer.IsMod(16))
                    {
                        _stateTimer.Value++;
                        if (_stateTimer.Value == 10)
                        {
                            _bossBackgroundHandler.ShowCoins = false;
                            _dynamicBlockController.ResetCoinsForLevelBoss();
                            SetPhase(Phase.Rise);
                        }
                    }
                }

                if (_levelTimer.IsMod(8))
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

                BulletOrbit(4);

                _bulletControllers.Execute(b =>
                {
                    if (b.WorldSprite.Y > 114)
                        b.Explode();
                });
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

        private void BulletOrbit(byte increase)
        {
            _bulletAngle.Value += increase;
            if (_bulletAngle.Value >= 360)
                _bulletAngle.Value = 0;

            PlaceBullets();
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

        private BossBulletController GetBulletController(int index) =>
            _bulletControllers.GetByIndex(_bullets[index]-1);

        private void PlaceBullets()
        {
            var center = new Point(_orbitCenter.X, _orbitCenter.Y);
            var radius = 16;

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
