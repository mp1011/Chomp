using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
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
        private SpriteControllerPool<ExplosionController> _explosionControllers;

        private IEnemyOrBulletSpriteControllerPool _enemyAControllers;
        private IEnemyOrBulletSpriteControllerPool _enemyBControllers;
        private IEnemyOrBulletSpriteControllerPool _extra1Controllers;
        private IEnemyOrBulletSpriteControllerPool _extra2Controllers;

        private SceneDefinition _scene;

        private Specs Specs => _gameModule.Specs;

        public PlayerController Player => _playerController;

        public SpriteControllerPool<BombController> BombControllers => _bombControllers;

        public SpriteControllerPool<ExplosionController> ExplosionControllers => _explosionControllers;

        public SceneSpriteControllers(ChompGameModule chompGameModule, 
            PlayerController playerController, 
            SpriteControllerPool<BombController> bombControllers, 
            SpriteControllerPool<DoorController> doorControllers,
            SpriteControllerPool<PlatformController> platformControllers,
            SpriteControllerPool<ExplosionController> explosionControllers,
            IEnemyOrBulletSpriteControllerPool enemyAControllers, 
            IEnemyOrBulletSpriteControllerPool enemyBControllers, 
            IEnemyOrBulletSpriteControllerPool extra1Controllers, 
            IEnemyOrBulletSpriteControllerPool extra2Controllers)
        {
            _gameModule = chompGameModule;
            _playerController = playerController;
            _platformControllers = platformControllers;
            _explosionControllers = explosionControllers;
            _bombControllers = bombControllers;
            _doorControllers = doorControllers;
            _enemyAControllers = enemyAControllers;
            _enemyBControllers = enemyBControllers;
            _extra1Controllers = extra1Controllers;
            _extra2Controllers = extra2Controllers;
        }

        public void Initialize(SceneDefinition scene, 
            NBitPlane levelMap, 
            NBitPlane levelAttributeTable, 
            ExitType lastExitType,
            GameBit isCarryingBomb)
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

                if(isCarryingBomb.Value)
                {
                    var bomb = _bombControllers.TryAddNew(0);
                    if (bomb != null)
                    {
                        bomb.DestructionBitOffset = 255;
                        bomb.SetCarried();

                        GameDebug.DebugLog($"Create carried bomb at Sprite {bomb.SpriteIndex}");
                        isCarryingBomb.Value = false;
                    }
                }
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

            _gameModule.WorldScroller.Update();
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

            _explosionControllers.Execute(c => c.Update());
            _enemyAControllers?.Execute(c => c.Update());
            _enemyBControllers?.Execute(c => c.Update());
            _extra1Controllers?.Execute(c => c.Update());
            _extra2Controllers?.Execute(c => c.Update());
        }

        public void CheckBombCarry(GameBit isCarryingBomb)
        {
            _bombControllers.Execute(c =>
            {
                if (c.IsCarried)
                    isCarryingBomb.Value = true;
            });
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

            _explosionControllers.Execute(c => c.WorldSprite.UpdateSprite());
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
            GameDebug.DebugLog("Checking sprite spawn");
            byte nextDestructionBitOffset = 0;

            DynamicScenePartHeader header = _gameModule.CurrentScenePartHeader;

            for (int i = 0; i < header.PartsCount; i++)
            {
                ScenePart sp = header.GetScenePart(i, _scene);

                byte destructionBitOffset = nextDestructionBitOffset;
                nextDestructionBitOffset += sp.DestroyBitsRequired;

                if (sp.DestroyBitsRequired > 0 && _gameModule.ScenePartsDestroyed.IsDestroyed(destructionBitOffset))
                {
                    GameDebug.DebugLog($"Skipping part {i} ({sp.Type}) because it has been destroyed");
                    continue;
                }

                if (header.IsPartActivated(i))
                {
                    GameDebug.DebugLog($"Skipping part {i} ({sp.Type}) because it has already been activated");
                    continue;
                }
            
                switch(sp.Type)
                {
                    case ScenePartType.Coin:
                    case ScenePartType.DestructibleBlock:
                    case ScenePartType.LockedDoor:
                    case ScenePartType.Button:
                    case ScenePartType.SideExit:
                        continue;
                }

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
                    if (sprite == null)
                    {
                        GameDebug.DebugLog($"Unable to spawn {sp.Type}");
                        continue;
                    }

                    sprite.FallCheck = _scene.SpriteFallCheck;

                    if (sprite is DoorController dc)
                    {
                        if (sp.Type == ScenePartType.DoorBackExit)
                            dc.DoorType = ExitType.DoorBack;
                        else
                            dc.DoorType = ExitType.DoorForward;
                    }
                    else if(sprite is PlatformController pc)
                    {
                        pc.PlatformType = sp.Type switch 
                        {
                            ScenePartType.Platform_LeftRight => PlatformType.LeftRight,
                            ScenePartType.Platform_Vanishing => PlatformType.Vanishing,
                            ScenePartType.Platform_UpDown => PlatformType.UpDown,
                            _ =>  PlatformType.Falling
                        };
                    }

                    if(sp.DestroyBitsRequired > 0)                    
                        sprite.DestructionBitOffset = destructionBitOffset;                    
                    else
                        sprite.DestructionBitOffset = 255;
                    
                    if (sprite != null)
                    {
                        header.MarkActive(i);
                        sprite.WorldSprite.X = spawnX;
                        sprite.WorldSprite.Y = spawnY;
                        sprite.WorldSprite.UpdateSprite();


                        GameDebug.DebugLog($"Created sprite: Controller={sprite.GetType().Name} X={spawnX} Y={spawnY}");
                    }
                }      
                else
                {
                    GameDebug.DebugLog($"Skipping part {i} ({sp.Type}) because it is not in bounds");
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
                ScenePartType.Platform_Falling => _platformControllers,
                ScenePartType.Platform_LeftRight => _platformControllers,
                ScenePartType.Platform_UpDown => _platformControllers,
                ScenePartType.Platform_Vanishing=> _platformControllers,
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
