using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;
using System.Security.Principal;

namespace ChompGame.MainGame.SpriteControllers.Bosses
{
    class Level2BossController : LevelBossController
    {
        protected override int BossHP => GameDebug.BossTest ? 1 : 4;
        private BossPart _eye2, _eye3, _eye4, _arm1, _arm2;
        private Tentacle _tentacle1, _tentacle2;
        private NibbleEnum<Phase> _phase;      
        private DenseTwoBitArray _partState;

        private LowNibble _bulletCounter;
        private TwoBit _eyeHit;
        private GameBit _tentacle1Hit;
        private GameBit _tentacle2Hit;

        private enum Phase : byte
        {
            BeforeBoss,
            Init,
            FadeIn,
            Sway,
            PrepareShoot,
            Shoot,
            Dying
        }

        class Tentacle
        {
            public const int NumSections = 6;
            private ChompTail _arm;
            private GameByte _curl;
            private Specs _specs;

            private GameBit _curlTarget;
            private MaskedByte _curlSpeed;
            private GameByte _levelTimer;

            public bool TargetRight
            {
                get => _curlTarget.Value;
                set => _curlTarget.Value = value;
            }

            //0-7
            public byte CurlSpeed
            {
                get => _curlSpeed.Value;
                set => _curlSpeed.Value = value;
            }

            public int Curl
            {
                get => _curl.Value - 128;
                set => _curl.Value = (byte)(value + 128);
            }

            public Sprite GetSprite(int index) => _arm.GetSprite(index);

            public Tentacle(SystemMemoryBuilder memoryBuilder, 
                ChompGameModule gameModule,
                GameBit curlTarget,
                MaskedByte curlSpeed,
                GameByte levelTimer)
            {
                _levelTimer = levelTimer;
                _specs = gameModule.Specs;
                _arm = new ChompTail(memoryBuilder, NumSections, gameModule);
                _curl = memoryBuilder.AddByte();
                _curlTarget = curlTarget;
                _curlSpeed = curlSpeed;
            }

            public void Initialize()
            {
                _arm.CreateTail(SpriteTileIndex.Enemy2);
                Curl = 0;
            }

            public void CurlBetween(int min, int max)
            {
                Update();
                if (Curl <= min)
                    _curlTarget.Value = true;
                else if (Curl >= max)
                    _curlTarget.Value = false;
            }

            public bool Update()
            {
                if (_curlSpeed == 0 || _levelTimer.IsMod(2))
                    return false;

                int newCurl = _curlTarget.Value ? (Curl + _curlSpeed.Value) : (Curl - _curlSpeed.Value);
                if (newCurl >= 127)
                {
                    Curl = 127;
                    return true;
                }
                else if(newCurl <= -128)
                {
                    Curl = -128;
                    return true;
                }

                Curl = newCurl;
                return false;
            }

            public void UpdatePosition(BossPart anchor)
            {
                var target = anchor.WorldSprite;
                for (int i = 0; i < NumSections; i++)
                    target = UpdateSection(target, _arm.GetWorldSprite(i), i);

            }

            private SimpleWorldSprite UpdateSection(SimpleWorldSprite anchor, SimpleWorldSprite section, int sectionNumber)
            {
                int mod = Curl / 4;

                var offset = new Point(0, 6);
                offset = offset.RotateDeg(Curl + sectionNumber * mod);
                section.X = anchor.X + offset.X;
                section.Y = anchor.Y + offset.Y;

                section.Sprite.Visible = section.X <= 256;
                return section;
            }

            public Point DestroyNextSection()
            {
                int index = NumSections - 1;

                while (_arm.IsErased(index))
                {
                    index--;
                    if (index < 0)
                        return Point.Zero;
                }

                var sprite = GetSprite(index);
                var location = new Point(sprite.X, sprite.Y);
                _arm.Erase(index);
                return location;
            }
        }

        public Level2BossController(
            ChompGameModule gameModule, 
            WorldSprite player, 
            EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers,
            SystemMemoryBuilder memoryBuilder) 
            : base(gameModule, player, bulletControllers, memoryBuilder)
        {
            _eye2 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _eye3 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _eye4 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _arm1 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossArm, memoryBuilder.Memory));
            _arm2 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossArm, memoryBuilder.Memory));

            var address = memoryBuilder.CurrentAddress;
            memoryBuilder.AddByte();

            _tentacle1 = new Tentacle(memoryBuilder, gameModule, 
                new GameBit(address, Bit.Bit0, memoryBuilder.Memory),
                new MaskedByte(address, (Bit)28, memoryBuilder.Memory, 2), _levelTimer);

            _tentacle2 = new Tentacle(memoryBuilder, gameModule,
                new GameBit(address, Bit.Bit1, memoryBuilder.Memory),
                new MaskedByte(address, (Bit)224, memoryBuilder.Memory, 5), _levelTimer);

            _phase = new NibbleEnum<Phase>(new LowNibble(memoryBuilder));
            memoryBuilder.AddByte();

            _partState = new DenseTwoBitArray(memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            memoryBuilder.AddByte();

            _bulletCounter = new LowNibble(memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            _eyeHit = new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 6);
            _tentacle1Hit = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit4, memoryBuilder.Memory);
            _tentacle2Hit = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit5, memoryBuilder.Memory);
            memoryBuilder.AddByte();


            GameDebug.Watch1 = new DebugWatch("St", () => _stateTimer.Value);
        }

        protected override string BossTiles { get; } =
            @"00BC000
              0BII900
              7IIII90
              GIIIIH0
              6GFFF50";

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
            if (WorldSprite.Y < 64)
                WorldSprite.Y = 64;

            if(_phase.Value == Phase.BeforeBoss)
            {
                _musicModule.CurrentSong = MusicModule.SongName.None;
                if (_player.X > 60)
                {
                    SetPhase(Phase.Init);
                    _musicModule.CurrentSong = MusicModule.SongName.Nemesis;
                }
            }
            else if (_phase.Value == Phase.Init)
            {
                SetupBoss();
                SetPhase(Phase.FadeIn);
            }
            else if(_phase.Value == Phase.FadeIn)
            {
                _bossBackgroundHandler.BossBgEffectType = BackgroundEffectType.SineWave;

                if (_bossBackgroundHandler.BossBgEffectValue > 0)
                {
                    _bossBackgroundHandler.BossBgEffectValue--;

                    if (_bossBackgroundHandler.BossBgEffectValue == 0)
                    {
                        SetupBossParts();
                        WorldSprite.Visible = true;
                        _tentacle1.TargetRight = false;
                        _tentacle2.TargetRight = true;
                        _tentacle1.CurlSpeed = 4;
                        _tentacle2.CurlSpeed = 4;
                    }
                }
                else
                {

                    if (_tentacle1.Update() & _tentacle2.Update())
                        SetPhase(Phase.PrepareShoot);

                    UpdatePartPositions();
                }
            }
            else if (_phase.Value == Phase.Sway)
            {
                _motion.XAcceleration = 1;
                _motion.YAcceleration = 1;

                if (WorldSprite.X > 100)
                    _motion.TargetXSpeed = -16;

                if (WorldSprite.X < 30 || _motion.TargetXSpeed == 0)
                    _motion.TargetXSpeed = 16;

                if(_levelTimer.IsMod(64))
                {
                    if(WorldSprite.X < _player.X)
                        _motion.TargetXSpeed = 16;
                    else
                        _motion.TargetXSpeed = -16;
                }

                if (WorldSprite.Y < 70 || _motion.TargetYSpeed == 0)
                    _motion.TargetYSpeed = 4;
                else if (WorldSprite.Y > 78)
                    _motion.TargetYSpeed = -4;

                if (_tentacle1.Update())
                {
                    _tentacle1.TargetRight = !_tentacle1.TargetRight;
                    _tentacle1.CurlSpeed = (byte)(_tentacle1.TargetRight ? 1 : 2);
                }

                if (_tentacle2.Update())
                {
                    _tentacle2.TargetRight = !_tentacle2.TargetRight;
                    _tentacle2.CurlSpeed = (byte)(_tentacle2.TargetRight ? 2 : 1);
                }

                _motionController.Update();

                if (_levelTimer.IsMod(64))
                    _stateTimer.Value++;

                if (_stateTimer.Value == 15)
                    SetPhase(Phase.Sway);
            }
            else if (_phase.Value == Phase.PrepareShoot)
            {
                if (_tentacle1.Update() & _tentacle2.Update() & _motion.XSpeed == 0 & _motion.YSpeed == 0)
                    SetPhase(Phase.Shoot);

                _motionController.Update();
            }
            else if (_phase.Value == Phase.Shoot)
            {
                PositionFreeCoinBlocksNearPlayer();

                _tentacle1.CurlSpeed = 1;
                _tentacle2.CurlSpeed =1;

                _tentacle1.CurlBetween(-100, -60);
                _tentacle2.CurlBetween(60, 100);

                if(_levelTimer.IsMod(32))
                {
                    if (WorldSprite.X < _player.X)
                        _motion.TargetXSpeed = 12;
                    else
                        _motion.TargetXSpeed = -12;

                    _motion.XAcceleration = 2;
                }
                _motionController.Update();

                if (_levelTimer.IsMod(8))
                {
                    _bulletCounter.Value++;
                    if (_bulletCounter.Value <= 1)
                        IncEyeState(0);
                    if (_bulletCounter.Value >= 3 && _bulletCounter.Value < 5)
                        IncEyeState(1);
                    if (_bulletCounter.Value >= 5 && _bulletCounter.Value < 7)
                        IncEyeState(2);
                    if (_bulletCounter.Value >= 7 && _bulletCounter.Value < 10)
                        IncEyeState(3);                   
                }

                if (_levelTimer.IsMod(64))
                {
                    if(AnyBombs())
                    {
                        if (_partState[0] < 3) _partState[0] = 0;
                        if (_partState[1] < 3) _partState[1] = 0;
                        if (_partState[2] < 3) _partState[2] = 0;
                        if (_partState[3] < 3) _partState[3] = 0;

                        SetPhase(Phase.Sway);
                        return;
                    }

                    _stateTimer.Value++;
                    if (_stateTimer.Value == 0)
                    {
                        if (_partState[0] < 3) _partState[0] = 0;
                        if (_partState[1] < 3) _partState[1] = 0;
                        if (_partState[2] < 3) _partState[2] = 0;
                        if (_partState[3] < 3) _partState[3] = 0;

                        SetPhase(Phase.PrepareShoot);
                    }
                }
            }

            _position.X = (byte)(WorldSprite.X - 12 - _tileModule.Scroll.X);
            _position.Y = (byte)(WorldSprite.Y - 68);

            if (_phase.Value >= Phase.Sway)
            {
                UpdatePartPositions();
                UpdateEyePalette(0);
                UpdateEyePalette(1);
                UpdateEyePalette(2);
                UpdateEyePalette(3);
            }
        }


        private bool AnyBombs()
        {
            bool anyBombs = false;
            _gameModule.SceneSpriteControllers.BombControllers.Execute(c =>
            {
                anyBombs = true;
            });

            return anyBombs;
        }

        protected override void UpdateDying()
        {
            _musicModule.CurrentSong = MusicModule.SongName.None;
            _position.X = (byte)(WorldSprite.X - 12 - _tileModule.Scroll.X);
            _position.Y = (byte)(WorldSprite.Y - 68);

            if (_stateTimer.Value == 0)
            {
                if (_levelTimer.IsMod(8))
                {
                    var pt1 = _tentacle1.DestroyNextSection();
                    var pt2 = _tentacle2.DestroyNextSection();

                    if (pt1.X == 0 && pt1.Y == 0
                        && pt2.X == 0 && pt2.Y == 0)
                    {
                        _stateTimer.Value = 1;
                        _statusBar.AddToScore((uint)PointsForEnemy);

                        var s1 = _arm1.Sprite;
                        var s2 = _arm2.Sprite;
                        CreateExplosion(s1.X, s1.Y);
                        CreateExplosion(s2.X, s2.Y);

                        _arm1.Sprite.Tile = 0;
                        _arm2.Sprite.Tile = 0;
                        _audioService.PlaySound(ChompAudioService.Sound.Break);
                    }
                    else
                    {
                        CreateExplosion(pt1.X, pt1.Y);
                        CreateExplosion(pt2.X, pt2.Y);

                        _audioService.PlaySound(ChompAudioService.Sound.Break);
                    }
                }
            }
            else
            {
                _bossBackgroundHandler.BossBgEffectType = BackgroundEffectType.DissolveFromBottom;

                if(_levelTimer.IsMod(4))
                {
                    _bossBackgroundHandler.BossBgEffectValue++;
                    if (_bossBackgroundHandler.BossBgEffectValue == 0)
                    {
                        _bossBackgroundHandler.BossBgEffectValue = 255;
                    }
                }

                if (_stateTimer.Value < 5 && _levelTimer.IsMod(4))
                {
                    var randomX = _rng.Generate(3) * 2;
                    var randomY = _rng.Generate(3);

                    CreateExplosion(WorldSprite.X - 8 + randomX, WorldSprite.Y + randomY);
                    _audioService.PlaySound(ChompAudioService.Sound.Break);
                }

                if (_levelTimer.Value.IsMod(32))
                {
                    _stateTimer.Value++;
                    if(_stateTimer.Value == 2)
                    {
                        _eye2.Sprite.Tile = 0;
                        WorldSprite.Visible = false;
                    }
                    if (_stateTimer.Value == 3)
                    {
                        _eye3.Sprite.Tile = 0;
                        _eye4.Sprite.Tile = 0;
                    }

                    if (_stateTimer.Value == 10)
                    {
                        EraseBossTiles();
                        _bossBackgroundHandler.BossBgEffectType = BackgroundEffectType.None;
                        _bossBackgroundHandler.BossBgEffectValue = 0;
                        Destroy();
                        _exitsModule.GotoNextLevel();
                        _player.Y = _gameModule.Specs.ScreenHeight + 8;
                    }
                }
            }
        }

        private Sprite GetEyeSprite(int index) =>
            index switch {
                1 => _eye2.Sprite,
                2 => _eye3.Sprite,
                3 => _eye4.Sprite,
                _ => WorldSprite.GetSprite()
            };

        private void UpdateEyePalette(byte index)
        {
            var state = _partState[index];
            if (state == 3)
                GetEyeSprite(index).Palette = SpritePalette.Fire;
            else if (state == 0)
                GetEyeSprite(index).Palette = SpritePalette.Enemy1;
            else
                GetEyeSprite(index).Palette = (SpritePalette)(_levelTimer.IsMod(2) ? 1 : 2);
        }

        private void IncEyeState(byte index)
        {
            if (_partState[index] == 3)
                return;

            _partState[index]++;
            if (_partState[index] == 3)
            {
                FireBullet(GetEyeSprite(index));
                _partState[index] = 0;
            }            
        }

        private void FireBullet(Sprite source)
        {
            var bullet = _bulletControllers.TryAddNew();
            if (bullet == null)
                return;

            bullet.DestroyOnTimer = true;
            bullet.WorldSprite.TileIndex = SpriteTileIndex.Extra1;
            bullet.WorldSprite.X = source.X;
            bullet.WorldSprite.Y = source.Y;
            bullet.WorldSprite.FlipX = true;

            int bulletSpeed = 24;
            for(int i = 0; i < 4; i++)
            {
                if (_partState[i] == 3)
                    bulletSpeed += 8;
            }

            var angle = bullet.WorldSprite.Center.GetVectorTo(_player.Center, bulletSpeed);
            angle = angle.RotateDeg(-16 + _rng.Generate(5));

            bullet.Motion.XSpeed = angle.X;
            bullet.Motion.YSpeed = angle.Y;

            _audioService.PlaySound(ChompAudioService.Sound.Fireball);
        }

        private void SetPhase(Phase phase)
        {
            _stateTimer.Value = 0;
            _phase.Value = phase;

            if(phase == Phase.BeforeBoss)
            {
                _bossBackgroundHandler.ShowCoins = true;
                WorldSprite.X = 80;
                WorldSprite.Y = 64;          
                _paletteModule.BgColor = 0;
            }
            else if(phase == Phase.FadeIn)
            {
                _bossBackgroundHandler.BossBgEffectValue = 100;
                WorldSprite.X = 80;
                WorldSprite.Y = 80;
                WorldSprite.Visible = false;
            }
            else if(phase == Phase.Sway)
            {
                _tentacle1.TargetRight = true;
                _tentacle2.TargetRight = true;
                _tentacle1.CurlSpeed = 1;
                _tentacle2.CurlSpeed = 1;
            }
            else if (phase == Phase.PrepareShoot)
            {
                _tentacle1.TargetRight = false;
                _tentacle2.TargetRight = true;
                _tentacle1.CurlSpeed = 2;
                _tentacle2.CurlSpeed = 2;
                _motion.TargetXSpeed = 0;
                _motion.TargetYSpeed = 0;
            }
            else if(phase == Phase.Dying)
            {
                WorldSprite.Status = WorldSpriteStatus.Dying;
            }
        }

        protected override void BeforeInitializeSprite()
        {
            SetPhase(Phase.BeforeBoss);
            base.BeforeInitializeSprite();
        }
        private void SetupBoss()
        {
            WorldSprite.X = 80;
            WorldSprite.Y = 64;
            SetBossTiles();
            _stateTimer.Value = 1;
            _paletteModule.BgColor = 0;
        }

        private void SetupBossParts()
        {
            var e2 = _eye2.PrepareSprite(SpriteTileIndex.Enemy1);
            e2.Tile2Offset = 1;
            e2.FlipX = true;
            _eye2.XOffset = -22;
            _eye2.YOffset = 0;

            var e3 = _eye3.PrepareSprite(SpriteTileIndex.Enemy1);
            e3.Tile2Offset = 1;
            e3.FlipX = true;
            _eye3.XOffset = -18;
            _eye3.YOffset = 8;

            var e4 = _eye4.PrepareSprite(SpriteTileIndex.Enemy1);
            e4.Tile2Offset = 1;
            e4.FlipX = false;
            _eye4.XOffset = -4;
            _eye4.YOffset = 8;

            var a1 = _arm1.PrepareSprite(SpriteTileIndex.Enemy2);
            a1.FlipX = false;
            _arm1.XOffset = -8;
            _arm1.YOffset = 16;

            var a2 = _arm2.PrepareSprite(SpriteTileIndex.Enemy2);
            a2.FlipX = false;
            _arm2.XOffset = 4;
            _arm2.YOffset = 16;

            _tentacle1.Initialize();
            _tentacle2.Initialize();
        }

        public override bool CollidesWithPlayer(PlayerController player)
        {
            if (_phase.Value < Phase.Sway)
                return false;

            int x = WorldSprite.X;
            int y = WorldSprite.Y;

            var spots = (Tentacle.NumSections * 2);
            var spotValue = _levelTimer.Value % spots;
            bool collisionEnabled = false;

            if(spotValue < Tentacle.NumSections)
            {
                var s = _tentacle1.GetSprite(spotValue);
                collisionEnabled = s.Visible;
                WorldSprite.X = s.X;
                WorldSprite.Y = s.Y;
            }
            else
            {
                var s = _tentacle2.GetSprite(spotValue - Tentacle.NumSections);
                collisionEnabled = s.Visible;
                WorldSprite.X = s.X;
                WorldSprite.X = s.X;
                WorldSprite.Y = s.Y;
            }
            
            var result = collisionEnabled && player.CollidesWith(WorldSprite);
            WorldSprite.X = x;
            WorldSprite.Y = y;
            return result;
        }

        public override bool CollidesWithBomb(WorldSprite bomb)
        {
            if (_phase.Value < Phase.Sway)
                return false;

            int x = WorldSprite.X;
            int y = WorldSprite.Y;

            // Check eyes
            var eyeHit = CheckBombEyeCollision(0, bomb)
                || CheckBombEyeCollision(1, bomb)
                || CheckBombEyeCollision(2, bomb)
                || CheckBombEyeCollision(3, bomb);

            bool tentacleHit = false;

            if (!eyeHit)
            {
                var spotValue = _levelTimer.Value % Tentacle.NumSections;

                // Tentacle 1
                var t1Sprite = _tentacle1.GetSprite(spotValue);
                if (t1Sprite.Visible)
                {
                    WorldSprite.X = t1Sprite.X;
                    WorldSprite.Y = t1Sprite.Y;
                    if (base.CollidesWithBomb(bomb))
                    {
                        _tentacle1Hit.Value = true;
                        tentacleHit = true;
                    }
                }

                // Tentacle 2
                var t2Sprite = _tentacle2.GetSprite(spotValue);
                if (t2Sprite.Visible)
                {
                    WorldSprite.X = t2Sprite.X;
                    WorldSprite.Y = t2Sprite.Y;
                    if (base.CollidesWithBomb(bomb))
                    {
                        _tentacle2Hit.Value = true;
                        tentacleHit = true;
                    }
                }
            }

            WorldSprite.X = x;
            WorldSprite.Y = y;
            return eyeHit || tentacleHit;
        }


        private bool CheckBombEyeCollision(int eyeIndex, WorldSprite bomb)
        {
            if (_partState[eyeIndex] == 3)
                return false;

            var eyeSprite = GetEyeSprite(eyeIndex);
            WorldSprite.X = eyeSprite.X;
            WorldSprite.Y = eyeSprite.Y;

            var result = base.CollidesWithBomb(bomb);
            if (result)
                _eyeHit.Value = (byte)eyeIndex;

            return result;
        }
        protected override void UpdatePartPositions()
        {
            _eye2.XOffset = -5;
            _eye2.YOffset = 1;

            _eye3.XOffset = -1;
            _eye3.YOffset = 6;

            _eye4.XOffset = -6;
            _eye4.YOffset = 7;


            _eye2.UpdatePosition(WorldSprite);
            _eye3.UpdatePosition(WorldSprite);
            _eye4.UpdatePosition(WorldSprite);
            _arm1.UpdatePosition(WorldSprite);
            _arm2.UpdatePosition(WorldSprite);

            _tentacle1.UpdatePosition(_arm1);
            _tentacle2.UpdatePosition(_arm2);
        }

        public override BombCollisionResponse HandleBombCollision(WorldSprite bomb)
        {
            // Tentacle hit logic
            if (_tentacle1Hit.Value)
            {
                int section = _levelTimer.Value % Tentacle.NumSections;
                var sectionSprite = _tentacle1.GetSprite(section);
                _tentacle1.TargetRight = bomb.X < sectionSprite.X;
                _tentacle1Hit.Value = false;
                _audioService.PlaySound(ChompAudioService.Sound.EnemyHit);
                return BombCollisionResponse.Bounce;
            }
            if (_tentacle2Hit.Value)
            {
                int section = _levelTimer.Value % Tentacle.NumSections;
                var sectionSprite = _tentacle2.GetSprite(section);
                _tentacle2.TargetRight = bomb.X < sectionSprite.X;
                _tentacle2Hit.Value = false;
                _audioService.PlaySound(ChompAudioService.Sound.EnemyHit);
                return BombCollisionResponse.Bounce;
            }

            _partState[_eyeHit.Value] = 3;
            _audioService.PlaySound(ChompAudioService.Sound.Lightning);

            if(GameDebug.BossTest)
                SetPhase(Phase.Dying);
            else if (_partState[0] == 3
                && _partState[1] == 3
                && _partState[2] == 3
                && _partState[3] == 3)
            {
                SetPhase(Phase.Dying);
            }
            else
            {
                SetPhase(Phase.PrepareShoot);
            }

            return BombCollisionResponse.Destroy;
        }
    }
}
