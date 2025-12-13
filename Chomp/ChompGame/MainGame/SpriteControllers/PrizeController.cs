using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteControllers.MotionControllers;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class PrizeController : ActorController, IAutoScrollSpriteController
    {
        private ChompGameModule _gameModule;
        public const int ExtraHealth = 0;
        public const int Coin3 = 1;
        public const int Coin5Diag = 2;
        public const int Coin5Diag2 = 3;

        private readonly RewardsModule _rewardsModule;
        public const byte HealthPerPickup = 4;
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
        protected override bool DestroyWhenOutOfBounds => true;
       
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

            _gameModule = gameModule;
            _rewardsModule = gameModule.RewardsModule;
            var motionController = new ActorMotionController(gameModule.SpritesModule, gameModule.SpriteTileTable,
                 gameModule.LevelTimer, memoryBuilder, new SpriteDefinition(SpriteType.Prize, memoryBuilder.Memory), WorldSprite);

            _variation = new LowNibble(memoryBuilder);
            _delay = new HighNibble(memoryBuilder);   
             memoryBuilder.AddByte();

            _motionController = motionController;
            _motion = motionController.Motion;
            _collisionDetector = gameModule.CollisionDetector;
            _audioService = gameModule.AudioService;
            _statusBar = gameModule.StatusBar;
            Palette = SpritePalette.Platform;
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

            if (_variation.Value == ExtraHealth)
                Update_ExtraHealth();
            else
                Update_Coin();
        }

        private void Update_ExtraHealth()
        {
            if (_gameModule.CurrentScene.IsAutoScroll)
            {
                _motion.XSpeed = -8;
                _motion.TargetXSpeed = -8;
            }
            else
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
            if(playerController.CollidesWith(WorldSprite))
            {
                Destroy();

                if (_variation.Value == ExtraHealth)
                    OnCollected_ExtraHealth();
                else
                    OnCollected_Coin();
            }
        }

        private void OnCollected_ExtraHealth()
        {
            if (_statusBar.Health < StatusBar.FullHealth - HealthPerPickup)
                _statusBar.Health += HealthPerPickup;
            else
                _statusBar.Health = StatusBar.FullHealth;

            _audioService.PlaySound(ChompAudioService.Sound.ButtonPress);
        }

        private void OnCollected_Coin()
        {
            if(_gameModule.CurrentLevel != Level.Level2_2_Fly2)
                _statusBar.AddToScore(25);

            _rewardsModule.CheckRewards(1);
            _audioService.PlaySound(ChompAudioService.Sound.CollectCoin);
        }

    }
}
