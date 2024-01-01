using ChompGame.GameSystem;
using ChompGame.MainGame;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers;
using ChompGame.MainGame.SpriteControllers.Base;
using System;
using System.Linq;

namespace ChompGame.Data
{
    interface ISpriteControllerPool
    {
        void Execute(Action<ISpriteController> action, bool skipIfInactive = true);
        ISpriteController TryAddNew();
        bool CanAddNew();
    }

    interface ICollidableSpriteControllerPool : ISpriteControllerPool
    {
        void Execute(Action<ICollidableSpriteController> action, bool skipIfInactive = true);  
    }

    class SpriteControllerPool<T> : ISpriteControllerPool
        where T:class, ISpriteController
    {
        private readonly SpritesModule _spritesModule;
        protected readonly T[] _items;
         
        public SpriteControllerPool(
            int size, 
            SpritesModule spritesModule,
            Func<T> generateController)
        {
            _spritesModule = spritesModule;
            _items = Enumerable.Range(0, size)
                .Select(p => generateController())
                .ToArray();
        }

        public bool CanAddNew()
        {
            byte freeSpriteIndex = _spritesModule.GetFreeSpriteIndex();
            if (freeSpriteIndex == 255)
                return false;

            for (byte i = 0; i < _items.Length; i++)
            {
                if (_items[i].Status != WorldSpriteStatus.Inactive)
                    continue;

                return true;
            }

            return false;
        }

        ISpriteController ISpriteControllerPool.TryAddNew()
            => TryAddNew();
        public T TryAddNew()
        {
            byte freeSpriteIndex = _spritesModule.GetFreeSpriteIndex();
            if (freeSpriteIndex == 255)
                return null;

            for (byte i = 0; i < _items.Length; i++)
            {
                if (_items[i].Status != WorldSpriteStatus.Inactive)
                    continue;

                _items[i].SpriteIndex = freeSpriteIndex;
                _items[i].Status = WorldSpriteStatus.Active;
                _items[i].InitializeSprite();

                return _items[i];
            }

            return null;
        }

        public void Execute(Action<T> action, bool skipIfInactive=true)
        {
            foreach(var item in _items)
            {
                if (!skipIfInactive || item.Status != WorldSpriteStatus.Inactive)
                {
                    action(item);
                }
            }
        }
        public void Execute(Action<ISpriteController> action, bool skipIfInactive = true)
        {
            foreach (var item in _items)
            {
                if (!skipIfInactive || item.Status != WorldSpriteStatus.Inactive)
                {
                    action(item);
                }
            }
        }

    }

    class EnemyOrBulletSpriteControllerPool<T> : SpriteControllerPool<T>, ICollidableSpriteControllerPool
        where T : class, ICollidableSpriteController
    {
        public EnemyOrBulletSpriteControllerPool(int size, SpritesModule spritesModule, Func<T> generateController) 
            : base(size, spritesModule, generateController)
        {
        }

        public bool CollidesWithPlayer(PlayerController player, WorldSprite sprite) => player.CollidesWith(sprite);

        public void Execute(Action<ICollidableSpriteController> action, bool skipIfInactive = true)
        {
            foreach (var item in _items)
            {
                if (!skipIfInactive || item.Status != WorldSpriteStatus.Inactive)
                {
                    action(item);
                }
            }
        }
    }

}
