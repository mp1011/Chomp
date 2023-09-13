using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame.SpriteControllers
{
    class ChompBoss2Controller : EnemyController
    {
        public const int BossHp = 3;

        private readonly WorldSprite _player;
        private readonly EnemyOrBulletSpriteControllerPool<BossBulletController> _bullets;
        private readonly ChompAudioService _audioService;
        private readonly Specs _specs;
        private readonly GameByteArray _tailSprites;
        private readonly NibblePoint _motionTarget;

        private const int MaxY = 32;
        private const int MinY = 12;

        private const int TailSections = 4;

        private Point Target => new Point(8 + _motionTarget.X * 4, 8 + _motionTarget.Y * 2);
     
        enum Phase : byte 
        {
            Init=0,
        }

        private GameByte _phaseByte;
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

            _phaseByte = memoryBuilder.AddByte();
            _phase = new GameByteEnum<Phase>(_phaseByte);
            _specs = gameModule.Specs;
            _tailSprites = new GameByteArray(memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            memoryBuilder.AddBytes(TailSections);

            _motionTarget = new NibblePoint(memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            memoryBuilder.AddByte();
        }
     
        protected override void OnSpriteCreated(Sprite sprite)
        {
            _hitPoints.Value = BossHp;
            _stateTimer.Value = 0;
        }

        protected override void UpdateActive()
        {
            _motionController.Update();

            _motion.XSpeed = -20;
            _motion.TargetXSpeed = 0;
            _motion.XAcceleration = 10;
        }
       
        protected override void UpdateDying() { }
    }
}
