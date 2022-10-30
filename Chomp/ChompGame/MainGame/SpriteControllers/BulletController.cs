using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.Helpers;

namespace ChompGame.MainGame.SpriteControllers
{
    class BulletController : ISpriteController, ICollidesWithPlayer
    {
        private readonly CoreGraphicsModule _coreGraphicsModule;
        private readonly MovingSpriteController _movingSpriteController;
        private GameByte _bulletTimer;
        public AcceleratedMotion Motion => _movingSpriteController.Motion;
        public Sprite GetSprite() => _movingSpriteController.GetSprite();
        public WorldSprite WorldSprite => _movingSpriteController.WorldSprite;

        public byte SpriteIndex
        {
            get => _movingSpriteController.SpriteIndex;
            set
            {
                _movingSpriteController.SpriteIndex = value;
                _bulletTimer.Value = 0;
            }
        }

        public BulletController(
            SpritesModule spritesModule,
            CollisionDetector collisionDetector,
            GameByte levelTimer,
            SystemMemoryBuilder memoryBuilder)
        {
            _bulletTimer = memoryBuilder.AddByte();
            _coreGraphicsModule = spritesModule.GameSystem.CoreGraphicsModule;

            _movingSpriteController = new MovingSpriteController(
                spritesModule,
                collisionDetector,
                levelTimer,
                memoryBuilder,
                255,
                20,
                0,
                0,
                0,
                0,
                0);
        }

        public void Update()
        {
            if (_bulletTimer.Value == 0)
            {
                _movingSpriteController.Motion.YSpeed = 0;
                _movingSpriteController.Motion.TargetYSpeed = 0;

                var sprite = GetSprite();
                if (sprite.FlipX)
                {
                    _movingSpriteController.Motion.XSpeed = -_movingSpriteController.WalkSpeed;
                    _movingSpriteController.Motion.TargetXSpeed = -_movingSpriteController.WalkSpeed; 
                }
                else
                {
                    _movingSpriteController.Motion.XSpeed = _movingSpriteController.WalkSpeed;
                    _movingSpriteController.Motion.TargetXSpeed = _movingSpriteController.WalkSpeed;
                }
            }

            _movingSpriteController.Update();
            _bulletTimer.Value++;

            if(_bulletTimer.Value == 100)
            {
                GetSprite().Tile = 0;
                SpriteIndex = 255;
            }
        }

        public void HandleCollision(WorldSprite player)
        {
            GetSprite().Tile = 0;
            SpriteIndex = 255;
        }
    }
}
