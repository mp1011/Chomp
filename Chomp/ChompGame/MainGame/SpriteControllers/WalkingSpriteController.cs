using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Helpers;

namespace ChompGame.MainGame.SpriteControllers
{
    class WalkingSpriteController : ISpriteController
    {
        public byte WalkSpeed { get; }
        public byte WalkAccel { get; }
        public byte BrakeAccel { get; }
        public byte JumpSpeed { get; }
        public byte FallSpeed { get; }
        public byte GravityAccel { get; }

        private readonly GameByte _levelTimer;
        private readonly SpritesModule _spritesModule;
        private readonly CollisionDetector _collisionDetector;

        public byte LevelTimer => _levelTimer.Value;

        public AcceleratedMotion Motion { get; }
        public WorldSprite WorldSprite { get; }
        public byte SpriteIndex
        {
            get => WorldSprite.SpriteIndex.Value;
            set => WorldSprite.SpriteIndex.Value = value;
        }

        public Sprite GetSprite() => _spritesModule.GetSprite(SpriteIndex);

        public WalkingSpriteController(
            SpritesModule spritesModule,
            CollisionDetector collisionDetector,
            GameByte levelTimer,
            SystemMemoryBuilder memoryBuilder,
            byte spriteIndex,
            byte walkSpeed,
            byte walkAccel,
            byte brakeAccel,
            byte jumpSpeed,
            byte fallSpeed,
            byte gravityAccel)
        {
            _spritesModule = spritesModule;
            _collisionDetector = collisionDetector;
            _levelTimer = levelTimer;

            Motion = new AcceleratedMotion(levelTimer, memoryBuilder);

            WalkSpeed = walkSpeed;
            WalkAccel = walkAccel;
            BrakeAccel = brakeAccel;
            JumpSpeed = jumpSpeed;
            FallSpeed = fallSpeed;
            GravityAccel = gravityAccel;

            WorldSprite = new WorldSprite(
                specs: _spritesModule.Specs,
                spritesModule: _spritesModule,
                spriteIndex: memoryBuilder.AddByte(spriteIndex),
                worldBlock: memoryBuilder.AddNibblePoint(),
                motion: Motion.CurrentMotion);
        }

        public CollisionInfo Update()
        {
            Motion.TargetYSpeed = FallSpeed;
            Motion.YAcceleration = GravityAccel;

            var sprite = WorldSprite.GetSprite();
            Motion.Apply(WorldSprite);

            if (Motion.XSpeed == 0)
            {
                sprite.Tile2Offset = 1;
            }
            else
            {
                if ((_levelTimer.Value % 16) == 0)
                {
                    sprite.Tile2Offset = sprite.Tile2Offset.Toggle(1, 2);
                }
            }

            if (Motion.TargetXSpeed < 0 && !sprite.FlipX)
            {
                sprite.FlipX = true;
            }
            else if (Motion.TargetXSpeed > 0 && sprite.FlipX)
            {
                sprite.FlipX = false;
            }

            return _collisionDetector.DetectCollisions(WorldSprite, 14); //todo, hard-coding
        }
    }
}
