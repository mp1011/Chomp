using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
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

        public LevelBuilder(ChompGameModule chompGameModule, SceneDefinition sceneDefinition)
        {
            _gameModule = chompGameModule;
            _sceneDefinition = sceneDefinition;
        }

        public void BuildBackgroundNametable(NBitPlane nameTable)
        {
            _gameModule.TileModule.NameTable.Reset();
            _gameModule.StatusBar.InitializeTiles(); 
          
            //todo, handle theme bg's diferently

            switch(_sceneDefinition.ScrollStyle)
            {
                case ScrollStyle.None:

                    int mountain1Pos = _sceneDefinition.GetBgPosition1();

                    if (mountain1Pos != 0)
                    {
                        //mountain layer 1
                        nameTable.SetFromString(0, mountain1Pos - Constants.StatusBarTiles,
                            @"0000050000000500
                                3412162534121625",
                            shouldReplace: b => b == 0);
                    }

                    break;

                case ScrollStyle.Horizontal:

                    mountain1Pos = _sceneDefinition.GetParallaxLayerTile(ParallaxLayer.Back1, includeStatusBar:false);
                    int mountain2Pos = _sceneDefinition.GetParallaxLayerTile(ParallaxLayer.Back2, includeStatusBar: false);
                    int groundPos = _sceneDefinition.GetParallaxLayerTile(ParallaxLayer.Foreground, includeStatusBar: false);

                    //mountain layer 1
                    nameTable.SetFromString(0, mountain1Pos,
                        @"0000050000000500
                            3412162534121625",
                        shouldReplace: b => b == 0);


                    //mountain layer 2
                    nameTable.SetFromString(0, mountain2Pos,
                      @"00000500001200000050000120000000
                              12341623416621234162341662123434",
                        shouldReplace: b => b == 0);

                    nameTable.ForEach((x, y, b) =>
                    {
                        if (y >= mountain2Pos + 2 && y < groundPos)
                        {
                            nameTable[x, y] = 6;
                        }
                    });

                    var vramNametable = _gameModule.TileModule.NameTable;

                    nameTable.CopyTo(
                        destination: vramNametable,
                        source: new InMemoryByteRectangle(0, 0, vramNametable.Width, vramNametable.Height-2),
                        destinationPoint: new Point(0, 2),
                        specs: _gameModule.Specs,
                        memory: _gameModule.GameSystem.Memory);

                    break;
            }
            //int layerABegin = 2 + _sceneDefinition.BeginTiles + _sceneDefinition.ParallaxLayerABeginTile;
            //int layerBBegin = layerABegin + _sceneDefinition.ParallaxLayerBTiles;
            //int layerCBegin = layerBBegin + _sceneDefinition.ParallaxLayerATiles;




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

            return _sceneDefinition.ScrollStyle switch {
                ScrollStyle.Horizontal => BuildAttributeTable_Horizontal(attributeTable, nameTable),
                _ => BuildAttributeTable_Default(attributeTable, nameTable)
            };          
        }

        private NBitPlane BuildAttributeTable_Default(NBitPlane attributeTable, NBitPlane nameTable)
        {
            int mountainAttributePos = _sceneDefinition.GetBgPosition1() / _gameModule.Specs.AttributeTableBlockSize;
            if (mountainAttributePos == 0)
                mountainAttributePos = 255;

            attributeTable.ForEach((x, y, b) =>
            {
                bool isSolid = nameTable[x * 2, y * 2] != 0
                    || nameTable[(x * 2) + 1, (y * 2) + 1] != 0;

                if (y > mountainAttributePos)
                    attributeTable[x, y] = 0; //green tiles, gray 
                else
                    attributeTable[x, y] = 1;

                if (isSolid || y > mountainAttributePos)
                    attributeTable[x, y] = 0;
                else
                    attributeTable[x, y] = 1;

            });

            return attributeTable;
        }

        private NBitPlane BuildAttributeTable_Horizontal(NBitPlane attributeTable, NBitPlane nameTable)
        {
            int groundPosition = _sceneDefinition.GetParallaxLayerTile(ParallaxLayer.Foreground, includeStatusBar: false) / _gameModule.Specs.AttributeTableBlockSize;
            
            attributeTable.ForEach((x, y, b) =>
            {
                if (y >= groundPosition)
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

            return nameTable;
        }

        private NBitPlane AddEdgeTiles(NBitPlane nameTable)
        {
            return _sceneDefinition.ScrollStyle switch 
            {
                ScrollStyle.Horizontal => AddEdgeTiles(nameTable,
                                                   top: _sceneDefinition.BeginTiles,
                                                   bottom: _sceneDefinition.EndTiles),
                ScrollStyle.None => AddEdgeTiles(nameTable, 
                                                    top: _sceneDefinition.TopTiles,
                                                    left: _sceneDefinition.LeftTiles,
                                                    bottom: _sceneDefinition.BottomTiles,
                                                    right: _sceneDefinition.RightTiles),
                _ => throw new NotImplementedException(),
            };
        }

        private NBitPlane AddShapeTiles(NBitPlane nameTable, int seed)
        {
            return _sceneDefinition.ScrollStyle switch {
                ScrollStyle.Horizontal => _sceneDefinition.LevelShape switch {
                    LevelShape.Flat => nameTable,
                    _ => AddGroundVariance(nameTable, seed)
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

            AddStairs(nameTable, new Rectangle(
                _sceneDefinition.LeftTiles*2, 
                nameTable.Height - _sceneDefinition.BottomTiles*2 - stairSize,
                stairSize,
                stairSize), 
                riseRight: false);

            AddStairs(nameTable, new Rectangle(
                nameTable.Width - _sceneDefinition.RightTiles*2 - stairSize,
                nameTable.Height - _sceneDefinition.BottomTiles*2 - stairSize,
                stairSize,
                stairSize),
                riseRight: true);

            return nameTable;
        }

        private NBitPlane AddBigStair(NBitPlane nameTable)
        {
            int stairSize = 10;
            AddStairs(nameTable, new Rectangle(
              nameTable.Width - _sceneDefinition.RightTiles*2 - stairSize,
              nameTable.Height - _sceneDefinition.BottomTiles*2 - stairSize,
              stairSize,
              stairSize),
              riseRight: true);

            return nameTable;
        }

        private NBitPlane AddStairs(NBitPlane nameTable, Rectangle region, bool riseRight )
        {

            nameTable.ForEach(new Point(region.X, region.Y), new Point(region.Right, region.Bottom), (x, y, b) =>
            {
                int stairY = y - region.Y;
                int stairX = x - region.X;

                if (riseRight && stairX >= region.Width - stairY - 1)
                    nameTable[x, y] = 1;
                else if (!riseRight && stairX <= stairY)
                    nameTable[x, y] = 1;
            });

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
            return nameTable;
            //var rng = new Random(seed);
            //int nextSectionBegin = 0;

            //int groundUpper = _sceneDefinition.ParallaxEndTile;
            //int groundLower = nameTable.Height+1;

            //int groundPosition = rng.Next(groundUpper, groundLower);
            
            //for (var col = 0; col < nameTable.Width; col++)
            //{
            //    for (var row = _sceneDefinition.ParallaxEndTile; row < nameTable.Height; row++)
            //    {
               
            //        if(col == nextSectionBegin)
            //        {
            //            nextSectionBegin = nextSectionBegin + GetNextGroundSectionWidth(rng);
            //            int nextGroundPosition = rng.Next(groundUpper, groundLower);
            //            if(nextGroundPosition == groundPosition)
            //            {
            //                nextGroundPosition = groundPosition - 1;
            //                if (nextGroundPosition < groundUpper)
            //                    nextGroundPosition = groundPosition + 1;
            //            }

            //            groundPosition = nextGroundPosition;
            //        }

            //        if (row < groundPosition)
            //            nameTable[col, row] = 0;
            //        else 
            //            nameTable[col, row] = 1;
            //    }
            //}
            
            //return nameTable;
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


            if (_sceneDefinition.HasSprite(SpriteLoadFlags.Player))
            {
                playerController = new PlayerController(_gameModule, memoryBuilder);

                bombControllers = new SpriteControllerPool<BombController>(
                     size: 2,
                     _gameModule.SpritesModule,
                     () => new BombController(_gameModule, playerController, memoryBuilder));
            }

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
                    () => new LizardEnemyController(extraA, _gameModule, memoryBuilder));
            }

            if (_sceneDefinition.HasSprite(SpriteLoadFlags.Bird))
            {
                if (enemyA == null)
                {
                    enemyA = new EnemyOrBulletSpriteControllerPool<BirdEnemyController>(
                        2,
                        _gameModule.SpritesModule,
                        () => new BirdEnemyController(playerController.WorldSprite, _gameModule, memoryBuilder));
                }
                else
                {
                    enemyB = new EnemyOrBulletSpriteControllerPool<BirdEnemyController>(
                       2,
                       _gameModule.SpritesModule,
                       () => new BirdEnemyController(playerController.WorldSprite, _gameModule, memoryBuilder));
                }
            }

            return new SceneSpriteControllers(_gameModule, playerController, bombControllers, enemyA, enemyB, extraA, extraB);
        }

        public void ApplyLevelAlterations(NBitPlane levelMap)
        {
            ScenePartsHeader header = new ScenePartsHeader(_gameModule.CurrentLevel, _gameModule.GameSystem.Memory);

            for (int i = 0; i < header.PartsCount; i++)
            {
                ScenePart sp = new ScenePart(_gameModule.GameSystem.Memory, header.FirstPartAddress + (ScenePart.Bytes * i), _sceneDefinition);

                if (sp.Type == ScenePartType.Pit)
                {
                    AddPit(sp, levelMap);
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
                    levelMap[x, y] = (byte)_sceneDefinition.LeftTile;
                else if (!tileRight)
                    levelMap[x, y] = (byte)_sceneDefinition.RightTile;
                else
                    levelMap[x, y] = (byte)_sceneDefinition.GetGroundFillTile(x, y);
            });
        }

        private void AddPit(ScenePart part, NBitPlane levelMap)
        {
            for(int x = part.X; x <= part.X + part.Y; x++)
            {
                for(int y = levelMap.Height-1; y > 0; y--)
                {
                    if (levelMap[x, y] == 0)
                        break;

                    levelMap[x, y] = 0;
                }
            }
        }

       

        public void SetupVRAMPatternTable(
           NBitPlane masterPatternTable,
           NBitPlane vramPatternTable,
           SystemMemory memory)
        {
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
                source: new InMemoryByteRectangle(4, 4, 8, 1),
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

            // row 2 - health guage
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 4, 4, 1),
                destinationPoint: new Point(0, 2),
                _gameModule.Specs,
                memory);

            // row 3 - background
            // row 4 - foreground
            //todo- store theme definition separately
            switch (_sceneDefinition.Theme)
            {
                case Theme.Plains:
                    //tile row 1
                    masterPatternTable.CopyTilesTo(
                        destination: vramPatternTable,
                        source: new InMemoryByteRectangle(0, 5, 8, 1),
                        destinationPoint: new Point(1, 3),
                        _gameModule.Specs,
                        memory);

                    //tile row 2
                    masterPatternTable.CopyTilesTo(
                        destination: vramPatternTable,
                        source: new InMemoryByteRectangle(0, 12, 8, 1),
                        destinationPoint: new Point(0, 4),
                        _gameModule.Specs,
                        memory);
                    break;
            }

            GridPoint spriteDestination = new ByteGridPoint(_gameModule.Specs.PatternTableTilesAcross, _gameModule.Specs.PatternTableTilesDown);
            spriteDestination.Y = 5;

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

                spriteDestination.Advance(2, extraRowSkip: 1);

                //fireball sprite
                masterPatternTable.CopyTilesTo(
                  destination: vramPatternTable,
                  source: new InMemoryByteRectangle(4, 0, 4, 1),
                  destinationPoint: new Point(spriteDestination.X, spriteDestination.Y),
                  _gameModule.Specs,
                  memory);

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

                spriteDestination.Advance(4, extraRowSkip: 1);
            }
        }
    }
}
