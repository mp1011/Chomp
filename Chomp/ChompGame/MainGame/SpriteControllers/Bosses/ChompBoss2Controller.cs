using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;
using System.Windows.Markup;

namespace ChompGame.MainGame.SpriteControllers
{
    class ChompBoss2Controller : EnemyController
    {
        public const int TurnAngle = 4;
        public const int BossHp = GameDebug.BossTest ? 1 : 4;


        private readonly MusicModule _musicModule;
        private readonly PlayerController _playerController;
        private WorldSprite _player => _playerController.WorldSprite;
        private readonly EnemyOrBulletSpriteControllerPool<BossBulletController> _bullets;
        private readonly SpriteControllerPool<PrizeController> _prizes;

        private readonly Specs _specs;
        private readonly ChompTail _tail;
        private readonly RandomModule _randomModule;
        private const int NumTailSections = 6;
        private PrecisionMotion _firstTailSectionMotion;
        private GameBit _counterAttack;

        protected override int PointsForEnemy => 1000;

        enum Phase : byte 
        {
            Init=0,
            MoveToCenter=1,
            Chase=2,
            Pause=3,
            Counterattack=4,
            Dying =5,
            Dead=6
        }

        protected override bool DestroyWhenFarOutOfBounds => false;
        protected override bool DestroyWhenOutOfBounds => false;
        protected override bool AlwaysActive => true;

        private GameByteEnum<Phase> _phase;
        protected override bool DestroyBombOnCollision => true;
        public ChompBoss2Controller(PlayerController player,
            EnemyOrBulletSpriteControllerPool<BossBulletController> bullets,
            SpriteControllerPool<PrizeController> prizes,
            ChompGameModule gameModule, 
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Chomp, SpriteTileIndex.Enemy1, gameModule, memoryBuilder)
        {
            _playerController = player;
            _bullets = bullets;
            _prizes = prizes;
            _randomModule = gameModule.RandomModule;
            _musicModule = gameModule.MusicModule;

            var phase = new MaskedByte(memoryBuilder.CurrentAddress, Bit.Right4, memoryBuilder.Memory, 0);
            _phase = new GameByteEnum<Phase>(phase);

            _counterAttack = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit7, memoryBuilder.Memory);
            memoryBuilder.AddByte();

            _specs = gameModule.Specs;
            _tail = new ChompTail(memoryBuilder, NumTailSections, gameModule);
         
            Palette = SpritePalette.Enemy1;

            _firstTailSectionMotion = new PrecisionMotion(memoryBuilder);
            memoryBuilder.AddBytes(PrecisionMotion.Bytes * NumTailSections - 1);
        }

        protected override void BeforeInitializeSprite()
        {
            _tail.CreateTail();
            SpriteIndex = _spritesModule.GetFreeSpriteIndex();           
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _hitPoints.Value = BossHp;
            _stateTimer.Value = 0;

            SetPhase(Phase.Init);
        }

        private void SetPhase(Phase p)
        {
            _stateTimer.Value = 0;
            _phase.Value = p;

            if(p == Phase.Init)
            {
                _musicModule.CurrentSong = MusicModule.SongName.None;
             //   GameDebug.Watch1 = new DebugWatch("PHASE", () => (int)_phase.Value);
             //   GameDebug.Watch2 = new DebugWatch("ST", () => (int)_stateTimer.Value);
             //   GameDebug.Watch3 = new DebugWatch("HP", () => (int)_hitPoints.Value);

                InitTail();
            }
            else if (p == Phase.Dying)
            {
                _musicModule.CurrentSong = MusicModule.SongName.None;
            }
        }

        public override BombCollisionResponse HandleBombCollision(WorldSprite player)
        {
            if (!_counterAttack.Value)
            {
                _counterAttack.Value = true;
                SetPhase(Phase.Pause);
            }
            return base.HandleBombCollision(player);
        }

        protected override void UpdateActive()
        {
            _motionController.Update();

            if (_rng.RandomChance(50) && _levelTimer.Value.IsMod(64))
                SpawnCoins();

            UpdateTail();

            if (WorldSprite.X < 0)
                WorldSprite.X = 0;
            if (WorldSprite.Y < 8)
                WorldSprite.Y = 8;

            if (_phase.Value == Phase.Init)
            {
                if (_stateTimer.Value == 0)
                {
                    _stateTimer.Value = 1;
                    InitTail();
                }

                if (_levelTimer.IsMod(8))
                    _stateTimer.Value++;

                if (_stateTimer.Value == 8)
                {
                    _musicModule.CurrentSong = MusicModule.SongName.Threat;

                    _motion.XSpeed = -40;
                    _motion.TargetXSpeed = -20;
                    _motion.XAcceleration = 5;

                    SetPhase(Phase.MoveToCenter);
                }
            }
            else if (_phase.Value == Phase.MoveToCenter)
            {
                var target = new Point(32, 32);
                _motion.TargetTowards(WorldSprite, target, 20);

                var pos = new Point(WorldSprite.X, WorldSprite.Y);
                if (pos.DistanceSquared(target) < 64)
                {
                    _stateTimer.Value++;
                    if (_stateTimer.Value == 0)
                    {
                        SetPhase(Phase.Chase);
                    }
                }

                return;
            }
            else if (_phase.Value == Phase.Chase)
            {
                if (_levelTimer.IsMod(32))
                {
                    _stateTimer.Value++;
                    if (_stateTimer.Value == 0)
                        SetPhase(Phase.Pause);
                }

                var speed = 20 + (_stateTimer.Value * 2);
                if (speed > 40)
                    speed = 40;
                _motion.TurnTowards(WorldSprite, _player.Center, TurnAngle, speed);
                return;
            }           
            else if (_phase.Value == Phase.Pause)
            {
                _motion.TargetXSpeed = 0;
                _motion.TargetYSpeed = 0;
                _motion.XAcceleration = 2;
                _motion.YAcceleration = 2;
                if (_levelTimer.IsMod(4))
                {
                    _stateTimer.Value++;
                    if (_stateTimer.Value == 0)
                    {
                        if (_counterAttack.Value)
                        {
                            _counterAttack.Value = false;
                            SetPhase(Phase.Counterattack);
                        }
                        else
                            SetPhase(Phase.Chase);
                    }
                }
            }
            else if (_phase.Value == Phase.Counterattack)
            {
                if (_stateTimer.Value == 0)
                {
                    _motion.TargetXSpeed = 20;
                    _motion.XAcceleration = 5;
                }

                if (_levelTimer.IsMod(16))
                    _stateTimer.Value++;

                if (_stateTimer.Value > 1)
                {
                    _motion.Turn(8, 40);

                    if (_levelTimer.IsMod(16))
                        FireBullet();
                }

                if (_stateTimer.Value == 15)
                    SetPhase(Phase.Chase);
            }
        }

        private void SpawnCoins()
        {
            if (!_prizes.CanAddNew())
                return;

            var prize = _prizes.TryAddNew();
            if (prize == null)
                return;

            prize.WorldSprite.X = _specs.ScreenWidth;
            prize.WorldSprite.Y = 16 + (_rng.GenerateByte() % (_specs.ScreenHeight - 20)) ;
            prize.Variation = PrizeController.Coin3;
            prize.AfterSpawn(_prizes);
            prize.WorldSprite.UpdateSprite();
        }

        private void FireBullet()
        {
            var bullet = _bullets.TryAddNew();
            if (bullet == null)
                return;

            _audioService.PlaySound(ChompAudioService.Sound.Fireball);

            bullet.DestroyOnCollision = false;
            bullet.DestroyOnTimer = true;
            bullet.WorldSprite.Center = WorldSprite.Center;
            bullet.Motion.YSpeed = _motion.YSpeed;
            bullet.Motion.XSpeed = _motion.XSpeed;
        }

        private void InitTail()
        {
            var target = WorldSprite.Center;

            for (int i = 0; i < NumTailSections; i++)
            {
                var section = _tail.GetSprite(i);
                section.X = (byte)target.X;
                section.Y = (byte)target.Y;
            }
        }

        private void UpdateTail()
        {
            var target = WorldSprite.Center;
            
            for (int i = 0; i < NumTailSections; i++)
            {
                target = UpdateTailSection(_tail.GetSprite(i), target, i);
            }
        }

        private Point UpdateTailSection(Sprite section, Point target, int sectionNumber)
        {
            target = target.ClampTo(_specs.ScreenWidth, _specs.ScreenHeight, 0);

            var sectionPos = new Point(section.X, section.Y);
            var distSq = sectionPos.DistanceSquared(target);
            int speed = 60 - (sectionNumber * 6);

            if (distSq > 24 * 24)
            {
                var offset = target.GetVectorTo(sectionPos, 6);
                section.X = (target.X + offset.X).ByteClamp(_specs.ScreenWidth);
                section.Y = (target.Y + offset.Y).ByteClamp(_specs.ScreenHeight);

                return new Point(section.X, section.Y);
            }
            PrecisionMotion sectionMotion = new PrecisionMotion(_spritesModule.GameSystem.Memory,
                _firstTailSectionMotion.Address + (sectionNumber * PrecisionMotion.Bytes));
           
            var angleTo = sectionPos.GetVectorTo(target, speed);
            sectionMotion.XSpeed = angleTo.X;
            sectionMotion.YSpeed = angleTo.Y;

            sectionMotion.Apply(section);

            section.X = section.X.ByteClamp(_specs.ScreenWidth);
            section.Y = section.Y.ByteClamp(_specs.ScreenHeight);

            if (_phase.Value == Phase.Pause)
                return target;

            var targetOffset = angleTo
                .AdjustLength(100) 
                .RotateDeg(180)
                .AdjustLength(4);

            return new Point(section.X + targetOffset.X, section.Y + targetOffset.Y);
        }
       
        protected override void UpdateDying() 
        {
            if (_hitPoints.Value > 0)
            {
                base.UpdateDying();
                if (WorldSprite.Status == WorldSpriteStatus.Active)
                {
                    GetSprite().Palette = Palette;
                    SetPhase(Phase.Pause);
                }

                return;
            }

            if(_phase.Value == Phase.Dead)
            {
                if (_stateTimer.Value == 14)
                {
                    _playerController.OnBossDead();             
                    WorldSprite.Destroy();
                    _scenePartsDestroyed.SetDestroyed(DestructionBitOffset);
                    _stateTimer.Value = 15;
                }
                else if (_stateTimer.Value < 14)
                {
                    if (_levelTimer.IsMod(4))
                    {
                        CreateExplosion(WorldSprite.X - 2 + (_rng.Generate(2) * 3), 
                                        WorldSprite.Y - 2 + (_rng.Generate(2) * 3));

                        _stateTimer.Value++;
                    }
                }
                return;
            }

            if (_phase.Value != Phase.Dying)
            {
                _statusBar.AddToScore((uint)PointsForEnemy);
                SetPhase(Phase.Dying);
            }

            _stateTimer.Value++;

            if(_stateTimer.Value == 15)
            {
                _stateTimer.Value = 0;
                if (!DestroyNextTail())
                {
                    SetPhase(Phase.Dead);
                }
            }            
        }

        private bool DestroyNextTail()
        {
            int index = NumTailSections - 1;

            while(_tail.IsErased(index))
            {
                index--;
                if (index < 0)
                    return false;
            }

            var tail = _tail.GetSprite(index);
            var x = tail.X;
            var y = tail.Y;
            _tail.Erase(index);

            _audioService.PlaySound(ChompAudioService.Sound.Break);
            CreateExplosion(x, y);
           
            return true;
        }

        private void CreateExplosion(int x, int y)
        {
            var explosion = _bullets.TryAddNew();
            if (explosion != null)
            {
                explosion.EnsureInFrontOf(this);
                explosion.WorldSprite.Center = new Point(x, y);
                explosion.Motion.YSpeed = 0;
                explosion.Motion.XSpeed = 0;
                explosion.Explode();
            }
        }
    }
}
