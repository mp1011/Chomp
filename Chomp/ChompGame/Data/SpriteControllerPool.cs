using ChompGame.GameSystem;
using System;
using System.Linq;

namespace ChompGame.Data
{
    class SpriteControllerPool<T>
        where T:class, ISpriteController
    {
        private readonly SpritesModule _spritesModule;
        private readonly T[] _items;
        private readonly Action<Sprite> _configureSprite;
         
        public SpriteControllerPool(
            int size, 
            SpritesModule spritesModule,
            Func<T> generateController,
            Action<Sprite> configureSprite)
        {
            _spritesModule = spritesModule;
            _configureSprite = configureSprite;
            _items = Enumerable.Range(0, size)
                .Select(p => generateController())
                .ToArray();
        }

        public T TryAddNew()
        {
            byte freeSpriteIndex = _spritesModule.GetFreeSpriteIndex();
            if (freeSpriteIndex == 255)
                return null;

            for (byte i = 0; i < _items.Length; i++)
            {
                if (_items[i].SpriteIndex != 255)
                    continue;

                _items[i].SpriteIndex = freeSpriteIndex;
                _configureSprite(_spritesModule.GetSprite(freeSpriteIndex));
                return _items[i];
            }

            return null;
        }

        public void Execute(Action<T> action)
        {
            foreach(var item in _items)
            {
                if (item.SpriteIndex != 255)
                {
                    action(item);
                }
            }
        }


    }
}
