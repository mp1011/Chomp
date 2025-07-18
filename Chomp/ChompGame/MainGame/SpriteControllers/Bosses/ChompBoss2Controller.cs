﻿using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SpriteControllers
{
    class ChompBoss2Controller : EnemyController
    {
        public const int TurnAngle = 4;
        public const int BossHp = GameDebug.BossTest ? 1 : 4;

        private readonly PlayerController _playerController;
        private WorldSprite _player => _playerController.WorldSprite;
        private readonly EnemyOrBulletSpriteControllerPool<BossBulletController> _bullets;
        private readonly SpriteControllerPool<PrizeController> _prizes;

        private readonly Specs _specs;
        private readonly ChompTail _tail;
        private readonly RandomModule _randomModule;
        private const int NumTailSections = 6;
        private PrecisionMotion _firstTailSectionMotion;

        protected override int PointsForEnemy => 500;

        enum Phase : byte 
        {
            Init=0,
            Chase=1,
            SlowChase=2,
            Pause=3,
            Loop=4,
            ReCenter=5,
            Dying=6,
            Dead=7
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

            var phase = new MaskedByte(_stateTimer.Address, Bit.Left4, memoryBuilder.Memory, 4);
            _phase = new GameByteEnum<Phase>(phase);
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
        }

        private void CheckRecenter()
        {
            if (_phase.Value == Phase.ReCenter)
                return;

            if(WorldSprite.X < 0
                || WorldSprite.X > _specs.ScreenWidth
                || WorldSprite.Y < 0
                || WorldSprite.Y > _specs.ScreenHeight)
            {
                SetPhase(Phase.ReCenter);
            }
        }

        protected override void UpdateActive()
        {
            if (_phase.Value > Phase.Init)
            {
                if (WorldSprite.X < -4)
                {
                    WorldSprite.X = -4;
                    SetPhase(Phase.ReCenter);
                }

                if (WorldSprite.X > _specs.ScreenWidth + 4)
                    WorldSprite.X = _specs.ScreenWidth + 4;
            }

            _motionController.Update();
            if (_levelTimer.IsMod(16))
                _stateTimer.Value++;

            if (_rng.RandomChance(50) && _levelTimer.Value.IsMod(64))
                SpawnCoins();

            UpdateTail();

            if(_phase.Value != Phase.Init)
                CheckRecenter();

            switch (_phase.Value)
            {
                case Phase.Init:
                    if (_stateTimer.Value < 3)
                        InitTail();

                    _motion.XSpeed = -40;
                    _motion.TargetXSpeed = -20;
                    _motion.XAcceleration = 5;

                    if (WorldSprite.X < 48)
                        SetPhase(Phase.SlowChase);

                    return;
                case Phase.Chase:
                    var speed = 60 - _stateTimer.Value * 3;
                    _motion.TurnTowards(WorldSprite, _player.Center, TurnAngle, speed);

                    if (_stateTimer.Value == 10 || WorldSprite.X <= 0)
                        SetPhase(Phase.Pause);
                    return;
                case Phase.SlowChase:
                    speed = 20;
                    _motion.TurnTowards(WorldSprite, _player.Center, TurnAngle, speed);

                    if (_stateTimer.Value == 10)
                        SetPhase(Phase.Chase);
                    return;

                case Phase.Pause:
                    _motion.TargetXSpeed = 0;
                    _motion.TargetYSpeed = 0;
                    _motion.XAcceleration = 2;
                    _motion.YAcceleration = 2;

                    if (_stateTimer.Value == 6)
                    {
                        if (_randomModule.RandomChance(70))
                            SetPhase(Phase.SlowChase);
                        else
                            SetPhase(Phase.Loop);
                    }
                    return;

                case Phase.Loop:

                    if (_stateTimer.Value == 0)
                    {
                        _motion.TargetXSpeed = 20;
                        _motion.XAcceleration = 5;
                    }

                    if (_stateTimer.Value > 1)
                    {
                        _motion.Turn(8, 40);

                        if (_levelTimer.IsMod(16))
                            FireBullet();
                    }
            
                    if (_stateTimer.Value == 15)
                        SetPhase(Phase.Pause);

                    return;

                case Phase.ReCenter:
                    var target = new Point(32, 32);
                    _motion.TargetTowards(WorldSprite, target, 20);

                    var pos = new Point(WorldSprite.X, WorldSprite.Y);
                    if (pos.DistanceSquared(target) < 64)
                        SetPhase(Phase.Pause);

                    return;
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
        }

        private void FireBullet()
        {
            var bullet = _bullets.TryAddNew();
            if (bullet == null)
                return;

            _audioService.PlaySound(ChompAudioService.Sound.Fireball);

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
                    GetSprite().Palette = Palette;

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
                        _stateTimer.Value++;
                }
                return;
            }

            if(_phase.Value != Phase.Dying)            
                SetPhase(Phase.Dying);

            _stateTimer.Value++;

            if(_stateTimer.Value == 15)
            {
                _stateTimer.Value = 0;
                if (!DestroyNextTail())
                {
                    CreateExplosion(WorldSprite.X, WorldSprite.Y);
                    CreateExplosion(WorldSprite.X + 4, WorldSprite.Y + 4);
                    CreateExplosion(WorldSprite.X + 2, WorldSprite.Y);
                    CreateExplosion(WorldSprite.X + 2, WorldSprite.Y + 2);

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
