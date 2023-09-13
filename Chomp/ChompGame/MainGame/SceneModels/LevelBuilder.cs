using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.MainGame.SceneModels.SceneParts;
using ChompGame.MainGame.SpriteControllers;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame.SceneModels
{
    class LevelBuilder
    {
        private SceneDefinition _sceneDefinition;
        private ChompGameModule _gameModule;
        private SpriteTileTable _spriteTileTable;

        public LevelBuilder(ChompGameModule chompGameModule, SceneDefinition sceneDefinition)
        {
            _gameModule = chompGameModule;
            _spriteTileTable = chompGameModule.SpriteTileTable;
            _sceneDefinition = sceneDefinition;
        }

        public void BuildBackgroundNametable(NBitPlane nameTable)
        {
            _gameModule.TileModule.NameTable.Reset();
            _gameModule.StatusBar.InitializeTiles();

            _sceneDefinition.ThemeSetup.BuildBackgroundNameTable(nameTable);

            var vramNametable = _gameModule.TileModule.NameTable;

            nameTable.CopyTo(
                destination: vramNametable,
                source: new InMemoryByteRectangle(0, 0, vramNametable.Width, vramNametable.Height - 2),
                destinationPoint: new Point(0, 2),
                specs: _gameModule.Specs,
                memory: _gameModule.GameSystem.Memory);
        }
       
        public NBitPlane BuildAttributeTable(
            SystemMemoryBuilder memoryBuilder, 
            NBitPlane nameTable)
        {
            NBitPlane attributeTable = NBitPlane.Create(
                memoryBuilder.CurrentAddress, 
                memoryBuilder.Memory,
                _gameModule.Specs.AttributeTableBitsPerBlock,
               _sceneDefinition.LevelTileWidth / _gameModule.Specs.AttributeTableBlockSize,
               _sceneDefinition.LevelTileHeight / _gameModule.Specs.AttributeTableBlockSize);

            memoryBuilder.AddBytes(attributeTable.Bytes);

            return _sceneDefinition.ThemeSetup.BuildAttributeTable(attributeTable, nameTable);
        }

        public NBitPlane BuildNameTable(SystemMemoryBuilder memoryBuilder, int seed)
        {
            NBitPlane nameTable = NBitPlane.Create(memoryBuilder.CurrentAddress, 
                memoryBuilder.Memory,
                _gameModule.Specs.NameTableBitPlanes,
                _sceneDefinition.LevelTileWidth,
                _sceneDefinition.LevelTileHeight);

            memoryBuilder.AddBytes(nameTable.Bytes);

            nameTable = AddEdgeTiles(nameTable);
            nameTable = AddShapeTiles(nameTable, seed);
            nameTable = AddExitTiles(nameTable);

            return nameTable;
        }


        private NBitPlane AddExitTiles(NBitPlane nameTable)
        {
            ScenePartsHeader header = new ScenePartsHeader(_gameModule.CurrentLevel, _gameModule.GameSystem.Memory);

            for (int i = 0; i < header.PartsCount; i++)
            {
                var sp = new ExitScenePart(_gameModule.GameSystem.Memory, header.FirstPartAddress + (BaseScenePart.Bytes * i), _sceneDefinition, _gameModule.Specs);

                if (sp.Type != ScenePartType.SideExit)
                    continue;

                if (sp.ExitType == ExitType.Right)
                    nameTable = AddRightExit(nameTable);

                else if (sp.ExitType == ExitType.Left)
                    nameTable = AddLeftExit(nameTable);

                else if (sp.ExitType == ExitType.Bottom)
                    nameTable = AddBottomExit(nameTable);
            }

            return nameTable;
        }

        private NBitPlane AddRightExit(NBitPlane nameTable)
        {      
            nameTable.ForEach((x, y, b) =>
            {
                if(y >= nameTable.Height - _sceneDefinition.RightEdgeFloorTiles - 4
                    && y < nameTable.Height - _sceneDefinition.RightEdgeFloorTiles
                    && x >= nameTable.Width - _sceneDefinition.RightTiles)
                {
                    nameTable[x, y] = 0;
                }
            });

            return nameTable;
        }

        private NBitPlane AddLeftExit(NBitPlane nameTable)
        {
            nameTable.ForEach((x, y, b) =>
            {
                if (y >= nameTable.Height - _sceneDefinition.LeftEdgeFloorTiles - 4
                    && y < nameTable.Height - _sceneDefinition.LeftEdgeFloorTiles
                    && x <= _sceneDefinition.LeftTiles)
                {
                    nameTable[x, y] = 0;
                }
            });

            return nameTable;
        }

        private NBitPlane AddBottomExit(NBitPlane nameTable)
        {
            int xStart = (nameTable.Width / 2) - 2;
            int width = 4;

            nameTable.ForEach((x, y, b) =>
            {

                if (x >= xStart 
                    && x < xStart + width
                    && y >= nameTable.Height - _sceneDefinition.BottomTiles)
                {
                    nameTable[x, y] = 0;
                }
            });

            return nameTable;
        }


        private NBitPlane AddEdgeTiles(NBitPlane nameTable)
        {
            return _sceneDefinition.ScrollStyle switch 
            {
                ScrollStyle.Horizontal => AddEdgeTiles(nameTable,
                                                   top: _sceneDefinition.TopTiles,
                                                   bottom: _sceneDefinition.BottomTiles),

                ScrollStyle.Vertical => AddEdgeTiles(nameTable,
                                                    left: _sceneDefinition.LeftTiles,
                                                    right: _sceneDefinition.RightTiles,
                                                    bottom: _sceneDefinition.BottomTiles),
                 
                ScrollStyle.None => AddEdgeTiles(nameTable, 
                                                    top: _sceneDefinition.TopTiles,
                                                    left: _sceneDefinition.LeftTiles,
                                                    bottom: _sceneDefinition.BottomTiles,
                                                    right: _sceneDefinition.RightTiles),

                ScrollStyle.NameTable => AddEdgeTiles(nameTable,
                                                    top: _sceneDefinition.TopTiles,
                                                    left: _sceneDefinition.LeftTiles,
                                                    bottom: _sceneDefinition.BottomTiles,
                                                    right: _sceneDefinition.RightTiles),

                _ => throw new NotImplementedException(),
            };
        }

        private NBitPlane AddShapeTiles(NBitPlane nameTable, int seed)
        {
            return _sceneDefinition.ScrollStyle switch 
            {
                ScrollStyle.Horizontal => _sceneDefinition.LevelShape switch 
                {
                    LevelShape.Flat => nameTable,
                    _ => AddGroundVariance(nameTable, seed)
                },
                ScrollStyle.Vertical => _sceneDefinition.LevelShape switch 
                {
                    LevelShape.ZigZag => AddZigZagTiles(nameTable),
                    LevelShape.Ladder => nameTable, //todo
                    _ => nameTable 
                },
                ScrollStyle.NameTable => _sceneDefinition.LevelShape switch {
                    _ => nameTable
                },
                ScrollStyle.None => _sceneDefinition.LevelShape switch {
                    LevelShape.Flat => nameTable,
                    LevelShape.CornerStairs => AddCornerStairs(nameTable),
                    LevelShape.BigStair => AddBigStair(nameTable),
                    LevelShape.TShape => AddTShape(nameTable),
                    _ => throw new NotImplementedException(),
                },
                _ => throw new NotImplementedException(),
            };
        }

        private NBitPlane AddZigZagTiles(NBitPlane nameTable)
        {
            int sectionHeight = 9;
            int gapSize = 2;
            bool left = true;

            Point p = new Point(0, _sceneDefinition.TopTiles + sectionHeight);

            while (p.Y < nameTable.Height - _sceneDefinition.BottomTiles)
            {
                if (left)
                {
                    p = new Point(p.X + 1, p.Y);
                    if(p.X == nameTable.Width - _sceneDefinition.RightTiles - gapSize)
                    {
                        left = false;
                        p.X = nameTable.Width - _sceneDefinition.RightTiles;
                        p.Y += sectionHeight;
                    }

                    if (p.Y < nameTable.Height - 1)
                    {
                        nameTable[p.X, p.Y] = 1;
                        nameTable[p.X, p.Y + 1] = 1;
                    }

                }
                else
                {
                    p = new Point(p.X - 1, p.Y);
                    if (p.X == _sceneDefinition.LeftTiles + gapSize)
                    {
                        left = true;
                        p.X = _sceneDefinition.LeftTiles;
                        p.Y += sectionHeight;
                    }

                    if (p.Y < nameTable.Height - 1)
                    {
                        nameTable[p.X, p.Y] = 1;
                        nameTable[p.X, p.Y + 1] = 1;
                    }
                }
            }

            return nameTable;
        }

        private NBitPlane AddTShape(NBitPlane nameTable)
        {
            int ceilingY = _sceneDefinition.TopTiles * 2;
            int floorY = ceilingY + (_sceneDefinition.BottomTiles * 2);

            //todo, add variation
            int pitBegin = 4 + _sceneDefinition.LeftTiles*2;
            int pitEnd = pitBegin + _sceneDefinition.RightTiles*2;

            nameTable.ForEach((x, y, b) =>
            {
                if (y <= ceilingY)
                    nameTable[x, y] = 1;
                else if (y >= floorY)
                {
                    if (x <= pitBegin || x >= pitEnd)
                        nameTable[x, y] = 1;
                    else
                        nameTable[x, y] = 0;
                }
                else
                {
                    nameTable[x, y] = 0;
                }
            });

            return nameTable;
        }

        private NBitPlane AddCornerStairs(NBitPlane nameTable)
        {
            int stairSize = 4;

            Rectangle leftStair = new Rectangle(
                _sceneDefinition.LeftTiles,
                nameTable.Height - _sceneDefinition.BottomTiles - stairSize,
                stairSize,
                stairSize);

            Rectangle rightStair = new Rectangle(
                nameTable.Width - _sceneDefinition.RightTiles - stairSize,
                nameTable.Height - _sceneDefinition.BottomTiles - stairSize,
                stairSize,
                stairSize);

            bool bigStep = _sceneDefinition.CornerStairStyle == CornerStairStyle.TwoBlockDouble;

            switch (_sceneDefinition.CornerStairStyle)
            {
                case CornerStairStyle.OneBlockDouble:
                case CornerStairStyle.TwoBlockDouble:
                    AddStairs(nameTable,
                        leftStair,
                        changeX: bigStep ?  2 : 1,
                        changeY: bigStep ? -2 : -1);

                    AddStairs(nameTable,
                        rightStair,
                        changeX: bigStep ? -2 : -1,
                        changeY: bigStep ? -2 : -1);

                    break;
                case CornerStairStyle.TwoBlockLeft:
                    AddStairs(nameTable,
                        leftStair,
                        changeX: -2,
                        changeY: -2);
                    break;
                case CornerStairStyle.TwoBlockRight:
                    AddStairs(nameTable,
                        rightStair,
                        changeX: 2,
                        changeY: -2);
                    break;
            }
            
            return nameTable;
        }

        private NBitPlane AddBigStair(NBitPlane nameTable)
        {
            AddStairs(nameTable, 
                region: new Rectangle(
                    4,
                    2,
                    nameTable.Width - 4,
                    nameTable.Height - _sceneDefinition.BottomTiles - 2),
              changeX: 2,
              changeY: 2);

            return nameTable;
        }

        private NBitPlane AddStairs(NBitPlane nameTable, Rectangle region, int changeX, int changeY)
        {
            int stepHeight = 0;
            if (changeY < 0)
            {
                stepHeight = region.Height;
            }

            int stepColumnsRemaining = changeX;

            for(int x = region.X; x < region.Right; x++)
            {
                int stairTop = region.Bottom - stepHeight;
                if (stairTop < region.Y)
                    stairTop = region.Y;

                for(int y = stairTop; y < region.Bottom; y++)
                {
                    nameTable[x, y] = 1;
                }

                if (--stepColumnsRemaining == 0)
                {
                    stepColumnsRemaining = changeX;
                    stepHeight += changeY;
                }
            }

            return nameTable;
        }


        private NBitPlane AddEdgeTiles(NBitPlane nameTable, int top = 0, int left = 0, int right = 0, int bottom = 0)
        {
            nameTable.ForEach((x, y, b) =>
            {
                if(x < left
                    || y < top
                    || x >= nameTable.Width - right 
                    || y >= nameTable.Height - bottom)
                {
                    nameTable[x, y] = 1;
                }
            });

            return nameTable;
        }

        private NBitPlane AddGroundVariance(NBitPlane nameTable, int seed)
        {
            var rng = new Random(seed);
            int nextSectionBegin = 0;

            int groundUpper = _sceneDefinition.GetBackgroundLayerTile(BackgroundLayer.Foreground, includeStatusBar: false);
            int groundLower = nameTable.Height;

            int groundPosition = rng.Next(groundUpper, groundLower);

            for (var col = 0; col < nameTable.Width; col++)
            {
                for (var row = groundUpper; row < nameTable.Height; row++)
                {

                    if (col == nextSectionBegin)
                    {
                        nextSectionBegin = nextSectionBegin + GetNextGroundSectionWidth(rng);
                        int nextGroundPosition = rng.Next(groundUpper, groundLower);
                        if (nextGroundPosition == groundPosition)
                        {
                            nextGroundPosition = groundPosition - 1;
                            if (nextGroundPosition < groundUpper)
                                nextGroundPosition = groundPosition + 1;
                        }

                        groundPosition = nextGroundPosition;
                    }

                    if (row < groundPosition)
                        nameTable[col, row] = 0;
                    else
                        nameTable[col, row] = 1;
                }
            }

            return nameTable;
        }

        private int GetNextGroundSectionWidth(Random rng)
        {
            return _sceneDefinition.LevelShape switch {
                LevelShape.LowVariance => rng.Next(8, 16),
                LevelShape.MediumVariance => rng.Next(6, 12),
                _ => rng.Next(3, 6)
            };
        }

        public SceneSpriteControllers CreateSpriteControllers(SystemMemoryBuilder memoryBuilder)
        {             
            PlayerController playerController = null;
            SpriteControllerPool<BombController> bombControllers = null;
            SpriteControllerPool<PrizeController> prizeControllers = null;
            SpriteControllerPool<DoorController> doorControllers = null;
            SpriteControllerPool<ButtonController> buttonControllers = null;
            SpriteControllerPool<PlatformController> platformControllers = null;
            SpriteControllerPool<ExplosionController> explosionControllers = null;


            if(_sceneDefinition.IsAutoScroll)
            {
                playerController = new PlayerPlaneController(_gameModule, memoryBuilder);
                bombControllers = new SpriteControllerPool<BombController>(
                   size: 2,
                   _gameModule.SpritesModule,
                   () => new BombController(_gameModule, playerController, memoryBuilder));

                prizeControllers = new SpriteControllerPool<PrizeController>(size: 10, _gameModule.SpritesModule,
                    () => new PrizeController(_gameModule, memoryBuilder));
            }
            else if (_sceneDefinition.HasSprite(SpriteType.Player))
            {
                playerController = new PlayerController(_gameModule, memoryBuilder);
                playerController.FallCheck = _sceneDefinition.SpriteFallCheck;

                bombControllers = new SpriteControllerPool<BombController>(
                     size: 2,
                     _gameModule.SpritesModule,
                     () => new BombController(_gameModule, playerController, memoryBuilder));

                prizeControllers = new SpriteControllerPool<PrizeController>(size: 2, _gameModule.SpritesModule,
                    () => new PrizeController(_gameModule, memoryBuilder));

                doorControllers = new SpriteControllerPool<DoorController>(
                    size: 2,
                    _gameModule.SpritesModule,
                    () => new DoorController(_gameModule, playerController, memoryBuilder));

                buttonControllers = new SpriteControllerPool<ButtonController>(
                    size: 2,
                    _gameModule.SpritesModule,
                    () => new ButtonController(_gameModule, playerController, memoryBuilder));

                platformControllers = new SpriteControllerPool<PlatformController>(
                    size: 4,
                    _gameModule.SpritesModule,
                    () => new PlatformController(_gameModule, memoryBuilder));
            }

            explosionControllers = new SpriteControllerPool<ExplosionController>(
                 size: 4,
                 _gameModule.SpritesModule,
                 () => new ExplosionController(_gameModule, memoryBuilder));

            ICollidableSpriteControllerPool[] spritePools = new ICollidableSpriteControllerPool[4];
            
            foreach(var sprite in _sceneDefinition.Sprites)
            {
                AssignSpriteControllers(sprite, spritePools, memoryBuilder, playerController);
            }

            return new SceneSpriteControllers(_gameModule, playerController,
                bombControllers,
                prizeControllers,
                doorControllers,
                buttonControllers,
                platformControllers,
                explosionControllers,
                spritePools);
        }

        private void AssignSpriteControllers(SpriteType spriteType, 
            ICollidableSpriteControllerPool[] spritePools,
            SystemMemoryBuilder memoryBuilder,
            PlayerController playerController)
        {
            int enemyIndex = spritePools[0] == null ? 0 : 1;
            int extraIndex = spritePools[2] == null ? 2 : 3;

            var enemyTileIndex = spritePools[0] == null ? SpriteTileIndex.Enemy1 : SpriteTileIndex.Enemy2;
            var extraTileIndex = spritePools[2] == null ? SpriteTileIndex.Extra1 : SpriteTileIndex.Extra2;

            switch (spriteType)
            {
                case SpriteType.Lizard:
                    spritePools[extraIndex] = new EnemyOrBulletSpriteControllerPool<BulletController>(
                                      2,
                                      _gameModule.SpritesModule,
                                      () => new BulletController(_gameModule, memoryBuilder, SpriteType.LizardBullet));

                    spritePools[enemyIndex] = new EnemyOrBulletSpriteControllerPool<LizardEnemyController>(
                        2,
                        _gameModule.SpritesModule,
                        () => new LizardEnemyController(spritePools[extraIndex], enemyTileIndex, _gameModule, playerController.WorldSprite, memoryBuilder));
                    break;

                case SpriteType.Plane:
                    spritePools[enemyIndex] = new EnemyOrBulletSpriteControllerPool<PlaneTakeoffController>(
                         1,
                        _gameModule.SpritesModule,
                        () => new PlaneTakeoffController(_gameModule, memoryBuilder, playerController));
                    break;

                case SpriteType.Rocket:
                    var bulletControllers = new EnemyOrBulletSpriteControllerPool<BossBulletController>(
                      8,
                      _gameModule.SpritesModule,
                      () => new BossBulletController(_gameModule, memoryBuilder));

                    spritePools[extraIndex] = bulletControllers;

                    spritePools[enemyIndex] = new EnemyOrBulletSpriteControllerPool<RocketEnemyController>(
                        4,
                        _gameModule.SpritesModule,
                        () => new RocketEnemyController(enemyTileIndex, _gameModule, playerController.WorldSprite, bulletControllers, memoryBuilder));
                    break;

                case SpriteType.Bird:
  
                    spritePools[enemyIndex] = new EnemyOrBulletSpriteControllerPool<BirdEnemyController>(
                    2,
                    _gameModule.SpritesModule,
                    () => new BirdEnemyController(playerController.WorldSprite, _gameModule, memoryBuilder, enemyTileIndex));
                    break;

                case SpriteType.Chomp:
                case SpriteType.LevelBoss:

                    AssignBossSpriteControllers(spritePools, memoryBuilder, playerController);
                    break;
            }
        }

        private void AssignBossSpriteControllers(ICollidableSpriteControllerPool[] spritePools,
            SystemMemoryBuilder memoryBuilder,
            PlayerController playerController)
        {
            int enemyIndex = spritePools[0] == null ? 0 : 1;
            int extraIndex = spritePools[2] == null ? 2 : 3;

            switch (_gameModule.CurrentLevel)
            {
                case Level.Level1_11_Boss:

                    var bullets = new EnemyOrBulletSpriteControllerPool<BossBulletController>(
                          3,
                          _gameModule.SpritesModule,
                          () => new BossBulletController(_gameModule, memoryBuilder));

                    spritePools[extraIndex] = bullets;
                    spritePools[enemyIndex] = new EnemyOrBulletSpriteControllerPool<ChompBoss1Controller>(
                            size: 1,
                            spritesModule: _gameModule.SpritesModule,
                            () => new ChompBoss1Controller(playerController.WorldSprite, bullets, _gameModule, memoryBuilder));

                    break;
                case Level.Level1_17_Boss:
      
                    var bossBulletControllers = new EnemyOrBulletSpriteControllerPool<BossBulletController>(
                        6,
                        _gameModule.SpritesModule,
                        () => new BossBulletController(_gameModule, memoryBuilder));

                    spritePools[extraIndex] = bossBulletControllers;
                    spritePools[enemyIndex] = new EnemyOrBulletSpriteControllerPool<LevelBossController>(
                          size: 1,
                          spritesModule: _gameModule.SpritesModule,
                          () => new LevelBossController(_gameModule, playerController.WorldSprite, bossBulletControllers, memoryBuilder));
                    break;

                case Level.Level2_2_Fly:
                    bossBulletControllers = spritePools[2] as EnemyOrBulletSpriteControllerPool<BossBulletController>;
                    spritePools[3] = new EnemyOrBulletSpriteControllerPool<ChompBoss2Controller>(
                            size: 1,
                            spritesModule: _gameModule.SpritesModule,
                            () => new ChompBoss2Controller(playerController.WorldSprite, bossBulletControllers, _gameModule, memoryBuilder));

                    break;
            }
        }


        public void ApplyLevelAlterations(NBitPlane levelMap)
        {
            DynamicScenePartHeader header = _gameModule.CurrentScenePartHeader;

            for (int i = 0; i < header.PartsCount; i++)
            {
                IScenePart sp = header.GetScenePart(i, _sceneDefinition, _gameModule.Specs);

                if (sp.Type == ScenePartType.Pit)
                {
                    AddPit(sp as PitScenePart, levelMap);
                    header.MarkActive(i);
                }
                else if(sp.Type == ScenePartType.Prefab)
                {
                    AddPrefab(sp as PrefabScenePart, levelMap);                   
                    header.MarkActive(i);
                }
            }
        }

        public void SetProperTiles(NBitPlane levelMap)
        {
            levelMap.ForEach(Point.Zero, new Point(levelMap.Width, levelMap.Height), (x, y, b) =>
            {
                if (b == 0)
                    return;

                bool tileAbove = y == 0 ? false : levelMap[x, y - 1] != 0;
                bool tileBelow = y == levelMap.Height-1 ? true : levelMap[x, y + 1] != 0;

                bool tileLeft = x == 0 ? true : levelMap[x - 1, y] != 0;
                bool tileRight = x == levelMap.Width-1 ? true : levelMap[x + 1, y] != 0;

                if (!tileAbove && !tileLeft)
                    levelMap[x, y] = (byte)_sceneDefinition.GroundLeftCorner;
                else if (!tileAbove && !tileRight)
                    levelMap[x, y] = (byte)_sceneDefinition.GroundRightCorner;
                else if (!tileAbove)
                    levelMap[x, y] = (byte)_sceneDefinition.GetGroundTopTile(x);
                else if (!tileLeft)
                    levelMap[x, y] = (byte)_sceneDefinition.LeftTileIndex;
                else if (!tileRight)
                    levelMap[x, y] = (byte)_sceneDefinition.RightTileIndex;
                else
                    levelMap[x, y] = (byte)_sceneDefinition.GetGroundFillTile(x, y);
            });
        }

        private void AddPit(PitScenePart part, NBitPlane levelMap)
        {
            for(int x = part.X; x <= part.X + part.Width; x++)
            {
                for(int y = levelMap.Height-1; y > 0; y--)
                {
                    if (levelMap[x, y] == 0)
                        break;

                    levelMap[x, y] = 0;
                }
            }
        }

        private void AddPrefab(PrefabScenePart part, NBitPlane levelMap)
        {
            levelMap.ForEach((x, y, _) =>
            {
                if (x >= part.X && x < part.XEnd
                    && y >= part.Y && y < part.YEnd)
                {
                    levelMap[x, y] = 1;
                }
            });   
            
            if(part.Shape == PrefabShape.StairLeft || part.Shape == PrefabShape.StairBoth)
            {
                int stepSize = 2;
                int steps = ((part.YEnd - part.Y) / stepSize);
                AddStairs(levelMap, new Rectangle(part.X - steps * stepSize, part.Y, steps * stepSize, steps * stepSize),
                    stepSize, stepSize);
            }

            if (part.Shape == PrefabShape.StairRight || part.Shape == PrefabShape.StairBoth)
            {
                int stepSize = 2;
                int steps = ((part.YEnd - part.Y) / stepSize) + 1;
                AddStairs(levelMap, new Rectangle(part.XEnd, part.Y, steps * stepSize, steps * stepSize),
                    stepSize, -stepSize);
            }
        }

        public void SetupVRAMPatternTable(
           NBitPlane masterPatternTable,
           NBitPlane vramPatternTable,
           SystemMemory memory)
        {           
            vramPatternTable.Reset();

            var builder = new VramBuilder(masterPatternTable, vramPatternTable, _spriteTileTable, memory, _gameModule.Specs);

            if (_sceneDefinition.IsLevelBossScene)
                builder.SpriteYBegin = 4;
            else
                builder.SpriteYBegin = 5;

            builder.AddStatusBarTiles();            
            _sceneDefinition.ThemeSetup.SetupVRAMPatternTable(masterPatternTable, vramPatternTable, memory);

            if(_sceneDefinition.IsAutoScroll)
            {
                //player sprite
                builder.AddSprite(SpriteTileIndex.Player, 0, 0, 1, 1);
                builder.AddSprite(SpriteTileIndex.Plane, 13, 2, 2, 1);
                builder.AddSprite(SpriteTileIndex.Bomb, 4, 1, 1, 1);
            }
            else if (_sceneDefinition.HasSprite(SpriteType.Player))
            {
                builder.AddSprite(SpriteTileIndex.Player, 0, 0, 2, 2);
                builder.AddSprite(SpriteTileIndex.Bomb, 4, 1, 1, 1);
           
                if(_sceneDefinition.HasSprite(SpriteType.Plane))
                    builder.AddSprite(SpriteTileIndex.Plane, 13, 2, 2, 1);                 
            }

            if (_sceneDefinition.HasSprite(SpriteType.LevelBoss) || _sceneDefinition.HasSprite(SpriteType.Chomp))
            {
                builder.AddBossSprites(_gameModule.CurrentLevel);
            }
            else
            {
                builder.AddSprite(SpriteTileIndex.Door, 14, 5, 2, 2);
            }

            if (_sceneDefinition.HasSprite(SpriteType.Lizard))
            {
                builder.AddEnemySprite(2, 0, 2, 2);
                var fireballTile = builder.AddExtraSprite(4, 0, 3, 1);
                _spriteTileTable.SetTile(SpriteTileIndex.Explosion, (byte)(fireballTile + 1));
            }

            if (_sceneDefinition.HasSprite(SpriteType.Rocket))
            {
                builder.AddEnemySprite(5, 1, 2, 1);
                builder.AddExtraSprite(4, 1, 1, 1);
                builder.AddSprite(SpriteTileIndex.Explosion, 5, 0, 2, 1);
            }

            if (_sceneDefinition.HasSprite(SpriteType.Bird))
                builder.AddEnemySprite(8, 0, 4, 1);

            if (_sceneDefinition.HasSprite(SpriteType.LevelBoss) || _sceneDefinition.HasSprite(SpriteType.Chomp))
            {
            }
            else
            {
                builder.AddSprite(SpriteTileIndex.Platform, 12, 5, 2, 1);
                builder.AddSprite(SpriteTileIndex.Button, 11, 6, 2, 1);
            }

            if (_sceneDefinition.HasSprite(SpriteType.Player))
            {
                builder.AddSprite(SpriteTileIndex.Prize, 7, 0, 1, 1);
            }

            builder.SpriteYBegin = 3;
            builder.AddSprite(SpriteTileIndex.Block, 13, 6, 1, 1);
            builder.AddSprite(SpriteTileIndex.Coin, 15, 0, 1, 1);
        }
         
    }
}
