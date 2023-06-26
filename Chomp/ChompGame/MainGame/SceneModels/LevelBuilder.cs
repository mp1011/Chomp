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
          
            switch(_sceneDefinition.Theme)
            {
                case ThemeType.Plains:
                case ThemeType.PlainsEvening:

                    byte mountain1Pos, mountain2Pos, groundPos;
                  
                    mountain1Pos = (byte)_sceneDefinition.GetBackgroundLayerTile(BackgroundLayer.Back1, includeStatusBar: false);
                    mountain2Pos = (byte)(_sceneDefinition.GetBackgroundLayerTile(BackgroundLayer.Back2, includeStatusBar: false));
                    groundPos = (byte)_sceneDefinition.GetBackgroundLayerTile(BackgroundLayer.Foreground, includeStatusBar: false);
                   
                    AddPlainsMountainTiles(nameTable, mountain1Pos, mountain2Pos, groundPos);

                    break;
            }

            var vramNametable = _gameModule.TileModule.NameTable;

            nameTable.CopyTo(
                destination: vramNametable,
                source: new InMemoryByteRectangle(0, 0, vramNametable.Width, vramNametable.Height - 2),
                destinationPoint: new Point(0, 2),
                specs: _gameModule.Specs,
                memory: _gameModule.GameSystem.Memory);
        }

        private void AddPlainsMountainTiles(NBitPlane nameTable, byte mountain1Pos, byte mountain2Pos, byte groundPos)
        {
            string layer1Row1 = "00000C00";
            string layer1Row2 = "AB89859C";

            //mountain layer 1
            nameTable.SetFromString(0, mountain1Pos,
                $@"{layer1Row1}{layer1Row1}{layer1Row1}{layer1Row1}
                         {layer1Row2}{layer1Row2}{layer1Row2}{layer1Row2}",
                shouldReplace: b => b == 0);

            string layer2Row1 = "000C000089000000";
            string layer2Row2 = "AB859AB855989AB0";


            //mountain layer 2
            nameTable.SetFromString(0, mountain2Pos,
              $@"{layer2Row1}{layer2Row1}
                       {layer2Row2}{layer2Row2}",
                shouldReplace: b => b == 0);
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

            if (_sceneDefinition.IsLevelBossScene)
                return BuildAttributeTable_LevelBoss(attributeTable);

            return BuildAttributeTable_Default(attributeTable, nameTable);
        }

        private NBitPlane BuildAttributeTable_Default(NBitPlane attributeTable, NBitPlane nameTable)
        {
            int foreGroundAttributePosition = _sceneDefinition.GetBackgroundLayerTile(BackgroundLayer.ForegroundStart, false) / _gameModule.Specs.AttributeTableBlockSize;
            
            attributeTable.ForEach((x, y, b) =>
            {
                bool isSolid = nameTable[x * 2, y * 2] != 0
                    || nameTable[(x * 2) + 1, (y * 2) + 1] != 0;

                if (isSolid || y >= foreGroundAttributePosition)
                    attributeTable[x, y] = 1;
                else
                    attributeTable[x, y] = 0;

            });

            return attributeTable;
        }

        private NBitPlane BuildAttributeTable_LevelBoss(NBitPlane attributeTable)
        {
            int floorPos = attributeTable.Height - 1;
            attributeTable.ForEach((x, y, b) =>
            {
                if(y == floorPos)
                    attributeTable[x, y] = 1;
                else 
                    attributeTable[x, y] = 0;
            });

            return attributeTable;
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
            SpriteControllerPool<DoorController> doorControllers = null;
            SpriteControllerPool<ButtonController> buttonControllers = null;
            SpriteControllerPool<PlatformController> platformControllers = null;
            SpriteControllerPool<ExplosionController> explosionControllers = null;


            if (_sceneDefinition.HasSprite(SpriteLoadFlags.Player))
            {
                playerController = new PlayerController(_gameModule, memoryBuilder);
                playerController.FallCheck = _sceneDefinition.SpriteFallCheck;

                bombControllers = new SpriteControllerPool<BombController>(
                     size: 2,
                     _gameModule.SpritesModule,
                     () => new BombController(_gameModule, playerController, memoryBuilder));

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

            IEnemyOrBulletSpriteControllerPool enemyA = null, enemyB = null, extraA = null, extraB = null;

           
            if(_sceneDefinition.HasSprite(SpriteLoadFlags.Lizard))
            {
                extraA = new EnemyOrBulletSpriteControllerPool<BulletController>(
                   2,
                   _gameModule.SpritesModule,
                   () => new BulletController(_gameModule, memoryBuilder, SpriteType.LizardBullet));


                enemyA = new EnemyOrBulletSpriteControllerPool<LizardEnemyController>(
                    2,
                    _gameModule.SpritesModule,
                    () => new LizardEnemyController(extraA, SpriteTileIndex.Enemy1, _gameModule, playerController.WorldSprite, memoryBuilder));
            }

            if (_sceneDefinition.HasSprite(SpriteLoadFlags.Bird))
            {
                if (enemyA == null)
                {
                    enemyA = new EnemyOrBulletSpriteControllerPool<BirdEnemyController>(
                        2,
                        _gameModule.SpritesModule,
                        () => new BirdEnemyController(playerController.WorldSprite, _gameModule, memoryBuilder, SpriteTileIndex.Enemy1));
                }
                else
                {
                    enemyB = new EnemyOrBulletSpriteControllerPool<BirdEnemyController>(
                       2,
                       _gameModule.SpritesModule,
                       () => new BirdEnemyController(playerController.WorldSprite, _gameModule, memoryBuilder, SpriteTileIndex.Enemy2));
                }
            }

            if (_sceneDefinition.HasSprite(SpriteLoadFlags.Boss))
            {
                if (_gameModule.CurrentLevel == Level.Level1_11_Boss)
                {
                    var bullets = new EnemyOrBulletSpriteControllerPool<BossBulletController>(
                          3,
                          _gameModule.SpritesModule,
                          () => new BossBulletController(_gameModule, memoryBuilder));

                    extraA = bullets;
                    enemyA = new EnemyOrBulletSpriteControllerPool<ChompBoss1Controller>(
                            size: 1,
                            spritesModule: _gameModule.SpritesModule,
                            () => new ChompBoss1Controller(playerController.WorldSprite, bullets, _gameModule, memoryBuilder));
                }
                else if (_gameModule.CurrentLevel == Level.Level1_17_Boss)
                {
                   var bossBulletControllers = new EnemyOrBulletSpriteControllerPool<BossBulletController>(
                       6,
                       _gameModule.SpritesModule,
                       () => new BossBulletController(_gameModule, memoryBuilder));

                    extraA = bossBulletControllers;
                    enemyA = new EnemyOrBulletSpriteControllerPool<LevelBossController>(
                          size: 1,
                          spritesModule: _gameModule.SpritesModule,
                          () => new LevelBossController(_gameModule, playerController.WorldSprite, bossBulletControllers, memoryBuilder));

                }
            }

            return new SceneSpriteControllers(_gameModule, playerController,
                bombControllers,
                doorControllers,
                buttonControllers,
                platformControllers,
                explosionControllers,
                enemyA, 
                enemyB, 
                extraA, 
                extraB);
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

            //row 0 - top status bar text
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(4, 3, 7, 1),
                destinationPoint: new Point(1, 0),
                _gameModule.Specs,
                memory);

            //row 1 - bottom status bar text
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(5, 4, 8, 1),
                destinationPoint: new Point(0, 1),
                _gameModule.Specs,
                memory);

            // row 2 - more text
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(12, 4, 2, 1),
                destinationPoint: new Point(6, 2),
                _gameModule.Specs,
                memory);

            // row 2 - health guage, filled tile
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 4, 5, 1),
                destinationPoint: new Point(1, 2),
                _gameModule.Specs,
                memory);

            // row 3 - background
            // row 4 - foreground
            //todo- store theme definition separately
            switch (_sceneDefinition.Theme)
            {
                case ThemeType.Plains:
                case ThemeType.PlainsEvening:
                case ThemeType.PlainsBoss:

                    //need a better way to identify level bosses
                    if (_gameModule.CurrentLevel == Level.Level1_17_Boss)
                    {
                        masterPatternTable.CopyTilesTo(
                              destination: vramPatternTable,
                              source: new InMemoryByteRectangle(0, 12, 6, 1),
                              destinationPoint: new Point(0, 3),
                              _gameModule.Specs,
                              memory);
                    }
                    else
                    {
                        //tile row 1
                        masterPatternTable.CopyTilesTo(
                            destination: vramPatternTable,
                            source: new InMemoryByteRectangle(0, 5, 8, 1),
                            destinationPoint: new Point(0, 3),
                            _gameModule.Specs,
                            memory);

                        //tile row 2
                        masterPatternTable.CopyTilesTo(
                            destination: vramPatternTable,
                            source: new InMemoryByteRectangle(0, 12, 8, 1),
                            destinationPoint: new Point(0, 4),
                            _gameModule.Specs,
                            memory);
                    }

                    break;
            }

            GridPoint spriteDestination = new ByteGridPoint(_gameModule.Specs.PatternTableTilesAcross, _gameModule.Specs.PatternTableTilesDown);
            spriteDestination.Y = 5;

            //really need to fix this
            if (_gameModule.CurrentLevel == Level.Level1_17_Boss)
                spriteDestination.Y = 4;

            if (_sceneDefinition.HasSprite(SpriteLoadFlags.Player))
            {
                //player sprite
                masterPatternTable.CopyTilesTo(
                  destination: vramPatternTable,
                  source: new InMemoryByteRectangle(0, 0, 2, 2),
                  destinationPoint: new Point(spriteDestination.X, spriteDestination.Y),
                  _gameModule.Specs,
                  memory);

                //bomb sprite
                masterPatternTable.CopyTilesTo(
                  destination: vramPatternTable,
                  source: new InMemoryByteRectangle(5, 1, 1, 1),
                  destinationPoint: new Point(spriteDestination.X+1, spriteDestination.Y),
                  _gameModule.Specs,
                  memory);
                _spriteTileTable.SetTile(SpriteTileIndex.Bomb,2);

                spriteDestination.Advance(2, extraRowSkip: 1);

               
            }

            if (_sceneDefinition.HasSprite(SpriteLoadFlags.Lizard))
            {
                //lizard sprite
                masterPatternTable.CopyTilesTo(
                  destination: vramPatternTable,
                  source: new InMemoryByteRectangle(2, 0, 2, 2),
                  destinationPoint: new Point(spriteDestination.X, spriteDestination.Y),
                  _gameModule.Specs,
                  memory);
                _spriteTileTable.SetTile(SpriteTileIndex.Enemy1, 3);

                spriteDestination.Advance(2, extraRowSkip: 1);

                //fireball sprite
                masterPatternTable.CopyTilesTo(
                  destination: vramPatternTable,
                  source: new InMemoryByteRectangle(4, 0, 4, 1),
                  destinationPoint: new Point(spriteDestination.X, spriteDestination.Y),
                  _gameModule.Specs,
                  memory);

                _spriteTileTable.SetTile(SpriteTileIndex.Extra1, 5);
                _spriteTileTable.SetTile(SpriteTileIndex.Explosion, 6);

                spriteDestination.Advance(4, extraRowSkip: 1);
            }

            if (_sceneDefinition.HasSprite(SpriteLoadFlags.Bird))
            {
                //bird sprite
                masterPatternTable.CopyTilesTo(
                  destination: vramPatternTable,
                  source: new InMemoryByteRectangle(8, 0, 4, 1),
                  destinationPoint: new Point(spriteDestination.X, spriteDestination.Y),
                  _gameModule.Specs,
                  memory);
                _spriteTileTable.SetTile(SpriteTileIndex.Enemy2, 17);
                spriteDestination.Advance(4, extraRowSkip: 1);
            }

            if(_sceneDefinition.HasSprite(SpriteLoadFlags.Boss))
            {
                AddBossSprites(masterPatternTable, vramPatternTable, memory, spriteDestination);
            }

            if (!_sceneDefinition.HasSprite(SpriteLoadFlags.Boss))
            {
                //platform
                masterPatternTable.CopyTilesTo(
                   destination: vramPatternTable,
                   source: new InMemoryByteRectangle(12, 5, 2, 1),
                   destinationPoint: new Point(4, 6),
                   _gameModule.Specs,
                   memory);
                _spriteTileTable.SetTile(SpriteTileIndex.Platform, 13);

                //door
                masterPatternTable.CopyTilesTo(
                    destination: vramPatternTable,
                    source: new InMemoryByteRectangle(14, 5, 2, 2),
                    destinationPoint: new Point(6, 6),
                    _gameModule.Specs,
                    memory);

                //button
                masterPatternTable.CopyTilesTo(
                   destination: vramPatternTable,
                   source: new InMemoryByteRectangle(11, 6, 2, 1),
                   destinationPoint: new Point(4, 7),
                   _gameModule.Specs,
                   memory);
                _spriteTileTable.SetTile(SpriteTileIndex.Button, 21); 
            }

            //block 
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(13, 6, 1, 1),
                destinationPoint: new Point(6, 3),
                _gameModule.Specs,
                memory);
            _spriteTileTable.SetTile(SpriteTileIndex.Block, 14);

            //coin 
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(15, 0, 1, 1),
                destinationPoint: new Point(7, 3),
                _gameModule.Specs,
                memory);
            _spriteTileTable.SetTile(SpriteTileIndex.Coin, 15);
        }
    
        private void AddBossSprites(
            NBitPlane masterPatternTable,
            NBitPlane vramPatternTable,
            SystemMemory memory,
            GridPoint spriteDestination)
        {
            if(_gameModule.CurrentLevel == Level.Level1_11_Boss)
            {
                masterPatternTable.CopyTilesTo(
                  destination: vramPatternTable,
                  source: new InMemoryByteRectangle(8, 1, 4, 2),
                  destinationPoint: new Point(spriteDestination.X, spriteDestination.Y),
                  _gameModule.Specs,
                  memory);

                spriteDestination.Advance(4, extraRowSkip: 0);

                masterPatternTable.CopyTilesTo(
                    destination: vramPatternTable,
                    source: new InMemoryByteRectangle(7, 1, 1, 1),
                    destinationPoint: new Point(spriteDestination.X, spriteDestination.Y),
                    _gameModule.Specs,
                    memory);

                spriteDestination.Advance(1, extraRowSkip: 0);

                masterPatternTable.CopyTilesTo(
                    destination: vramPatternTable,
                    source: new InMemoryByteRectangle(12, 2, 1, 1),
                    destinationPoint: new Point(spriteDestination.X, spriteDestination.Y),
                    _gameModule.Specs,
                    memory);

                _spriteTileTable.SetTile(SpriteTileIndex.Enemy1, 3);
                _spriteTileTable.SetTile(SpriteTileIndex.Enemy2, 7);
                _spriteTileTable.SetTile(SpriteTileIndex.Extra1, 8);
                _spriteTileTable.SetTile(SpriteTileIndex.Explosion, 17);

                //explosion
                masterPatternTable.CopyTilesTo(
                  destination: vramPatternTable,
                  source: new InMemoryByteRectangle(5, 0, 3, 1),
                  destinationPoint: new Point(0, 7),
                  _gameModule.Specs,
                  memory);
            }
            else if(_gameModule.CurrentLevel == Level.Level1_17_Boss)
            {
                //body
                masterPatternTable.CopyTilesTo(
                 destination: vramPatternTable,
                 source: new InMemoryByteRectangle(11, 9, 5, 3),
                 destinationPoint: new Point(spriteDestination.X+1, spriteDestination.Y),
                 _gameModule.Specs,
                 memory);

                //eye
                masterPatternTable.CopyTilesTo(
                    destination: vramPatternTable,
                    source: new InMemoryByteRectangle(11, 12, 2, 2),
                    destinationPoint: new Point(spriteDestination.X - 2, spriteDestination.Y + 2),
                    _gameModule.Specs,
                    memory);

                //bullet
                masterPatternTable.CopyTilesTo(
                  destination: vramPatternTable,
                  source: new InMemoryByteRectangle(4, 0, 4, 1),
                  destinationPoint: new Point(spriteDestination.X, spriteDestination.Y + 3),
                  _gameModule.Specs,
                  memory);

                //bullet2
                masterPatternTable.CopyTilesTo(
                  destination: vramPatternTable,
                  source: new InMemoryByteRectangle(10, 7, 1, 1),
                  destinationPoint: new Point(spriteDestination.X+5, spriteDestination.Y + 3),
                  _gameModule.Specs,
                  memory);

                _spriteTileTable.SetTile(SpriteTileIndex.Enemy1, 17);
                _spriteTileTable.SetTile(SpriteTileIndex.Enemy2, 23);
                _spriteTileTable.SetTile(SpriteTileIndex.Extra1, 27);
                _spriteTileTable.SetTile(SpriteTileIndex.Extra2, 32);
                _spriteTileTable.SetTile(SpriteTileIndex.Explosion, 28);

            }


        }
    }
}
