using ChompGame.Data;
using ChompGame.GameSystem;

namespace ChompGame.MainGame.SpriteControllers
{
    public enum GravityStrength : byte
    {
        None,
        Low,
        Medium,
        High
    }

    public enum MovementSpeed : byte
    {
        VerySlow,
        Slow,
        Fast,
        VeryFast
    }

    public enum AnimationStyle : byte
    {
        NoAnimatin,
        AnimateWhenMoving,
        AnimateLowerTileOnly,
        AlwaysAnimate
    }

    class MovingSpriteController : ISpriteController
    {
        private GameByte _settings;

        private TwoBitEnum<GravityStrength> _gravityStrength;
        private TwoBitEnum<AnimationStyle> _animationStyle;
        private TwoBitEnum<MovementSpeed> _movementSpeed;
        private GameBit _collidesWithBackground;
        private GameBit _flipXWhenMovingLeft;
        

        public MovementSpeed MovementSpeed
        {
            get => _movementSpeed.Value;
            set
            {
                _movementSpeed.Value = value;

                switch (_movementSpeed.Value)
                {
                    case MovementSpeed.VerySlow:
                        WalkSpeed = 1;
                        WalkAccel = 1;
                        BrakeAccel = 1;
                        break;
                    case MovementSpeed.Slow:
                        WalkSpeed = 10;
                        WalkAccel = 5;
                        BrakeAccel = 5;
                        break;
                    case MovementSpeed.Fast:
                        WalkSpeed = 40;
                        WalkAccel = 5;
                        BrakeAccel = 10;
                        break;
                    case MovementSpeed.VeryFast:
                        WalkSpeed = 60;
                        WalkAccel = 10;
                        BrakeAccel = 10;
                        break;
                }
            }
        }

        public GravityStrength GravityStrength
        {
            get => _gravityStrength.Value;
            set
            {
                _gravityStrength.Value = value;
                switch (_gravityStrength.Value)
                {
                    case GravityStrength.None:
                        FallSpeed = 0;
                        GravityAccel = 0;
                        JumpSpeed = 0;
                        break;
                    case GravityStrength.Low:
                        FallSpeed = 20;
                        GravityAccel = 5;
                        JumpSpeed = 80;
                        break;
                    case GravityStrength.Medium:
                        FallSpeed = 64;
                        GravityAccel = 10;
                        JumpSpeed = 80;
                        break;
                    case GravityStrength.High:
                        FallSpeed = 127;
                        GravityAccel = 10;
                        JumpSpeed = 30;
                        break;
                }
            }
        }

        public byte WalkSpeed { get; private set; }
        public byte WalkAccel { get; private set; }
        public byte BrakeAccel { get; private set; }
        public byte JumpSpeed { get; private set; }
        public byte FallSpeed { get; private set; }
        public byte GravityAccel { get; private set; }

        private readonly GameByte _levelTimer;
        private readonly SpritesModule _spritesModule;

        public byte LevelTimer => _levelTimer.Value;

        public AcceleratedMotion Motion { get; }
        public WorldSprite WorldSprite { get; }
        public byte SpriteIndex
        {
            get => WorldSprite.SpriteIndex.Value;
            set => WorldSprite.SpriteIndex.Value = value;
        }

        public Sprite GetSprite() => _spritesModule.GetSprite(SpriteIndex);

        public MovingSpriteController(
            SpritesModule spritesModule,
            GameByte levelTimer,
            SystemMemoryBuilder memoryBuilder,
            byte spriteIndex,
            GravityStrength gravityStrength,
            MovementSpeed movementSpeed,
            AnimationStyle animationStyle,
            bool collidesWithBackground,
            bool flipXWhenMovingLeft)
        {
            _spritesModule = spritesModule;
            _levelTimer = levelTimer;

            Motion = new AcceleratedMotion(levelTimer, memoryBuilder);

            _settings = memoryBuilder.AddByte();
            _gravityStrength = new TwoBitEnum<GravityStrength>(memoryBuilder.Memory, _settings.Address, 0);
            _movementSpeed = new TwoBitEnum<MovementSpeed>(memoryBuilder.Memory, _settings.Address, 2);
            _animationStyle = new TwoBitEnum<AnimationStyle>(memoryBuilder.Memory, _settings.Address, 4);
            _collidesWithBackground = new GameBit(_settings.Address, Bit.Bit6, memoryBuilder.Memory);
            _flipXWhenMovingLeft = new GameBit(_settings.Address, Bit.Bit7, memoryBuilder.Memory);

            GravityStrength = gravityStrength;
            MovementSpeed = movementSpeed;

            _animationStyle.Value = animationStyle;
            _collidesWithBackground.Value = collidesWithBackground;
            _flipXWhenMovingLeft.Value = flipXWhenMovingLeft;

            WorldSprite = new WorldSprite(
                specs: _spritesModule.Specs,
                spritesModule: _spritesModule,
                spriteIndex: memoryBuilder.AddByte(spriteIndex),
                worldBlock: memoryBuilder.AddNibblePoint(),
                motion: Motion.CurrentMotion);          
        }

        public void Update()
        {
            Motion.TargetYSpeed = FallSpeed;
            Motion.YAcceleration = GravityAccel;

            var sprite = WorldSprite.GetSprite();
            Motion.Apply(WorldSprite);

            if (Motion.TargetXSpeed < 0 && !sprite.FlipX)
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
