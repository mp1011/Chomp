using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.Helpers;

namespace ChompGame.MainGame.SpriteControllers
{
    class LizardEnemyController : ISpriteController
    {
        private readonly WalkingSpriteController _walkingSpriteController;
        public AcceleratedMotion Motion => _walkingSpriteController.Motion;
        public Sprite GetSprite() => _walkingSpriteController.GetSprite();

        public byte SpriteIndex
        {
            get => _walkingSpriteController.SpriteIndex;
            set => _walkingSpriteController.SpriteIndex = value;
        }

        public LizardEnemyController(
            SpritesModule spritesModule,
            CollisionDetector collisionDetector,
            GameByte levelTimer,
            SystemMemoryBuilder memoryBuilder)
        {
            _walkingSpriteController = new WalkingSpriteController(
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

            if((_walkingSpriteController.LevelTimer % 128) == SpriteIndex)
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
        }
    }

}
