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
        private WorldSprite _collisionSprite;
        private BossPart _eye2, _eye3, _eye4, _arm1, _arm2;
        private Tentacle _tentacle1, _tentacle2;
        private NibbleEnum<Phase> _phase;

        private NibbleEnum<CollisionIndex> _bombCollisionIndex;

        private enum CollisionIndex : byte
        {
            None,
            Eye1,
            Eye2,
            Eye3,
            Eye4,
            LeftArm,
            RightArm
        }

        private enum Phase : byte
        {
            Init,
            Sway,
            PrepareShoot,
            Shoot
        }



        class Tentacle
        {
            public const int NumSections = 6;
            private ChompTail _arm;
            private GameByte _curl;

            private GameBit _curlTarget;
            private MaskedByte _curlSpeed;

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
                SpritesModule spritesModule, 
                SpriteTileTable spriteTileTable,
                GameBit curlTarget,
                MaskedByte curlSpeed)
            {
                _arm = new ChompTail(memoryBuilder, NumSections, spritesModule, spriteTileTable);
                _curl = memoryBuilder.AddByte();
                _curlTarget = curlTarget;
                _curlSpeed = curlSpeed;
            }

            public void Initialize()
            {
                _arm.CreateTail(SpriteTileIndex.Enemy2);
                Curl = 0;
            }

            public bool Update()
            {
                if (_curlSpeed == 0)
                    return false;

                int newCurl = _curlTarget.Value ? (Curl + _curlSpeed.Value) : (Curl - _curlSpeed.Value);
                if (newCurl >= 127)
                {
                    Curl = 127;
                    return true;
                }
                else if(newCurl <= -128)
                {
                    Curl = 128;
                    return true;
                }

                Curl = newCurl;
                return false;
            }

            public void UpdatePosition(Sprite anchor)
            {
                var target = anchor;
                for (int i = 0; i < NumSections; i++)
                {
                    target = UpdateSection(target, _arm.GetSprite(i), i);
                }
            }

            private Sprite UpdateSection(Sprite anchor, Sprite section, int sectionNumber)
            {
                int mod = Curl / 4;

                var offset = new Point(0, 6);
                offset = offset.RotateDeg(Curl + sectionNumber * mod);
                section.X = (byte)(anchor.X + offset.X);
                section.Y = (byte)(anchor.Y + offset.Y);
                return section;
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

            _tentacle1 = new Tentacle(memoryBuilder, _spritesModule, _spriteTileTable, 
                new GameBit(address, Bit.Bit0, memoryBuilder.Memory),
                new MaskedByte(address, (Bit)28, memoryBuilder.Memory, 2));

            _tentacle2 = new Tentacle(memoryBuilder, _spritesModule, _spriteTileTable,
                new GameBit(address, Bit.Bit1, memoryBuilder.Memory),
                new MaskedByte(address, (Bit)224, memoryBuilder.Memory, 5));

            _phase = new NibbleEnum<Phase>(new LowNibble(memoryBuilder));
            _bombCollisionIndex = new NibbleEnum<CollisionIndex>(new HighNibble(memoryBuilder));

            memoryBuilder.AddByte();
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
            if (WorldSprite.Y < 64)
                WorldSprite.Y = 64;

            if (_phase.Value == Phase.Init)
            {
                SetupBoss();
                SetPhase(Phase.Sway);
            }
            else if (_phase.Value == Phase.Sway)
            {
                _motion.XAcceleration = 1;
                _motion.YAcceleration = 1;

                if (WorldSprite.X > 100)
                    _motion.TargetXSpeed = -16;

                if (WorldSprite.X < 30 || _motion.TargetXSpeed == 0)
                    _motion.TargetXSpeed = 16;

                if (WorldSprite.Y < 70)
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

                if (_levelTimer.IsMod(32))
                    _stateTimer.Value++;

                if (_stateTimer.Value == 15)
                    SetPhase(Phase.PrepareShoot);
            }
            else if (_phase.Value == Phase.PrepareShoot)
            {
                if (_tentacle1.Update() & _tentacle2.Update() & _motion.XSpeed == 0 & _motion.YSpeed == 0)
                    SetPhase(Phase.Shoot);

                _motionController.Update();
            }
            else if (_phase.Value == Phase.Shoot)
            {
                _dynamicBlockController.PositionFreeCoinBlocksNearPlayer(
                    (byte)(_player.X / _spritesModule.Specs.TileWidth),
                    (byte)(_spritesModule.Specs.NameTableHeight - 6));

                if (_levelTimer.IsMod(16))
                {
                    var e = _stateTimer.Value % 6;
                    if (e == 0)
                        FireBullet(WorldSprite.GetSprite());
                    else if (e == 1)
                        FireBullet(_eye2.Sprite);
                    else if(e == 2)
                        FireBullet(_eye3.Sprite);
                    else if(e == 3)
                        FireBullet(_eye4.Sprite);

                    _stateTimer.Value++;
                    if (_stateTimer.Value == 0)
                        SetPhase(Phase.Sway);
                }
            }
            
            UpdatePartPositions();
        }

        private void FireBullet(Sprite source)
        {
            var bullet = _bulletControllers.TryAddNew();
            if (bullet == null)
                return;

            bullet.WorldSprite.TileIndex = SpriteTileIndex.Extra1;
            bullet.WorldSprite.X = source.X;
            bullet.WorldSprite.Y = source.Y;
            bullet.WorldSprite.FlipX = true;

            var angle = bullet.WorldSprite.Center.GetVectorTo(_player.Center, 32);
            bullet.Motion.XSpeed = angle.X;
            bullet.Motion.YSpeed = angle.Y;

            _audioService.PlaySound(ChompAudioService.Sound.Fireball);
        }

        private void SetPhase(Phase phase)
        {
            _stateTimer.Value = 0;
            _phase.Value = phase;

            if(phase == Phase.Sway)
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
        }
        protected override void BeforeInitializeSprite()
        {
            _phase.Value = Phase.Init;
            base.BeforeInitializeSprite();
        }
        private void SetupBoss()
        {
            WorldSprite.X = 40;
            WorldSprite.Y = _player.Y - 48;
            SetBossBackgroundEnd(4);
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


            GameDebug.Watch1 = new DebugWatch("Boss X", () => WorldSprite.X);
            GameDebug.Watch2 = new DebugWatch("Boss Y", () => WorldSprite.Y);
            GameDebug.Watch3 = new DebugWatch("ST", () => _stateTimer.Value);

        }

        public override bool CollidesWithPlayer(PlayerController player)
        {
            int x = WorldSprite.X;
            int y = WorldSprite.Y;

            var spots = (Tentacle.NumSections * 2);
            var spotValue = _levelTimer.Value % spots;

            if(spotValue < Tentacle.NumSections)
            {
                var s = _tentacle1.GetSprite(spotValue);
                WorldSprite.X = s.X;
                WorldSprite.Y = s.Y;
            }
            else
            {
                var s = _tentacle2.GetSprite(spotValue - Tentacle.NumSections);
                WorldSprite.X = s.X;
                WorldSprite.Y = s.Y;
            }

            var result = player.CollidesWith(WorldSprite);
            WorldSprite.X = x;
            WorldSprite.Y = y;
            return result;
        }

        public override bool CollidesWithBomb(WorldSprite bomb)
        {
            int x = WorldSprite.X;
            int y = WorldSprite.Y;

            var spots = (Tentacle.NumSections * 2);
            var spotValue = _levelTimer.Value % spots;

            if (spotValue < Tentacle.NumSections)
            {
                var s = _tentacle1.GetSprite(spotValue);
                WorldSprite.X = s.X;
                WorldSprite.Y = s.Y;
            }
            else
            {
                var s = _tentacle2.GetSprite(spotValue - Tentacle.NumSections);
                WorldSprite.X = s.X;
                WorldSprite.Y = s.Y;
            }

            var result = base.CollidesWithBomb(bomb);
            if(result)
            {
                if (spotValue < Tentacle.NumSections)
                {
                    _bombCollisionIndex.Value = CollisionIndex.LeftArm;
                    _tentacle1.CurlSpeed = 0;
                }
                else
                {
                    _bombCollisionIndex.Value = CollisionIndex.RightArm;
                    _tentacle2.CurlSpeed = 0;
                }
            }

            WorldSprite.X = x;
            WorldSprite.Y = y;
            return result;
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

            _tentacle1.UpdatePosition(_arm1.Sprite);
            _tentacle2.UpdatePosition(_arm2.Sprite);
        }

        public override bool HandleBombCollision(WorldSprite player)
        {
            return false;
        }

    }
}
