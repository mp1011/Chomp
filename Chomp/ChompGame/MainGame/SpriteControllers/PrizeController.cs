using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Helpers;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteControllers.MotionControllers;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class PrizeController : ActorController
    {
        private CollisionDetector _collisionDetector;
        private IMotionController _motionController;
        private AcceleratedMotion _motion;
        private ChompAudioService _audioService;
        private StatusBar _statusBar;

        public override IMotion Motion => _motion;

        public PrizeController(ChompGameModule gameModule, SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Prize, gameModule, memoryBuilder, SpriteTileIndex.Prize)
        {

            var motionController = new ActorMotionController(gameModule.SpritesModule, gameModule.SpriteTileTable,
                 gameModule.LevelTimer, memoryBuilder, new SpriteDefinition(SpriteType.Prize, memoryBuilder.Memory), WorldSprite);

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
            if(_motion.YSpeed == 0)
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

        public void CheckPlayerCollision(PlayerController playerController)
        {
            if(playerController.WorldSprite.Bounds.Intersects(WorldSprite.Bounds))
            {
                Destroy();
                if(_statusBar.Health < StatusBar.FullHealth)
                    _statusBar.Health++;
                _audioService.PlaySound(ChompAudioService.Sound.ButtonPress);
            }
        }

    }
}
