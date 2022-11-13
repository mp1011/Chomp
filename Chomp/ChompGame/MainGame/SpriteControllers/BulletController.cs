using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.MainGame.SpriteModels;

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
            GameByte levelTimer,
            SystemMemoryBuilder memoryBuilder, 
            SpriteType spriteType)
        {
            _bulletTimer = memoryBuilder.AddByte();
            _coreGraphicsModule = spritesModule.GameSystem.CoreGraphicsModule;

            _movingSpriteController = new MovingSpriteController(
                spritesModule,
                levelTimer,
                memoryBuilder,
                spriteIndex: 255,
                new SpriteDefinition(spriteType, memoryBuilder.Memory));
        }

        public void Update()
        {
            var sprite = GetSprite();

            if (_bulletTimer.Value == 0)
            {
                _movingSpriteController.Motion.YSpeed = 0;
                _movingSpriteController.Motion.TargetYSpeed = 0;

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

            if(_bulletTimer.Value == 100 || _bulletTimer.Value == 150)
            {
                sprite.Tile = 0;
                SpriteIndex = 255;
            }

            if(_bulletTimer.Value >= 101)
            {
                sprite.Tile = (byte)(6 + (_bulletTimer.Value % 3));
            }
        }

        public void HandleCollision(WorldSprite player)
        {
            if (_bulletTimer.Value > 100)
                return;

            _bulletTimer.Value = 101;
            _movingSpriteController.Motion.XSpeed = 0;
            _movingSpriteController.Motion.YSpeed = 0;
            _movingSpriteController.Motion.TargetXSpeed = 0;
            _movingSpriteController.Motion.TargetYSpeed = 0;
        }

        public void ConfigureSprite(Sprite sprite)
        {
            _movingSpriteController.ConfigureSprite(sprite);
            sprite.Palette = 3;
        }
    }
}
