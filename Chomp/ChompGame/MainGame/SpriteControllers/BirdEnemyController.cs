using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class BirdEnemyController : ISpriteController, ICollidesWithPlayer
    {
        private const int _hoverSpeed = 20;

        private readonly SpriteControllerPool<BulletController> _lizardBulletControllers;
        private readonly MovingSpriteController _movingSpriteController;
        private readonly WorldSprite _player;
        private GameByte _levelTimer;
        private GameByte _actionTimer;

        public WorldSprite WorldSprite => _movingSpriteController.WorldSprite;

        public AcceleratedMotion Motion => _movingSpriteController.Motion;
        public Sprite GetSprite() => _movingSpriteController.GetSprite();

        public byte SpriteIndex
        {
            get => _movingSpriteController.SpriteIndex;
            set => _movingSpriteController.SpriteIndex = value;
        }

        public BirdEnemyController(
            SpriteControllerPool<BulletController> lizardBulletControllers,
            WorldSprite player,
            SpritesModule spritesModule,
            GameByte levelTimer,
            SystemMemoryBuilder memoryBuilder)
        {
            _lizardBulletControllers = lizardBulletControllers;
            _levelTimer = levelTimer;
            _actionTimer = memoryBuilder.AddByte();
            _player = player;
            _movingSpriteController = new MovingSpriteController(
                spritesModule,
                levelTimer,
                memoryBuilder,
                spriteIndex: 255,
                spriteDefinition: new SpriteDefinition(SpriteType.Bird, memoryBuilder.Memory));
        }

        public void Update()
        {
            _movingSpriteController.Update();

            if((_levelTimer % 32) == 0)
            {
                _actionTimer.Value++;

                if (_actionTimer < 8)
                {
                    if ((_actionTimer % 2) == 0)
                        Motion.TargetYSpeed = _hoverSpeed;
                    else
                        Motion.TargetYSpeed = -_hoverSpeed;

                    WorldSprite.FlipX = _player.X < WorldSprite.X;
                }
                else if(_actionTimer >= 8 && _actionTimer < 14)
                {
                    Motion.TargetTowards(WorldSprite, _player, _movingSpriteController.WalkSpeed);
                }
                else if (_actionTimer >= 14)
                {
                    Motion.TargetXSpeed = 0;
                    Motion.TargetYSpeed = -_hoverSpeed;

                    if (WorldSprite.Y <= 32) //todo avoid hard coding
                    {
                        _actionTimer.Value = 0;
                        Motion.TargetYSpeed = 0;
                    }
                }
            }
        }

        public void HandleCollision(WorldSprite player)
        {
        }

        public void ConfigureSprite(Sprite sprite)
        {
            _movingSpriteController.ConfigureSprite(sprite);          
            Motion.TargetXSpeed = 0;
            Motion.TargetYSpeed = 0;
            Motion.XSpeed = 0;
            Motion.YSpeed = 0;

            Motion.XAcceleration = _movingSpriteController.WalkAccel;
            Motion.YAcceleration = _movingSpriteController.WalkAccel;

            sprite.Palette = 2;
        }
    }

}
