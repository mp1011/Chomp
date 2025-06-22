using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SceneModels.SceneParts;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteControllers.Platforms;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;
using System.Linq;

namespace ChompGame.MainGame.SpriteControllers
{
    class SceneSpriteControllers
    {
        private ChompGameModule _gameModule;
        private PlayerController _playerController;
        private SpriteControllerPool<BombController> _bombControllers;
        private SpriteControllerPool<PrizeController> _prizeControllers;
        private SpriteControllerPool<DoorController> _doorControllers;
        private SpriteControllerPool<ButtonController> _buttonControllers;
        private SpriteControllerPool<PlatformController> _platformControllers;
        private SpriteControllerPool<ExplosionController> _explosionControllers;
        private EnemyOrBulletSpriteControllerPool<TurretBulletController> _turretBulletControllers;
        private ICollidableSpriteControllerPool[] _spriteControllers;

        private SceneDefinition _scene;

        private Specs Specs => _gameModule.Specs;

        public PlayerController Player => _playerController;

        public SpriteControllerPool<BombController> BombControllers => _bombControllers;

        public SpriteControllerPool<PrizeController> PrizeControllers => _prizeControllers;

        public SpriteControllerPool<ExplosionController> ExplosionControllers => _explosionControllers;

        public SceneSpriteControllers(ChompGameModule chompGameModule, 
            PlayerController playerController, 
            SpriteControllerPool<BombController> bombControllers,
            SpriteControllerPool<PrizeController> prizeControllers,
            SpriteControllerPool<DoorController> doorControllers,
            SpriteControllerPool<ButtonController> buttonControllers,
            SpriteControllerPool<PlatformController> platformControllers,
            SpriteControllerPool<ExplosionController> explosionControllers,
            EnemyOrBulletSpriteControllerPool<TurretBulletController> turretBulletControllers,
            params ICollidableSpriteControllerPool[] spriteControllerPools)
        {
            _gameModule = chompGameModule;
            _playerController = playerController;
            _platformControllers = platformControllers;
            _explosionControllers = explosionControllers;
            _bombControllers = bombControllers;
            _prizeControllers = prizeControllers;
            _doorControllers = doorControllers;
            _buttonControllers = buttonControllers;
            _turretBulletControllers = turretBulletControllers;
            _spriteControllers = spriteControllerPools
                .Where(p => p != null)
                .ToArray();
        }

        public void Initialize(SceneDefinition scene, 
            NBitPlane levelMap, 
            NBitPlane levelAttributeTable, 
            ExitType lastExitType,
            GameBit isCarryingBomb)
        {
            _scene = scene;
            if (scene.HasSprite(SpriteType.Player))
            {
                _gameModule.WorldScroller.Initialize(scene, _playerController.WorldSprite, levelMap, levelAttributeTable);
            
                var playerSpriteDefinition = new SpriteDefinition(SpriteType.Player, _gameModule.GameSystem.Memory);
                _playerController.WorldSprite.X = 16;
                _playerController.WorldSprite.Y = 16;
              
                _playerController.InitializeSprite();
                _playerController.Motion.XSpeed = 0;
                _playerController.Motion.YSpeed = 0;
                _playerController.SetInitialPosition(levelMap, lastExitType, this);

                if(isCarryingBomb.Value)
                {
                    var bomb = _bombControllers.TryAddNew();
                    if (bomb != null)
                    {
                        bomb.DestructionBitOffset = 255;
                        bomb.SetCarried();
                        bomb.FallCheck = _scene.SpriteFallCheck;
                        _playerController.IsHoldingBomb = true;
                        GameDebug.DebugLog($"Create carried bomb at Sprite {bomb.SpriteIndex}", DebugLogFlags.SpriteSpawn);
                        isCarryingBomb.Value = false;
                    }
                }
            }

            _gameModule.WorldScroller.Update();
            CheckSpriteSpawn();
        }

        public void Update()
        {
            if(_scene.IsAutoScroll)
            {
                _playerController.Update();
                _bombControllers.Execute(c => c.Update());
                _prizeControllers.Execute(c => c.Update());

                if(_gameModule.LevelTimer.Value.IsMod(8))
                    CheckSpriteSpawn();
            }
            else if (_scene.HasSprite(SpriteType.Player))
            {
                _playerController.Update();
                _bombControllers.Execute(c => c.Update());
                _doorControllers.Execute(c => c.Update());
                _buttonControllers.Execute(c => c.Update());
                _platformControllers.Execute(c => c.Update());
                _prizeControllers.Execute(c => c.Update());
                _turretBulletControllers.Execute(c => c.Update());
            }

            _explosionControllers.Execute(c => c.Update());

            foreach (var spriteController in _spriteControllers)
                spriteController.Execute(c => c.Update());
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
            if (_scene.HasSprite(SpriteType.Player))
            {
                _playerController.WorldSprite.UpdateSprite();
                _bombControllers.Execute(c => c.WorldSprite.UpdateSprite());
                _prizeControllers.Execute(c => c.WorldSprite.UpdateSprite());

                if (!_scene.IsAutoScroll)
                {
                    _doorControllers.Execute(c => c.WorldSprite.UpdateSprite());
                    _buttonControllers.Execute(c => c.WorldSprite.UpdateSprite());
                    _platformControllers.Execute(c => c.WorldSprite.UpdateSprite());
                    _turretBulletControllers.Execute(c => c.WorldSprite.UpdateSprite());
                }
            }

            _explosionControllers.Execute(c => c.WorldSprite.UpdateSprite());

            foreach (var spriteController in _spriteControllers)
                spriteController.Execute(c => c.WorldSprite.UpdateSprite());

            CheckSpriteSpawn();
        }

        public Point GetDoorPosition(ExitType doorType)
        {
            DynamicScenePartHeader header = _gameModule.CurrentScenePartHeader;

            for (int i = 0; i < header.PartsCount; i++)
            {               
                IScenePart sp = header.GetSpriteScenePart(i, _scene, _gameModule.Specs);

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
            if(_scene.IsAutoScroll)
            {
                CheckSpriteSpawn_AutoScroll();
                return;
            }

            GameDebug.DebugLog("Checking sprite spawn", DebugLogFlags.SpriteSpawn);
            byte nextDestructionBitOffset = 0;

            DynamicScenePartHeader header = _gameModule.CurrentScenePartHeader;

            for (int i = 0; i < header.PartsCount; i++)
            {
                BaseScenePart sp = header.GetSpriteOrPlatformScenePart(i, _scene, _gameModule.Specs);

                byte destructionBitOffset = nextDestructionBitOffset;
                nextDestructionBitOffset += sp.DestroyBitsRequired;

                if (sp.DestroyBitsRequired > 0 && _gameModule.ScenePartsDestroyed.IsDestroyed(destructionBitOffset)
                    && !AllowGraceBomb(sp))
                {
                    GameDebug.DebugLog($"Skipping part {i} ({sp.Type}) because it has been destroyed", DebugLogFlags.SpriteSpawn);
                    continue;
                }

                if (header.IsPartActivated(i))
                {
                    GameDebug.DebugLog($"Skipping part {i} ({sp.Type}) because it has already been activated", DebugLogFlags.SpriteSpawn);
                    continue;
                }
            
                switch(sp.Type)
                {
                    case ScenePartType.Coin:
                    case ScenePartType.DestructibleBlock:
                    case ScenePartType.SwitchBlock:
                    case ScenePartType.SideExit:
                        continue;
                }

                var pool = GetPool(sp.Type);
                if (!pool.CanAddNew())
                    continue;

                int spawnX = sp.X * Specs.TileWidth;
                int spawnY = sp.Y * Specs.TileHeight;
                if (spawnY == 0)
                    spawnY = GetGroundPosition(sp.X);

                var spawnBounds = new Rectangle(spawnX, spawnY, Specs.TileWidth * 2, Specs.TileHeight * 2);

                if(_gameModule.WorldScroller.DistanceFromViewpane(spawnBounds) < 8)
                {
                    var sprite = pool.TryAddNew();
                    if (sprite == null)
                    {
                        GameDebug.DebugLog($"Unable to spawn {sp.Type}", DebugLogFlags.SpriteSpawn);
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
                        var platformType = sp.Type switch 
                        {
                            ScenePartType.Platform_LeftRight => PlatformType.LeftRight,
                            ScenePartType.Platform_Vanishing => PlatformType.Vanishing,
                            ScenePartType.Platform_UpDown => PlatformType.UpDown,
                            _ =>  PlatformType.Falling
                        };

                        pc.Initialize(platformType, spawnX, spawnY, (sp as PlatformScenePart).Length);
                    }
                    else if(sprite is TurretBulletController tc)
                    {
                        tc.ScenePartIndex = (byte)i;
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


                        GameDebug.DebugLog($"Created sprite: Controller={sprite.GetType().Name} X={spawnX} Y={spawnY}", DebugLogFlags.SpriteSpawn);
                    }
                }      
                else
                {
                    GameDebug.DebugLog($"Skipping part {i} ({sp.Type}) because it is not in bounds", DebugLogFlags.SpriteSpawn);
                }
            }
        }

        public void ReplaceEnemyController2(ICollidableSpriteControllerPool enemyControllers, ICollidableSpriteControllerPool extraControllers)
        {
            _spriteControllers[1] = enemyControllers;
            _spriteControllers[3] = extraControllers;
        }

        private bool AllowGraceBomb(BaseScenePart sp)
        {
            switch(_gameModule.CurrentLevel)
            {
                case Level.Level1_4_DeadEnd:
                case Level.Level3_19_Building3_Room9:
                case Level.Level6_3_Techbase3:
                    return sp.Type == ScenePartType.Bomb && !_playerController.IsHoldingBomb;
                default:
                    return false;
            }
        }

        private int GetGroundPosition(byte tileX)
        {
            var levelNt = _gameModule.WorldScroller.LevelNameTable;
            byte tileY = 0;
            while (!CollisionDetector.IsTileSolid(levelNt[tileX, tileY++]));

            return tileY * _gameModule.Specs.TileHeight;
        }

        private void CheckSpriteSpawn_AutoScroll()
        {
            DynamicScenePartHeader header = _gameModule.CurrentScenePartHeader;
            byte time = 0;

            for (int i = 0; i < header.PartsCount; i++)
            {
                AutoscrollScenePart sp = header.GeAutoScrollScenePart(i, _scene, _gameModule.Specs);
                time += sp.Delay;

                if (header.IsPartActivated(i))
                {
                    if (i == header.PartsCount - 1)
                        CheckAutoscrollEnd(time);
                    continue;
                }

                if (_gameModule.LevelTimerLong.Value == time)
                {
                    SpawnAutoscrollSprite(sp);
                    header.MarkActive(i);
                }
            }
        }

        private void CheckAutoscrollEnd(byte lastSpawnTime)
        {
            if (_scene.IsMidBossScene || _gameModule.LevelTimerLong.Value < lastSpawnTime + 32)
                return;

            foreach(var pool in _spriteControllers)
            {
                pool.Execute(c =>
                {
                    if (c.Status == WorldSpriteStatus.Active)
                        return;
                });          
            }

            _gameModule.ExitsModule.GotoNextLevel();
        }

        private void SpawnAutoscrollSprite(AutoscrollScenePart sp)
        {
            var pool = GetPool(sp.Type);

            var sprite = pool.TryAddNew();
            if (sprite == null)
            {
                GameDebug.DebugLog($"No free sprites available for {sp.Type}, attempting to claim a slot", DebugLogFlags.SpriteSpawn);
                sprite = TryClaimFreeSlot(pool);
            }

            if (sprite == null)
            {
                GameDebug.DebugLog($"Unable to spawn sprite of type {sp.Type}", DebugLogFlags.SpriteSpawn);
                return;
            }

            sprite.WorldSprite.X = Specs.ScreenWidth;
            sprite.WorldSprite.Y = sp.Y * Specs.TileHeight;
            sprite.WorldSprite.UpdateSprite();

            if (sprite is IAutoScrollSpriteController a)
            {
                a.Variation = sp.Variation;
                a.AfterSpawn(pool);
            }
        }

        private ISpriteController TryClaimFreeSlot(ISpriteControllerPool pool)
        {
            var screenBounds = new Rectangle(0, 0, _gameModule.Specs.ScreenWidth, _gameModule.Specs.ScreenHeight);
            bool foundMatch = false;

            pool.Execute(sc =>
            {
                if (foundMatch)
                    return;

                if (!sc.WorldSprite.Bounds.Intersects(screenBounds))
                {
                    sc.WorldSprite.Destroy();
                    foundMatch = true;
                }

            });

            return pool.TryAddNew();
        }

        private ISpriteControllerPool GetPool(ScenePartType scenePartType) =>

            scenePartType switch {
                ScenePartType.Bomb => _bombControllers,
                ScenePartType.Turret => _turretBulletControllers,
                ScenePartType.EnemyType1 => _spriteControllers[0],
                ScenePartType.EnemyType2 => _spriteControllers[1],
                ScenePartType.DoorBackExit => _doorControllers,
                ScenePartType.DoorFowardExit => _doorControllers,
                ScenePartType.Platform_Falling => _platformControllers,
                ScenePartType.Platform_LeftRight => _platformControllers,
                ScenePartType.Platform_UpDown => _platformControllers,
                ScenePartType.Platform_Vanishing=> _platformControllers,
                ScenePartType.Button => _buttonControllers,
                ScenePartType.Coin => _prizeControllers,
                _ => _bombControllers
            };
        
        public void CheckCollisions()
        {
            foreach(var c in _spriteControllers)
                _playerController.CheckEnemyOrBulletCollisions(c);

            _playerController.CheckEnemyOrBulletCollisions(_turretBulletControllers);
            _playerController.CheckBombPickup(_bombControllers);

            //should do this better
            if (!_scene.IsAutoScroll)
            {
                _platformControllers.Execute(b => b.CheckPlayerCollision(_playerController));
                _doorControllers.Execute(b =>
                {
                    b.CheckPlayerOpen();
                });

            }

            _prizeControllers.Execute(b => b.CheckPlayerCollision(_playerController));
            _bombControllers.Execute(b =>
            {
                foreach (var c in _spriteControllers)
                {
                    b.CheckEnemyCollisions(c);
                }
            });
        }
    }
}
