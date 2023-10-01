using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SpriteControllers
{
    class ChompBoss2Controller : EnemyController
    {
        public const int TurnAngle = 4;
        public const int BossHp = 3;

        private readonly WorldSprite _player;
        private readonly EnemyOrBulletSpriteControllerPool<BossBulletController> _bullets;
        private readonly ChompAudioService _audioService;
        private readonly Specs _specs;
        private readonly ChompTail _tail;
        private readonly NibblePoint _motionTarget;
        private const int NumTailSections = 6;
        private PrecisionMotion _firstTailSectionMotion;

        private Point Target => new Point(8 + _motionTarget.X * 4, 8 + _motionTarget.Y * 2);
     
        enum Phase : byte 
        {
            Init=0,
            Chase=1,
            Pause=2
        }

        protected override bool DestroyWhenFarOutOfBounds => false;
        protected override bool DestroyWhenOutOfBounds => false;

        private GameByteEnum<Phase> _phase;

        public ChompBoss2Controller(WorldSprite player,
            EnemyOrBulletSpriteControllerPool<BossBulletController> bullets,
            ChompGameModule gameModule, 
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Chomp, SpriteTileIndex.AutoscrollEnemy3, gameModule, memoryBuilder)
        {
            _player = player;
            _audioService = gameModule.AudioService;
            _bullets = bullets;

            var phase = new MaskedByte(_stateTimer.Address, Bit.Left4, memoryBuilder.Memory, 4);
            _phase = new GameByteEnum<Phase>(phase);
            _specs = gameModule.Specs;
            _tail = new ChompTail(memoryBuilder, NumTailSections, _spritesModule, _spriteTileTable);
         
            _motionTarget = new NibblePoint(memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            memoryBuilder.AddByte();

            Palette = 2;

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

        protected override void UpdateActive()
        {
            _motionController.Update();
            if (_levelTimer.IsMod(16))
                _stateTimer.Value++;

            UpdateTail();

            switch (_phase.Value)
            {
                case Phase.Init:
                    if (_stateTimer.Value < 3)
                        InitTail();

                    _motion.XSpeed = -40;
                    _motion.TargetXSpeed = -20;
                    _motion.XAcceleration = 5;

                    if (WorldSprite.X < 48)
                        SetPhase(Phase.Chase);

                    return;
                case Phase.Chase:
                    var speed = 60 - _stateTimer.Value*3;
                    _motion.TurnTowards(WorldSprite, _player.Center, TurnAngle, speed);

                    if (_stateTimer.Value == 10)
                        SetPhase(Phase.Pause);
                    return;

                case Phase.Pause:
                    _motion.TargetXSpeed = 0;
                    _motion.TargetYSpeed = 0;
                    _motion.XAcceleration = 2;
                    _motion.YAcceleration = 2;

                    if (_stateTimer.Value == 6)
                        SetPhase(Phase.Chase);
                    return;
            }

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
            var sectionPos = new Point(section.X, section.Y);
            var distSq = sectionPos.DistanceSquared(target);

            int speed = 60 - (sectionNumber * 6);

            PrecisionMotion sectionMotion = new PrecisionMotion(_spritesModule.GameSystem.Memory,
                _firstTailSectionMotion.Address + (sectionNumber * PrecisionMotion.Bytes));
           
            var angleTo = sectionPos.GetVectorTo(target, speed);
            sectionMotion.XSpeed = angleTo.X;
            sectionMotion.YSpeed = angleTo.Y;

            sectionMotion.Apply(section);

            if (_phase.Value == Phase.Pause)
                return target;

            var targetOffset = angleTo
                .AdjustLength(100)
                .RotateDeg(180)
                .AdjustLength(4);

            return new Point(section.X + targetOffset.X, section.Y + targetOffset.Y);
        }
       
        protected override void UpdateDying() { }
    }
}
