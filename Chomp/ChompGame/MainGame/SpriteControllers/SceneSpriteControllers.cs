using ChompGame.Data;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class SceneSpriteControllers
    {
        private ChompGameModule _gameModule;
        private PlayerController _playerController;
        private SpriteControllerPool<BombController> _bombControllers;

        private IEnemyOrBulletSpriteControllerPool _enemyAControllers;
        private IEnemyOrBulletSpriteControllerPool _enemyBControllers;
        private IEnemyOrBulletSpriteControllerPool _extra1Controllers;
        private IEnemyOrBulletSpriteControllerPool _extra2Controllers;

        private SceneDefinition _scene;

        public SceneSpriteControllers(ChompGameModule chompGameModule, 
            PlayerController playerController, 
            SpriteControllerPool<BombController> bombControllers, 
            IEnemyOrBulletSpriteControllerPool enemyAControllers, 
            IEnemyOrBulletSpriteControllerPool enemyBControllers, 
            IEnemyOrBulletSpriteControllerPool extra1Controllers, 
            IEnemyOrBulletSpriteControllerPool extra2Controllers)
        {
            _gameModule = chompGameModule;
            _playerController = playerController;
            _bombControllers = bombControllers;
            _enemyAControllers = enemyAControllers;
            _enemyBControllers = enemyBControllers;
            _extra1Controllers = extra1Controllers;
            _extra2Controllers = extra2Controllers;
        }

        public void Initialize(SceneDefinition scene, NBitPlane levelMap, NBitPlane levelAttributeTable)
        {
            _scene = scene;
            if (scene.HasSprite(SpriteLoadFlags.Player))
            {
                _gameModule.WorldScroller.Initialize(scene, _playerController.WorldSprite, levelMap, levelAttributeTable);
            
                var playerSpriteDefinition = new SpriteDefinition(SpriteType.Player, _gameModule.GameSystem.Memory);
                _playerController.WorldSprite.X = 16;
                _playerController.WorldSprite.Y = 16;
                _playerController.Palette = 1;
                _playerController.InitializeSprite();
                _playerController.Motion.XSpeed = 0;
                _playerController.Motion.YSpeed = 0;
            }

            GameDebug.Watch1 = new DebugWatch(
                "Player X",
                () => _playerController.WorldSprite.X);

            GameDebug.Watch2 = new DebugWatch(
                name: "Player Sprite X",
                () => _playerController.GetSprite().X);

            GameDebug.Watch3 = new DebugWatch(
               name: "Player Sprite Y",
               () => _playerController.GetSprite().Y);
        }

        public void Update()
        {
            if (_scene.HasSprite(SpriteLoadFlags.Player))
            {
                _playerController.Update();
                _bombControllers.Execute(c => c.Update());
            }

            _enemyAControllers.Execute(c => c.Update());
            _enemyBControllers.Execute(c => c.Update());
            _extra1Controllers.Execute(c => c.Update());
            //todo, when to use extra2
        }

        public void OnWorldScrollerUpdate()
        {
            if (_scene.HasSprite(SpriteLoadFlags.Player))
            {
                _playerController.WorldSprite.UpdateSprite();
                _bombControllers.Execute(c => c.WorldSprite.UpdateSprite());
            }

            _enemyAControllers.Execute(c => c.WorldSprite.UpdateSprite());
            _enemyBControllers.Execute(c => c.WorldSprite.UpdateSprite());
            _extra1Controllers.Execute(c => c.WorldSprite.UpdateSprite());
            //todo, when to use extra2
        }

        public void CheckCollissions()
        {
            _playerController.CheckEnemyOrBulletCollisions(_enemyAControllers);
            _playerController.CheckEnemyOrBulletCollisions(_enemyBControllers);
            _playerController.CheckEnemyOrBulletCollisions(_extra1Controllers);
            _playerController.CheckBombPickup(_bombControllers);

            _bombControllers.Execute(b =>
            {
                b.CheckEnemyCollisions(_enemyAControllers);
                b.CheckEnemyCollisions(_enemyBControllers);
            });
        }
    }
}
