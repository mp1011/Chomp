using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.Helpers;

namespace ChompGame.MainGame.SpriteControllers
{
    class PlayerController
    {
        private  readonly CollisionDetector _collisionDetector;
        private readonly MovingSpriteController _walkingSpriteController;
        private readonly InputModule _inputModule;
        public WorldSprite WorldSprite => _walkingSpriteController.WorldSprite;
        public AcceleratedMotion Motion => _walkingSpriteController.Motion;

        public PlayerController(
            SpritesModule spritesModule, 
            InputModule inputModule, 
            CollisionDetector collisionDetector,
            GameByte levelTimer,
            SystemMemoryBuilder memoryBuilder)
        {
            _walkingSpriteController = new MovingSpriteController(
                spritesModule,
                collisionDetector,
                levelTimer,
                memoryBuilder,
                0,
                40,
                5,
                10,
                80,
                64,
                10);
        
            _inputModule = inputModule;
            _collisionDetector = collisionDetector;
        }

        public void Update()
        {
            var motion = _walkingSpriteController.Motion;

            _walkingSpriteController.Update();
            _inputModule.OnLogicUpdate();

            var collisionInfo = _collisionDetector.DetectCollisions(WorldSprite, 14); //todo, hard-coding

            if (_inputModule.Player1.RightKey.IsDown())
            {
                motion.TargetXSpeed = _walkingSpriteController.WalkSpeed;
                motion.XAcceleration = _walkingSpriteController.WalkAccel;
            }
            else if (_inputModule.Player1.LeftKey.IsDown())
            {
                motion.TargetXSpeed = -_walkingSpriteController.WalkSpeed;
                motion.XAcceleration = _walkingSpriteController.WalkAccel;
            }
            else
            {
                motion.TargetXSpeed = 0;
                motion.XAcceleration = _walkingSpriteController.BrakeAccel;
            }

            if(collisionInfo.IsOnGround && _inputModule.Player1.AKey == GameKeyState.Pressed)
            {
                motion.YSpeed = -_walkingSpriteController.JumpSpeed;
            }
        }

        public void CheckCollisions<T>(SpriteControllerPool<T> sprites)
            where T : class, ISpriteController, ICollidesWithPlayer
        {
            sprites.Execute(p =>
            {
                if(p.WorldSprite.Bounds.Intersects(WorldSprite.Bounds))
                {
                    p.HandleCollision(WorldSprite);
                }
            });
        }
    }
}
