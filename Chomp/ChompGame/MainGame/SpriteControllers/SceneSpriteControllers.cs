using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SpriteControllers
{
    class SceneSpriteControllers
    {
        private GameByteEnum<Level> _currentLevel;
        private ChompGameModule _gameModule;
        private PlayerController _playerController;
        private SpriteControllerPool<BombController> _bombControllers;
        private SpriteControllerPool<DoorController> _doorControllers;
        private SpriteControllerPool<PlatformController> _platformControllers;

        private IEnemyOrBulletSpriteControllerPool _enemyAControllers;
        private IEnemyOrBulletSpriteControllerPool _enemyBControllers;
        private IEnemyOrBulletSpriteControllerPool _extra1Controllers;
        private IEnemyOrBulletSpriteControllerPool _extra2Controllers;

        private SceneDefinition _scene;

        private Specs Specs => _gameModule.Specs;

        public PlayerController Player => _playerController;

        public SceneSpriteControllers(ChompGameModule chompGameModule, 
            PlayerController playerController, 
            SpriteControllerPool<BombController> bombControllers, 
            SpriteControllerPool<DoorController> doorControllers,
            SpriteControllerPool<PlatformController> platformControllers,
            IEnemyOrBulletSpriteControllerPool enemyAControllers, 
            IEnemyOrBulletSpriteControllerPool enemyBControllers, 
            IEnemyOrBulletSpriteControllerPool extra1Controllers, 
            IEnemyOrBulletSpriteControllerPool extra2Controllers)
        {
            _gameModule = chompGameModule;
            _playerController = playerController;
            _platformControllers = platformControllers;
            _bombControllers = bombControllers;
            _doorControllers = doorControllers;
            _enemyAControllers = enemyAControllers;
            _enemyBControllers = enemyBControllers;
            _extra1Controllers = extra1Controllers;
            _extra2Controllers = extra2Controllers;
        }

        public void Initialize(SceneDefinition scene, NBitPlane levelMap, NBitPlane levelAttributeTable, ExitType lastExitType)
        {
            _scene = scene;
            if (scene.HasSprite(SpriteLoadFlags.Player))
            {
                _gameModule.WorldScroller.Initialize(scene, _playerController.WorldSprite, levelMap, levelAttributeTable);
            
                var playerSpriteDefinition = new SpriteDefinition(SpriteType.Player, _gameModule.GameSystem.Memory);
                _playerController.WorldSprite.X = 16;
                _playerController.WorldSprite.Y = 16;
                _playerController.Palette = 1;
                _playerController.InitializeSprite(1);
                _playerController.Motion.XSpeed = 0;
                _playerController.Motion.YSpeed = 0;
                _playerController.SetInitialPosition(levelMap, lastExitType, this);
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

            CheckSpriteSpawn();
        }

        public void Update()
        {
            if (_scene.HasSprite(SpriteLoadFlags.Player))
            {
                _playerController.Update();
                _bombControllers.Execute(c => c.Update());
                _doorControllers.Execute(c => c.Update());
                _platformControllers.Execute(c => c.Update());
            }

            _enemyAControllers?.Execute(c => c.Update());
            _enemyBControllers?.Execute(c => c.Update());
            _extra1Controllers?.Execute(c => c.Update());
            _extra2Controllers?.Execute(c => c.Update());
        }

        public void OnWorldScrollerUpdate()
        {
            if (_scene.HasSprite(SpriteLoadFlags.Player))
            {
                _playerController.WorldSprite.UpdateSprite();
                _bombControllers.Execute(c => c.WorldSprite.UpdateSprite());
                _doorControllers.Execute(c => c.WorldSprite.UpdateSprite());
                _platformControllers.Execute(c => c.WorldSprite.UpdateSprite());
            }

            _enemyAControllers.Execute(c => c.WorldSprite.UpdateSprite());
            _enemyBControllers.Execute(c => c.WorldSprite.UpdateSprite());
            _extra1Controllers.Execute(c => c.WorldSprite.UpdateSprite());
            //todo, when to use extra2

            CheckSpriteSpawn();
        }

        public Point GetDoorPosition(ExitType doorType)
        {
            DynamicScenePartHeader header = _gameModule.CurrentScenePartHeader;

            for (int i = 0; i < header.PartsCount; i++)
            {               
                ScenePart sp = header.GetScenePart(i, _scene);

                if(!(sp.Type == ScenePartType.DoorBackExit && doorType == ExitType.DoorBack)
                    && !(sp.Type == ScenePartType.DoorFowardExit && doorType == ExitType.DoorForward))
                {
                    continue;
                }

                return new Point(sp.X * Specs.TileWidth, sp.Y * Specs.TileHeight);
            }

            return new Point(0, 0);
        }

        public void CheckSpriteSpawn()
        {
            DynamicScenePartHeader header = _gameModule.CurrentScenePartHeader;

            for(int i = 0; i < header.PartsCount; i++)
            {
                if (header.IsPartActivated(i))
                    continue;

                ScenePart sp = header.GetScenePart(i, _scene);

                if (sp.Type == ScenePartType.SideExit)
                    continue;

                var pool = GetPool(sp.Type);
                if (!pool.CanAddNew())
                    continue;

                int spawnX = sp.X * Specs.TileWidth;
                int spawnY = sp.Y * Specs.TileHeight;

                if (spawnX >= _gameModule.WorldScroller.WorldScrollPixelX
                    && spawnY >= _gameModule.WorldScroller.WorldScrollPixelY
                    && spawnX <= _gameModule.WorldScroller.WorldScrollPixelX + Specs.NameTablePixelWidth
                    && spawnY <= _gameModule.WorldScroller.WorldScrollPixelY + Specs.NameTablePixelHeight)
                {
                    var sprite = pool.TryAddNew(_scene.GetPalette(sp.Type));
                    
                    if(sprite is DoorController dc)
                    {
                        if (sp.Type == ScenePartType.DoorBackExit)
                            dc.DoorType = ExitType.DoorBack;
                        else
                            dc.DoorType = ExitType.DoorForward;
                    }

                    if (sprite != null)
                    {
                        header.MarkActive(i);
                        sprite.WorldSprite.X = spawnX;
                        sprite.WorldSprite.Y = spawnY;
                    }
                }                
            }
        }

        private ISpriteControllerPool GetPool(ScenePartType scenePartType) =>

            scenePartType switch {
                ScenePartType.Bomb => _bombControllers,
                ScenePartType.EnemyType1 => _enemyAControllers,
                ScenePartType.EnemyType2 => _enemyBControllers,
                ScenePartType.DoorBackExit => _doorControllers,
                ScenePartType.DoorFowardExit => _doorControllers,
                ScenePartType.Platform => _platformControllers,
                _ => throw new System.NotImplementedException()
            };
        
        public void CheckCollissions()
        {
            _playerController.CheckEnemyOrBulletCollisions(_enemyAControllers);
            _playerController.CheckEnemyOrBulletCollisions(_enemyBControllers);
            _playerController.CheckEnemyOrBulletCollisions(_extra1Controllers);
            _playerController.CheckBombPickup(_bombControllers);

            _platformControllers.Execute(b => b.CheckPlayerCollision(_playerController));

            _doorControllers.Execute(b =>
            {
                b.CheckPlayerOpen();
            });

            _bombControllers.Execute(b =>
            {
                b.CheckEnemyCollisions(_enemyAControllers);
                b.CheckEnemyCollisions(_enemyBControllers);
            });
        }
    }
}
