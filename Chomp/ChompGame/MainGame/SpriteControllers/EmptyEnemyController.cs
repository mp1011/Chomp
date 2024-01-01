using ChompGame.Data;
using ChompGame.MainGame.SpriteControllers.Base;
using System;

namespace ChompGame.MainGame.SpriteControllers
{
    class EmptyEnemyController : ICollidableSpriteControllerPool
    {
        public static EmptyEnemyController Instance { get; } = new EmptyEnemyController();

        public bool CanAddNew() => false;
        public bool CollidesWithPlayer(PlayerController player, WorldSprite sprite) => false;

        public void Execute(Action<ICollidableSpriteController> action, bool skipIfInactive = true) { }
        public void Execute(Action<ISpriteController> action, bool skipIfInactive = true) { }
        public ISpriteController TryAddNew() => null;
    }
}
