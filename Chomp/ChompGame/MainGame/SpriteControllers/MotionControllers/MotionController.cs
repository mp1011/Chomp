using ChompGame.Helpers;

namespace ChompGame.MainGame.SpriteControllers.Base
{
    interface IMotionController
    {
        IMotion Motion { get; }

        byte Speed { get; }

        void Update();
        void AfterCollision(CollisionInfo collisionInfo);
    }

 }
