using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class MageController : EnemyController
    {
        private readonly WorldSprite _player;
        private readonly CollisionDetector _collisionDetector;
        private readonly ICollidableSpriteControllerPool _bulletControllers;

        public MageController(ICollidableSpriteControllerPool bulletControllers, SpriteTileIndex index, ChompGameModule gameModule, SystemMemoryBuilder memoryBuilder, WorldSprite player) 
            : base(SpriteType.Mage, index, gameModule, memoryBuilder)
        {
            _player = player;
            _collisionDetector = gameModule.CollissionDetector;
            _bulletControllers = bulletControllers;
            Palette = SpritePalette.Enemy1;
        }
        protected override void OnSpriteCreated(Sprite sprite)
        {

        }

        protected override void UpdateActive()
        {
           
        }
    }
}
