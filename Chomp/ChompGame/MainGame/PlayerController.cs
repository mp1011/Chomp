using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Helpers;

namespace ChompGame.MainGame
{
    class PlayerController
    {
        private const int WalkSpeed = 40;
        private const int WalkAccel = 5;
        private const int BrakeAccel = 10;

        public const int JumpSpeed = 80;
        private const int FallSpeed = 64;
        private const int GravityAccel = 10;

        private readonly GameByte _levelTimer;
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
            _levelTimer = levelTimer;

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

            var sprite = WorldSprite.GetSprite();
            Motion.Apply(WorldSprite);

            //todo, this value should come from level data
            var collisionInfo = _collisionDetector.DetectCollisions(WorldSprite, 14);

            if(collisionInfo.IsOnGround && _inputModule.Player1.AKey == GameKeyState.Pressed)
            {
                Motion.YSpeed = -JumpSpeed;
            }

            //split this out somehow
            if(Motion.XSpeed == 0)
            {
                sprite.Tile2Offset = 1;
            }
            else
            {
                if((_levelTimer.Value % 16) == 0)
                {
                    sprite.Tile2Offset = sprite.Tile2Offset.Toggle(1, 2);
                }
            }

            if(Motion.TargetXSpeed < 0 && !sprite.FlipX)
            {
                sprite.FlipX = true;
            }
            else if (Motion.TargetXSpeed > 0 && sprite.FlipX)
            {
                sprite.FlipX = false;
            }
        }
    }
}
