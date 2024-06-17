using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SpriteControllers
{
    class ChompBoss3Controller : EnemyController
    {
        public const int BossHp = 1;
        private const int StopY = 20;

        private readonly WorldScroller _scroller;
        private readonly EnemyOrBulletSpriteControllerPool<BouncingBossBulletController> _bullets;
        private readonly MusicModule _music;
        private readonly Specs _specs;

        private GameShort _armAngle;
        private GameByte _armLength;
        private Arm _arm1, _arm2;

        enum Phase : byte 
        {
            Init,
            Intro,
            BeginArmSpin,
            ArmSpin,
            EndArmSpin,
            Fireballs,
            Dying
        }

        class Arm
        {
            public const int NumSections = 6;
            private ChompTail _tail;

            public Arm(SystemMemoryBuilder systemMemoryBuilder, ChompGameModule chompGameModule)
            {
                _tail = new ChompTail(systemMemoryBuilder, NumSections, chompGameModule);
            }

            public void Build()
            {
                _tail.CreateTail();
            }

            public SimpleWorldSprite GetWorldSprite(int index) => _tail.GetWorldSprite(index);

            public void Hide()
            {
                for (int i = 0; i < NumSections; i++)
                {
                    var part = _tail.GetWorldSprite(i);
                    part.Y = 0;
                    part.X = 0;
                }
            }

            public void Update(WorldSprite boss, float angle, byte length)
            {
                Point lastPoint = boss.Center;

                for (int i = 0; i < NumSections; i++)
                {
                    Point offset = new Point(0, length).RotateDeg(angle);
                    angle -= 8;

                    var part = _tail.GetWorldSprite(i);
                    var newCenter = new Point(
                       lastPoint.X + offset.X,
                       lastPoint.Y + offset.Y);
                    part.Center = newCenter;
                    lastPoint = part.Center;
                }
            }
        }

        private GameByteEnum<Phase> _phase;

        protected override bool DestroyBombOnCollision => true;

        public ChompBoss3Controller(WorldSprite player,
            EnemyOrBulletSpriteControllerPool<BouncingBossBulletController> bullets,
            ChompGameModule gameModule, 
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Chomp, SpriteTileIndex.Enemy1, gameModule, memoryBuilder)
        {
            _music = gameModule.MusicModule;
            _scroller = gameModule.WorldScroller;
            _bullets = bullets;

            _phase = new GameByteEnum<Phase>(memoryBuilder.AddByte());
            _specs = gameModule.Specs;
 
            Palette = SpritePalette.Enemy1;

            _armAngle = memoryBuilder.AddShort();
            _armLength = memoryBuilder.AddByte();
            _arm1 = new Arm(memoryBuilder, gameModule);
            _arm2 = new Arm(memoryBuilder, gameModule);
        }

        protected override void BeforeInitializeSprite()
        {
            _arm1.Build();
            _arm2.Build();
            SpriteIndex = _spritesModule.GetFreeSpriteIndex();
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _hitPoints.Value = BossHp;
            _stateTimer.Value = 0;
            _phase.Value = Phase.Init;

            GameDebug.Watch1 = new DebugWatch("PHASE", () => (int)_phase.Value);
            GameDebug.Watch2 = new DebugWatch("PT", () => (int)_stateTimer.Value);
            GameDebug.Watch3 = new DebugWatch("Y", () => (int)WorldSprite.Y);

        }

        protected override void UpdateActive()
        {
            switch (_phase.Value)
            {
                case Phase.Init:

                    WorldSprite.X = (_specs.ScreenWidth / 2) - 4;
                    WorldSprite.Y = 0;

                    _motion.SetXSpeed(0);
                    _motion.SetYSpeed(0);

                    _music.CurrentSong = MusicModule.SongName.None;
                    if (_levelTimer.IsMod(16))
                    {
                        _stateTimer.Value++;
                        if (_stateTimer.Value == 0)
                            SetPhase(Phase.Intro);
                    }

                    break;

                case Phase.Intro:
                    _music.CurrentSong = MusicModule.SongName.Threat;
                    _motion.SetYSpeed(8);
                    _motionController.Update();

                    if (WorldSprite.Y > StopY)
                        SetPhase(Phase.BeginArmSpin);

                    break;
                case Phase.BeginArmSpin:
                    _motion.TargetYSpeed = 0;
                    _motion.YAcceleration = 1;

                    if (WorldSprite.X < (_specs.ScreenWidth / 2) - 4)
                    {
                        _motion.SetXSpeed(1);
                    }
                    else if (WorldSprite.X > (_specs.ScreenWidth / 2) - 4)
                    {
                        _motion.SetXSpeed(-1);
                    }
                    else
                        _motion.SetXSpeed(0);

                    _motionController.Update();
                   
                 

                    UpdateArms();

                    if (_levelTimer.IsMod(32))
                        _armLength.Value++;

                    if (_armLength.Value == 6)
                        SetPhase(Phase.ArmSpin);
                    break;
                case Phase.ArmSpin:
                    _motion.TargetYSpeed = 0;
                    _motion.YAcceleration = 1;

                    _motion.SetXSpeed(0);

                    _motionController.Update();
                    UpdateArms();

                    if (_levelTimer.IsMod(64))
                    {
                        _stateTimer.Value++;
                        if (_stateTimer.Value == 10)
                            SetPhase(Phase.EndArmSpin);
                    }
                    break;
                case Phase.EndArmSpin:
                    _motion.TargetYSpeed = 0;
                    _motion.YAcceleration = 1;
                    _motionController.Update();
                    UpdateArms();

                    if (_levelTimer.IsMod(32))
                        _armLength.Value--;

                    if (_armLength.Value == 0)
                    {
                        HideArms();
                        SetPhase(Phase.Fireballs);
                    }
                    break;
                case Phase.Fireballs:

                    Sway();
                    _motionController.Update();
                    _motion.SetYSpeed(0);

                   
                    if (_levelTimer.IsMod(64))
                    {
                        if(_stateTimer.Value <= 10)
                            FireBullet();

                        _stateTimer.Value++;
                        if (_stateTimer.Value == 0)
                            SetPhase(Phase.BeginArmSpin);
                    }
                    break;
            }
        }

        private void Sway()
        {
            if (_levelTimer.IsMod(16))
            {
                if (WorldSprite.X < (_specs.ScreenWidth / 2) - 4)
                    _motion.TargetXSpeed = 10;
                else
                    _motion.TargetXSpeed = -10;
            }

            _motion.XAcceleration = 1;
        }

        private void HideArms()
        {
            _arm1.Hide();
            _arm2.Hide();
        }

        private void UpdateArms()
        {
            float armAngle = _armAngle.Value * 0.6f;

            _arm1.Update(WorldSprite, armAngle, _armLength);
            _arm2.Update(WorldSprite, armAngle + 180f, _armLength);

            _armAngle.Value++;

            if (_armAngle.Value == 3600)
                _armAngle.Value = 0;
        }

        private void SetPhase(Phase phase)
        {
            _phase.Value = phase;
            _stateTimer.Value = 0;
        }

        private void FireBullet()
        {
            var bullet = _bullets.TryAddNew();
            if (bullet == null)
                return;

            bullet.WorldSprite.Center = WorldSprite.Center;
            bullet.AcceleratedMotion.TargetYSpeed = 20;
            bullet.AcceleratedMotion.YAcceleration = 1;
            bullet.AcceleratedMotion.YSpeed = -20;

            bullet.AcceleratedMotion.SetXSpeed(0);
        }

        private void CreateExplosion()
        {
            var bullet = _bullets.TryAddNew();
            if (bullet == null)
                return;

            bullet.EnsureInFrontOf(this);
            bullet.WorldSprite.Center = WorldSprite.Center.Add(
                _rng.RandomItem(-4, -2, 0, 2, 4),
                _rng.RandomItem(-4, 2, 0, 2, 4));

            bullet.Motion.YSpeed = 0;
            bullet.Motion.XSpeed = 0;
            bullet.Explode();            
        }

        protected override void UpdateDying()
        {
            if (_hitPoints.Value > 0)
            {
                base.UpdateDying();
                if (WorldSprite.Status == WorldSpriteStatus.Active)
                    GetSprite().Palette = Palette;

                return;
            }

            if (_phase.Value != Phase.Dying)
            {
                _phase.Value = Phase.Dying;
                _stateTimer.Value = 0;
            }
            if (_armLength.Value > 0)
            {
                if (_levelTimer.IsMod(32))
                    _armLength.Value--;

                UpdateArms();
                if (_armLength.Value == 0)
                    HideArms();
            }

            if (_levelTimer.IsMod(12))
                CreateExplosion();

            if (_levelTimer.IsMod(16))
            {
                _stateTimer.Value++;

                if (_stateTimer.Value == 0)
                {
                    Destroy();

                    _scroller.ModifyTiles((tilemap, attr) =>
                    {
                        for (int y = 8; y < 12; y++)
                        {
                            tilemap[tilemap.Width - 1, y] = 0;
                            tilemap[tilemap.Width - 2, y] = 0;
                            attr[(tilemap.Width / 2) - 1, y / 2] = 1;
                        }
                    });
                }
            }
        }

        public override bool CollidesWithPlayer(PlayerController player)
        {
            if (base.CollidesWithPlayer(player))
                return true;

            var partIndex = _levelTimer.Value % Arm.NumSections;

            var part1 = _arm1.GetWorldSprite(partIndex);
            var part2 = _arm2.GetWorldSprite(partIndex);

            return player.CollidesWith(part1) || player.CollidesWith(part2);
        } 
    }
}
