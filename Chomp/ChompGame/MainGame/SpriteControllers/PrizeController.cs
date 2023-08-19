using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Helpers;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteControllers.MotionControllers;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class PrizeController : ActorController, IAutoScrollSpriteController
    {
        public const int ExtraBomb = 0;
        public const int Coin3 = 1;
        public const int Coin5Diag = 2;
        public const int Coin5Diag2 = 3;

        private readonly RewardsModule _rewardsModule;
        public const byte HealthPerPickup = 2;
        private CollisionDetector _collisionDetector;
        private IMotionController _motionController;
        private AcceleratedMotion _motion;
        private ChompAudioService _audioService;
        private StatusBar _statusBar;

        public override IMotion Motion => _motion;

        private LowNibble _variation;
        private HighNibble _delay;
        public byte Variation
        {
            get => _variation.Value;
            set => _variation.Value = value;
        }

        public void AfterSpawn(ISpriteControllerPool pool)
        {
            _delay.Value = 0;

            if(_variation.Value == Coin3)
            {
                SpawnExtra(pool, WorldSprite.X, WorldSprite.Y, 2);
                SpawnExtra(pool, WorldSprite.X, WorldSprite.Y, 4);
            }
            else if (_variation.Value == Coin5Diag)
            {
                SpawnExtra(pool, WorldSprite.X, WorldSprite.Y + 4, 2);
                SpawnExtra(pool, WorldSprite.X, WorldSprite.Y + 8, 4);
                SpawnExtra(pool, WorldSprite.X, WorldSprite.Y + 12, 6);
                SpawnExtra(pool, WorldSprite.X, WorldSprite.Y + 16, 8);
            }
            else if (_variation.Value == Coin5Diag2)
            {
                SpawnExtra(pool, WorldSprite.X, WorldSprite.Y - 4, 2);
                SpawnExtra(pool, WorldSprite.X, WorldSprite.Y - 8, 4);
                SpawnExtra(pool, WorldSprite.X, WorldSprite.Y - 12, 6);
                SpawnExtra(pool, WorldSprite.X, WorldSprite.Y - 16, 8);
            }
        }

        private PrizeController SpawnExtra(ISpriteControllerPool pool, int x, int y, int delay)
        {
            var sprite = pool.TryAddNew();
            if (sprite == null)
                return null;
           
            PrizeController pc = sprite as PrizeController;
            pc.WorldSprite.X = x;
            pc.WorldSprite.Y = y;
            pc.WorldSprite.UpdateSprite();
            pc.Variation = Variation;
            pc._delay.Value = (byte)delay;
            return pc;
        }

        public PrizeController(ChompGameModule gameModule, SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Prize, gameModule, memoryBuilder, SpriteTileIndex.Prize)
        {
            _rewardsModule = gameModule.RewardsModule;
            var motionController = new ActorMotionController(gameModule.SpritesModule, gameModule.SpriteTileTable,
                 gameModule.LevelTimer, memoryBuilder, new SpriteDefinition(SpriteType.Prize, memoryBuilder.Memory), WorldSprite);

            _variation = new LowNibble(memoryBuilder);
            _delay = new HighNibble(memoryBuilder);   
             memoryBuilder.AddByte();

            _motionController = motionController;
            _motion = motionController.Motion;
            _collisionDetector = gameModule.CollissionDetector;
            _audioService = gameModule.AudioService;
            _statusBar = gameModule.StatusBar;
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            Visible = true;
        }

        protected override void UpdateActive()
        {
            if(_delay.Value > 0)
            {
                if (_levelTimer.IsMod(8))
                    _delay.Value--;

                return;
            }

            if (_variation.Value == ExtraBomb)
                Update_ExtraBomb();
            else
                Update_Coin();
        }

        private void Update_ExtraBomb()
        {
            if (_motion.YSpeed == 0)
            {
                _motion.TargetXSpeed = -10;
                _motion.XAcceleration = 5;
                _motion.SetYSpeed(2);
            }

            if (_levelTimer.Value.IsMod(24))
            {
                _motion.TargetXSpeed = -_motion.TargetXSpeed;
            }

            _motionController.Update();
            var collisionInfo = _collisionDetector.DetectCollisions(WorldSprite, _motion);
            if (collisionInfo.YCorrection != 0)
            {
                _motion.YSpeed = -10;
                _motion.YAcceleration = 1;
            }
        }

        private void Update_Coin()
        {
            _motion.SetXSpeed(-40);
            _motion.SetYSpeed(0);
            _motionController.Update();
        }

        public void CheckPlayerCollision(PlayerController playerController)
        {
            if(playerController.WorldSprite.Bounds.Intersects(WorldSprite.Bounds))
            {
                Destroy();

                if (_variation.Value == ExtraBomb)
                    OnCollected_ExtraBomb();
                else
                    OnCollected_Coin();
            }
        }

        private void OnCollected_ExtraBomb()
        {
            if (_statusBar.Health < StatusBar.FullHealth - HealthPerPickup)
                _statusBar.Health += HealthPerPickup;
            else
                _statusBar.Health = StatusBar.FullHealth;

            _audioService.PlaySound(ChompAudioService.Sound.ButtonPress);
        }

        private void OnCollected_Coin()
        {
            _statusBar.AddToScore(25);
            _rewardsModule.CheckRewards(1);
            _audioService.PlaySound(ChompAudioService.Sound.CollectCoin);
        }

    }
}
