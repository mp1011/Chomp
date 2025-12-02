using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteControllers.MotionControllers;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame.SpriteControllers
{
    class GemSpriteController : ActorController, ICollidableSpriteController
    {
        private readonly ChompAudioService _audioService;
        protected readonly CollisionDetector _collisionDetector;
        protected readonly PlayerController _playerController;
        private readonly EnemyOrBulletSpriteControllerPool<BossBulletController> _bullets;

        protected IMotionController _motionController;
        protected AcceleratedMotion _motion;

        public AcceleratedMotion AcceleratedMotion => _motion;

        protected override bool DestroyWhenFarOutOfBounds => false;
        protected override bool DestroyWhenOutOfBounds => false;

        protected override bool AlwaysActive => true;

        private LowNibble _index;
        private GameBit _collected;
        private GameBit _expanding;

        public byte Index
        {
            get => _index.Value;
            set => _index.Value = value;
        }

        public bool Expanding
        {
            get => _expanding.Value;
            set => _expanding.Value = value;
        }
        

        public GemSpriteController(
                ChompGameModule gameModule,
                PlayerController playerController,
                EnemyOrBulletSpriteControllerPool<BossBulletController> bullets,
                SystemMemoryBuilder memoryBuilder)
            : base(SpriteType.Gem, gameModule, memoryBuilder, SpriteTileIndex.Extra2)
        {
            _collisionDetector = gameModule.CollissionDetector;
            _playerController = playerController;
            _bullets = bullets;
            _index = new LowNibble(memoryBuilder);
            _collected = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit7, memoryBuilder.Memory);
            _expanding = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit6, memoryBuilder.Memory);

            memoryBuilder.AddByte();
            _audioService = gameModule.AudioService;
            var motionController = new ActorMotionController(gameModule.SpritesModule, gameModule.SpriteTileTable,
                gameModule.LevelTimer, memoryBuilder, new SpriteDefinition(SpriteType.Bomb, memoryBuilder.Memory), WorldSprite);

            _motionController = motionController;
            _motion = motionController.Motion;
        }

        protected override void BeforeInitializeSprite()
        {
            WorldSprite.X = 16 + _rng.Generate(6);
            WorldSprite.Y = 80 + _rng.Generate(4);

        }

        protected override void UpdateActive()
         {
            if (!_collected.Value)
            {
                if (_levelTimer.IsMod(16))
                {
                    if (WorldSprite.Y < 100)
                        _motion.TargetYSpeed = _rng.Generate(2);
                    else if (WorldSprite.Y > 112)
                        _motion.TargetYSpeed = -_rng.Generate(2);
                    else
                        _motion.TargetYSpeed = -8 + _rng.Generate(4);

                    if (WorldSprite.X < 16)
                        _motion.TargetXSpeed = _rng.Generate(2);
                    else if (WorldSprite.X < 48)
                        _motion.TargetXSpeed = -_rng.Generate(2);
                    else
                        _motion.TargetXSpeed = -8 + _rng.Generate(4);

                    _motion.XAcceleration = 1;
                    _motion.YAcceleration = 1;
                }

                _motionController.Update();

                if (WorldSprite.Bounds.Intersects(_playerController.WorldSprite.Bounds))
                {
                    _audioService.PlaySound(ChompAudioService.Sound.Reward);
                    _collected.Value = true;
                }
            }
            else if(WorldSprite.Visible)
            {
                var center = _playerController.WorldSprite.Center;

                byte b = _levelTimer.Value;
                var angle = 360 * (b / 255.0);

                angle = (angle + (45 * Index)) % 360;

                var radius = 12;
                if (_expanding)
                    radius = 12 + _levelTimer.Value / 8;

                if (_expanding && _levelTimer.Value == 255)
                    WorldSprite.Visible = false;

                Point offset = new Point(0, radius).RotateDeg((int)angle);
                WorldSprite.X = (byte)(center.X + offset.X);
                WorldSprite.Y = (byte)(center.Y + offset.Y);

                _bullets.Execute(b =>
                {
                    if (b.WorldSprite.Bounds.Intersects(WorldSprite.Bounds))
                    {
                        b.Destroy();
                        _audioService.PlaySound(ChompAudioService.Sound.EnemyHit);
                    }
                });
            }
        }

        protected override void HandleFall()
        {
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _motion.XSpeed = 0;
            _motion.YSpeed = 0;
            _collected.Value = false;
            Palette = SpritePalette.Enemy2;
            GetSprite().Palette = Palette;
        }

        protected override void UpdateDying() { }

        public bool CollidesWithPlayer(PlayerController player) => false;
        public bool CollidesWithBomb(WorldSprite bomb) => false;
        public CollisionResult HandlePlayerCollision(WorldSprite player) => CollisionResult.None;
        public BombCollisionResponse HandleBombCollision(WorldSprite player) => BombCollisionResponse.None;
    }
}
