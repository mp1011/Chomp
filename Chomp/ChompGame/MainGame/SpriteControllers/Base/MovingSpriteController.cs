using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers.Base
{
    class MovingSpriteController// : ISpriteController
    {
        private SpriteDefinition _spriteDefinition;

        public WorldSpriteStatus Status
        {
            get => WorldSprite.Status;
            set => WorldSprite.Status = value;
        }

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
        public MovingWorldSprite WorldSprite { get; }
        public byte SpriteIndex
        {
            get => WorldSprite.SpriteIndex.Value;
            set => WorldSprite.SpriteIndex.Value = value;
        }

        public Sprite GetSprite() => WorldSprite.GetSprite();

        public void ConfigureSprite(Sprite s) => WorldSprite.ConfigureSprite(s);

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

            WorldSprite = new MovingWorldSprite(
                specs: _spritesModule.Specs,
                spriteDefinition: spriteDefinition,
                memoryBuilder: memoryBuilder,
                spritesModule: _spritesModule,
                motion: Motion.CurrentMotion,
                scroller: worldScroller);          
        }

        public void AfterCollision(CollisionInfo collisionInfo)
        {
            if (FallSpeed != 0)
            {
                if (collisionInfo.YCorrection < 0 || collisionInfo.IsOnGround)
                {
                    Motion.YSpeed = 0;
                    Motion.TargetYSpeed = 0;
                }
                else
                {
                    Motion.TargetYSpeed = FallSpeed;
                    Motion.YAcceleration = GravityAccel;
                }
            }

            if(_spriteDefinition.StopsAtLedges && collisionInfo.LedgeHeight > 2)
            {
                if(Motion.XSpeed < 0 
                    && collisionInfo.LeftLedge)
                {
                    Motion.XSpeed = 0;
                    Motion.TargetXSpeed = WalkSpeed;
                }
                else if (Motion.XSpeed > 0 
                    && collisionInfo.RightLedge)
                {
                    Motion.XSpeed = 0;
                    Motion.TargetXSpeed = -WalkSpeed;
                }
            }

            if (Motion.XSpeed < 0
                   && collisionInfo.HitLeftWall)
            {
                Motion.XSpeed = 0;
                Motion.TargetXSpeed = WalkSpeed;
            }
            else if (Motion.XSpeed > 0
                && collisionInfo.HitRightWall)
            {
                Motion.XSpeed = 0;
                Motion.TargetXSpeed = -WalkSpeed;
            }

            if (collisionInfo.YCorrection > 0 || collisionInfo.IsOnGround)
            {
                Motion.YSpeed = 0;
            }

            if (collisionInfo.XCorrection != 0)
            {
                Motion.XSpeed = 0;
            }

        }

        public void Update()
        {
            if (WorldSprite.Status != WorldSpriteStatus.Active)
                return;

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
                sprite.Tile2Offset = 1;
            }
            else if ((_levelTimer.Value % 16) == 0)
            {
                if(_spriteDefinition.AnimationStyle == AnimationStyle.AnimateLowerTileOnly)
                    sprite.Tile2Offset = sprite.Tile2Offset.Toggle(1,2);
                else
                    sprite.Tile = sprite.Tile.Toggle(_spriteDefinition.Tile, (byte)(_spriteDefinition.Tile + sprite.SizeX));
            }
        }

       
    }
}
