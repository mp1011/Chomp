using ChompGame.Data.Memory;
using ChompGame.Helpers;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers.MotionControllers
{
    class SimpleMotionController : IMotionController
    {
        private SpriteDefinition _spriteDefinition;

        IMotion IMotionController.Motion => Motion;

        public PrecisionMotion Motion { get; }

        public byte Speed =>
            _spriteDefinition.MovementSpeed switch {
                MovementSpeed.VerySlow => 1,
                MovementSpeed.Slow => 10,
                MovementSpeed.Fast => 40,
                MovementSpeed.VeryFast => 60,
                _ => 0
            };
        public WorldSprite WorldSprite { get; }

        public SimpleMotionController(SystemMemoryBuilder memoryBuilder, WorldSprite worldSprite, SpriteDefinition spriteDefinition)
        {
            _spriteDefinition = spriteDefinition;
            Motion = new PrecisionMotion(memoryBuilder);
            WorldSprite = worldSprite;
        }

        public void AfterCollision(CollisionInfo collisionInfo)
        {

        }

        public void Update()
        {
            Motion.Apply(WorldSprite);
        }
    }
}
