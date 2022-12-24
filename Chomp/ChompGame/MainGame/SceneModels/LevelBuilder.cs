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

        private void FillStatusBarTopLine()
        {
            _gameModule.TileModule.NameTable
                .SetFromString(@"01277734562777777");            
        }

        public void BuildBackgroundNametable()
        {
            _gameModule.TileModule.NameTable.Reset();

            FillStatusBarTopLine();

            int layerABegin = 2 + _sceneDefinition.BeginTiles + _sceneDefinition.ParallaxLayerABeginTile;
            int layerBBegin = layerABegin + _sceneDefinition.ParallaxLayerBTiles;
            int layerCBegin = layerBBegin + _sceneDefinition.ParallaxLayerATiles;
         
            //mountain layer 1
            _gameModule.TileModule.NameTable.SetFromString(0, layerBBegin,
                @"00000500000005000000050000000500
                        34121625341216253412162534121625");

            //mountain layer 2
            _gameModule.TileModule.NameTable.SetFromString(0, layerCBegin,
              @"00000500001200000050000120000000
                      12341623416621234162341662123434");
        }

        public NBitPlane BuildAttributeTable(SystemMemory memory, int nameTableBytes)
        {
            //fix address
            NBitPlane attributeTable = NBitPlane.Create(
                memory.GetAddress(AddressLabels.FreeRAM) + nameTableBytes, memory, 
                _gameModule.Specs.AttributeTableBitsPerBlock,
               _sceneDefinition.LevelTileWidth / _gameModule.Specs.AttributeTableBlockSize,
               _sceneDefinition.LevelTileHeight / _gameModule.Specs.AttributeTableBlockSize);

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
                _ => throw new NotImplementedException(),
            };
        }

        private NBitPlane AddEdgeTiles(NBitPlane nameTable, int top = 0, int left = 0, int right = 0, int bottom = 0)
        {
            for(var row = 0; row < top; row++)
            {
                for(var col = 0; col < nameTable.Width; col++)
                {
                    nameTable[row, col] = (byte)_sceneDefinition.GetGroundFillTile(row, col);
                }
            }

            for (var row = nameTable.Height-bottom; row < nameTable.Height; row++)
            {
                for (var col = 0; col < nameTable.Width; col++)
                {
                    if (row == nameTable.Height - bottom)
                        nameTable[col, row] = (byte)_sceneDefinition.GetGroundTopTile(col);
                    else 
                        nameTable[col, row] = (byte)_sceneDefinition.GetGroundFillTile(row, col);
                }
            }

            if (left != 0 || right != 0)
                throw new NotImplementedException();

            return nameTable;
        }




        private NBitPlane AddGroundVariance(NBitPlane nameTable, int seed)
        {
            var rng = new Random(seed);
            int nextSectionBegin = 0;
            int currentSectionBegin = 0;

            int groundUpper = _sceneDefinition.ParallaxEndTile;
            int groundLower = nameTable.Height+1;

            int groundPosition = rng.Next(groundUpper, groundLower);
            
            for (var col = 0; col < nameTable.Width; col++)
            {
                for (var row = _sceneDefinition.ParallaxEndTile; row < nameTable.Height; row++)
                {
               
                    if(col == nextSectionBegin)
                    {
                        nextSectionBegin = nextSectionBegin + GetNextGroundSectionWidth(rng);
                        int nextGroundPosition = rng.Next(groundUpper, groundLower);
                        if(nextGroundPosition == groundPosition)
                        {
                            nextGroundPosition = groundPosition - 1;
                            if (nextGroundPosition < groundUpper)
                                nextGroundPosition = groundPosition + 1;
                        }

                        groundPosition = nextGroundPosition;
                        currentSectionBegin = col;
                    }

                    if (row < groundPosition)
                        nameTable[col, row] = 0;
                    else if (row == groundPosition && col == currentSectionBegin)
                        nameTable[col, row] = (byte)_sceneDefinition.GroundLeftCorner;
                    else if (row == groundPosition && col == nextSectionBegin - 1)
                        nameTable[col, row] = (byte)_sceneDefinition.GroundRightCorner;
                    else if (row == groundPosition)
                        nameTable[col, row] = (byte)_sceneDefinition.GetGroundTopTile(col);
                    else if(col == currentSectionBegin)
                        nameTable[col, row] = (byte)_sceneDefinition.GetLeftSideTile(row);
                    else if (col == nextSectionBegin - 1)
                        nameTable[col, row] = (byte)_sceneDefinition.GetRightSideTile(row);
                    else 
                        nameTable[col, row] = (byte)_sceneDefinition.GetGroundFillTile(row,col);
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
            ScenePartsHeader header = new ScenePartsHeader(_gameModule.GameSystem.Memory.GetAddress(AddressLabels.SceneParts), _gameModule.GameSystem.Memory);

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
            //tile row 1
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, (int)_sceneDefinition.TileRow, 8, 1),
                destinationPoint: new Point(0, 0),
                _gameModule.Specs,
                memory);

            //tile row 2
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(8, (int)_sceneDefinition.TileRow, 8, 1),
                destinationPoint: new Point(0, 1),
                _gameModule.Specs,
                memory);

            GridPoint spriteDestination = new ByteGridPoint(_gameModule.Specs.PatternTableTilesAcross, _gameModule.Specs.PatternTableTilesDown);
            spriteDestination.Y = 2;

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

                //status bar text
                masterPatternTable.CopyTilesTo(
                    destination: vramPatternTable,
                    source: new InMemoryByteRectangle(4, 3, 8, 2),
                    destinationPoint: new Point(0, 5),
                    _gameModule.Specs,
                    memory);

                //status bar text 2
                masterPatternTable.CopyTilesTo(
                    destination: vramPatternTable,
                    source: new InMemoryByteRectangle(12, 4, 2, 1),
                    destinationPoint: new Point(6, 7),
                    _gameModule.Specs,
                    memory);

                //health guage
                masterPatternTable.CopyTilesTo(
                    destination: vramPatternTable,
                    source: new InMemoryByteRectangle(0, 4, 4, 1),
                    destinationPoint: new Point(0, 7),
                    _gameModule.Specs,
                    memory);
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
