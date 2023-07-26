using ChompGame.Data;
using ChompGame.MainGame.SpriteControllers.Base;
using System;

namespace ChompGame.MainGame.SpriteControllers
{
    class EmptyEnemyController : IEnemyOrBulletSpriteControllerPool
    {
        public static EmptyEnemyController Instance { get; } = new EmptyEnemyController();

        public bool CanAddNew() => false;
        public void Execute(Action<IEnemyOrBulletSpriteController> action, bool skipIfInactive = true) { }
        public void Execute(Action<ISpriteController> action, bool skipIfInactive = true) { }
        public ISpriteController TryAddNew() => null;
    }
}
