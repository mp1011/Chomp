using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.Helpers;

namespace ChompGame.MainGame
{
    class PlayerController
    {
        private const int WalkSpeed = 40;
        private const int WalkAccel = 5;
        private const int BrakeAccel = 10;

        private const int FallSpeed = 64;
        private const int GravityAccel = 5;

        private readonly SpritesModule _spritesModule;
        private readonly InputModule _inputModule;
        private readonly CollisionDetector _collisionDetector;

        public AcceleratedMotion Motion { get; }
        public WorldSprite WorldSprite { get; }

        public PlayerController(
            SpritesModule spritesModule, 
            InputModule inputModule, 
            CollisionDetector collisionDetector,
            GameByte levelTimer,
            SystemMemoryBuilder memoryBuilder)
        {
            _spritesModule = spritesModule;
            _inputModule = inputModule;
            _collisionDetector = collisionDetector;

            Motion = new AcceleratedMotion(levelTimer, memoryBuilder);

            WorldSprite = new WorldSprite(
                specs: _spritesModule.Specs,
                spritesModule: _spritesModule,
                spriteIndex: memoryBuilder.AddByte(0),
                worldBlock: memoryBuilder.AddNibblePoint(),
                motion: Motion.CurrentMotion);
        }

        public void Update()
        {
            _inputModule.OnLogicUpdate();
            if (_inputModule.Player1.RightKey.IsDown())
            {
                Motion.TargetXSpeed = WalkSpeed;
                Motion.XAcceleration = WalkAccel;
            }
            else if (_inputModule.Player1.LeftKey.IsDown())
            {
                Motion.TargetXSpeed = -WalkSpeed;
                Motion.XAcceleration = WalkAccel;
            }
            else
            {
                Motion.TargetXSpeed = 0;
                Motion.XAcceleration = BrakeAccel;
            }

            Motion.TargetYSpeed = FallSpeed;
            Motion.YAcceleration = GravityAccel;

            Motion.Apply(WorldSprite.GetSprite());

            //todo, this value should come from level data
            _collisionDetector.DetectCollisions(WorldSprite, 14);
        }
    }
}
