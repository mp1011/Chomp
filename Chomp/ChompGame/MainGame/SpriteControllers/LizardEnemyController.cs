using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.Helpers;

namespace ChompGame.MainGame.SpriteControllers
{
    class LizardEnemyController : ISpriteController
    {
        private readonly CollisionDetector _collisionDetector;
        private readonly SpriteControllerPool<BulletController> _lizardBulletControllers;
        private readonly MovingSpriteController _walkingSpriteController;
        private GameByte _bulletTimer;

        public AcceleratedMotion Motion => _walkingSpriteController.Motion;
        public Sprite GetSprite() => _walkingSpriteController.GetSprite();

        
        public byte SpriteIndex
        {
            get => _walkingSpriteController.SpriteIndex;
            set => _walkingSpriteController.SpriteIndex = value;
        }

        public LizardEnemyController(
            SpriteControllerPool<BulletController> lizardBulletControllers,
            SpritesModule spritesModule,
            CollisionDetector collisionDetector,
            GameByte levelTimer,
            SystemMemoryBuilder memoryBuilder)
        {
            _bulletTimer = memoryBuilder.AddByte();
            _lizardBulletControllers = lizardBulletControllers;
            _collisionDetector = collisionDetector;

            _walkingSpriteController = new MovingSpriteController(
                spritesModule, 
                collisionDetector, 
                levelTimer, 
                memoryBuilder,
                255,
                10,
                0,
                0,
                0,
                64,
                10);            
        }

        public void Update()
        {
            _walkingSpriteController.Update();
            _collisionDetector.DetectCollisions(_walkingSpriteController.WorldSprite, 14); //todo, hard-coding

            if ((_walkingSpriteController.LevelTimer % 128) == SpriteIndex)
            {
                var motion = _walkingSpriteController.Motion;

                if(motion.TargetXSpeed < 0)
                {
                    motion.TargetXSpeed = _walkingSpriteController.WalkSpeed;
                    motion.XSpeed = _walkingSpriteController.WalkSpeed;
                }
                else
                {
                    motion.TargetXSpeed = -_walkingSpriteController.WalkSpeed;
                    motion.XSpeed = -_walkingSpriteController.WalkSpeed;
                }
            }

            _bulletTimer.Value++;
            if(_bulletTimer.Value == 200 + SpriteIndex)
            {
                _bulletTimer.Value = 0;
                var fireball = _lizardBulletControllers.TryAddNew();
                if(fireball != null)
                {
                    var thisSprite = _walkingSpriteController.WorldSprite;
                    fireball.WorldSprite.X = thisSprite.X;
                    fireball.WorldSprite.Y = thisSprite.Y;
                    fireball.WorldSprite.FlipX = thisSprite.FlipX;
                }
            }
        }
    }

}
