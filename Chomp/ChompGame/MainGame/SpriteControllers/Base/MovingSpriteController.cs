using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers.Base
{
    class MovingSpriteController : ISpriteController
    {
        public SpriteDefinition _spriteDefinition;

        public byte WalkSpeed =>
            _spriteDefinition.MovementSpeed switch 
            {
                MovementSpeed.VerySlow => 1,
                MovementSpeed.Slow => 10,
                MovementSpeed.Fast => 40,
                MovementSpeed.VeryFast => 60,
                _ => 0
            };

        public byte WalkAccel =>
            _spriteDefinition.MovementSpeed switch {
                MovementSpeed.VerySlow => 1,
                MovementSpeed.Slow => 5,
                MovementSpeed.Fast => 5,
                MovementSpeed.VeryFast => 10,
                _ => 0
            };

        public byte BrakeAccel =>
            _spriteDefinition.MovementSpeed switch {
                MovementSpeed.VerySlow => 1,
                MovementSpeed.Slow => 5,
                MovementSpeed.Fast => 10,
                MovementSpeed.VeryFast => 10,
                _ => 0
            };

        public byte JumpSpeed =>
            _spriteDefinition.GravityStrength switch {
                GravityStrength.None => 0,
                GravityStrength.Low => 80,
                GravityStrength.Medium => 80,
                GravityStrength.High => 30,
                _ => 0
            };

        public byte FallSpeed =>
            _spriteDefinition.GravityStrength switch {
                GravityStrength.None => 0,
                GravityStrength.Low => 20,
                GravityStrength.Medium => 64,
                GravityStrength.High => 127,
                _ => 0
            };

        public byte GravityAccel =>
            _spriteDefinition.GravityStrength switch {
                GravityStrength.None => 0,
                GravityStrength.Low => 5,
                GravityStrength.Medium => 10,
                GravityStrength.High => 10,
                _ => 0
            };

        private readonly GameByte _levelTimer;
        private readonly SpritesModule _spritesModule;

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
            SpriteDefinition spriteDefinition,
            WorldScroller worldScroller)
        {
            _spriteDefinition = spriteDefinition;
            _spritesModule = spritesModule;
            _levelTimer = levelTimer;

            Motion = new AcceleratedMotion(levelTimer, memoryBuilder);

            WorldSprite = new WorldSprite(
                specs: _spritesModule.Specs,
                spritesModule: _spritesModule,
                spriteIndex: memoryBuilder.AddByte(spriteIndex),
                motion: Motion.CurrentMotion,
                scroller: worldScroller,
                position: memoryBuilder.AddExtendedPoint());          
        }

        public void Update()
        {
            if (FallSpeed != 0)
            {
                Motion.TargetYSpeed = FallSpeed;
                Motion.YAcceleration = GravityAccel;
            }

            var sprite = WorldSprite.GetSprite();
            Motion.Apply(WorldSprite);

            if (_spriteDefinition.FlipXWhenMovingLeft)
            {
                if (Motion.TargetXSpeed < 0 && !sprite.FlipX)
                {
                    sprite.FlipX = true;
                }
                else if (Motion.TargetXSpeed > 0 && sprite.FlipX)
                {
                    sprite.FlipX = false;
                }
            }

            bool shouldAnimate = _spriteDefinition.AnimationStyle switch {
                AnimationStyle.AlwaysAnimate => true,
                AnimationStyle.AnimateWhenMoving => Motion.XSpeed != 0,
                AnimationStyle.AnimateLowerTileOnly => Motion.XSpeed != 0,
                _ => false
            };

            if(!shouldAnimate)
            {
                sprite.Tile = _spriteDefinition.Tile;
            }
            else if ((_levelTimer.Value % 16) == 0)
            {
                if(_spriteDefinition.AnimationStyle == AnimationStyle.AnimateLowerTileOnly)
                    sprite.Tile2Offset = sprite.Tile2Offset.Toggle(1,2);
                else
                    sprite.Tile = sprite.Tile.Toggle(_spriteDefinition.Tile, (byte)(_spriteDefinition.Tile + sprite.SizeX));
            }
        }

        public void ConfigureSprite(Sprite sprite)
        {
            sprite.Tile = _spriteDefinition.Tile;
            sprite.Tile2Offset = _spriteDefinition.SecondTileOffset;
            sprite.SizeX = _spriteDefinition.SizeX;
            sprite.SizeY = _spriteDefinition.SizeY;
        }
    }
}
