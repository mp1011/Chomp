using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers.MotionControllers
{
   class ActorMotionController : IMotionController
    {
        private SpriteDefinition _spriteDefinition;
        private SpriteTileTable _spriteTileTable;

        public WorldSpriteStatus Status
        {
            get => WorldSprite.Status;
            set => WorldSprite.Status = value;
        }

        byte IMotionController.Speed => WalkSpeed;
        public byte WalkSpeed =>
            _spriteDefinition.MovementSpeed switch {
                MovementSpeed.VerySlow => 1,
                MovementSpeed.Slow => 5,
                MovementSpeed.Fast => 40,
                MovementSpeed.VeryFast => 60,
                _ => 0
            };

        public byte WalkAccel =>
            _spriteDefinition.MovementSpeed switch {
                MovementSpeed.VerySlow => 1,
                MovementSpeed.Slow => 5,
                MovementSpeed.Fast => 10,
                MovementSpeed.VeryFast => 10,
                _ => 0
            };

        public byte BrakeAccel =>
            _spriteDefinition.MovementSpeed switch {
                MovementSpeed.VerySlow => 1,
                MovementSpeed.Slow => 5,
                MovementSpeed.Fast => 60,
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

        IMotion IMotionController.Motion => Motion;
        public AcceleratedMotion Motion { get; }
        public WorldSprite WorldSprite { get; }
        public byte SpriteIndex
        {
            get => WorldSprite.SpriteIndex.Value;
            set => WorldSprite.SpriteIndex.Value = value;
        }

        public Sprite GetSprite() => WorldSprite.GetSprite();

        public void ConfigureSprite(Sprite s) => WorldSprite.ConfigureSprite(s);


        public ActorMotionController(ChompGameModule gameModule,
            SystemMemoryBuilder memoryBuilder,
            SpriteType spriteType,
            WorldSprite worldSprite)
            : this(gameModule.SpritesModule, gameModule.SpriteTileTable, gameModule.LevelTimer, memoryBuilder,
                  new SpriteDefinition(spriteType, memoryBuilder.Memory), worldSprite)
        { }

        public ActorMotionController(
            SpritesModule spritesModule,
            SpriteTileTable spriteTileTable,
            GameByte levelTimer,
            SystemMemoryBuilder memoryBuilder,
            SpriteDefinition spriteDefinition,
            WorldSprite worldSprite)
        {
            _spriteDefinition = spriteDefinition;
            _spritesModule = spritesModule;
            _levelTimer = levelTimer;
            _spriteTileTable = spriteTileTable;
            WorldSprite = worldSprite;
            Motion = new AcceleratedMotion(levelTimer, memoryBuilder);
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

            if (_spriteDefinition.StopsAtLedges && collisionInfo.LedgeHeight > 2)
            {
                if (Motion.XSpeed < 0
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
            Update(true);
        }

        public void Update(bool ignoreUnlessActive)
        {
            if (ignoreUnlessActive && WorldSprite.Status != WorldSpriteStatus.Active)
                return;

            Motion.Apply(WorldSprite);
        }
    }

}
