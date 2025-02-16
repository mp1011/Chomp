using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels.SceneParts;
using ChompGame.MainGame.SpriteControllers;
using ChompGame.MainGame.SpriteControllers.Bosses;
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

        public void BuildBackgroundNametable(NBitPlane nameTable, NBitPlane attributeTable)
        {
            _gameModule.TileModule.NameTable.Reset();
            _gameModule.StatusBar.InitializeTiles();

            var themeSetup = _sceneDefinition.CreateThemeSetup(_gameModule);

            themeSetup.BuildBackgroundNameTable(nameTable);

            foreach (var b in themeSetup.SmartBackgroundBlocks)
                b.Apply(nameTable, attributeTable);

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

            attributeTable = _sceneDefinition.CreateThemeSetup(_gameModule)
                                             .BuildAttributeTable(attributeTable, nameTable);

            return attributeTable;
        }

        public NBitPlane BuildNameTable(SystemMemoryBuilder memoryBuilder, byte seed)
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


        public NBitPlane AddExitTiles(NBitPlane nameTable)
        {
            ScenePartsHeader header = new ScenePartsHeader(_gameModule.CurrentLevel, _gameModule.GameSystem.Memory);

            for (int i = 0; i < header.PartsCount; i++)
            {
                var sp = new ExitScenePart(_gameModule.GameSystem.Memory, header.FirstPartAddress + (BaseScenePart.Bytes * i), _sceneDefinition, _gameModule.Specs);

                if (sp.Type != ScenePartType.SideExit)
                    continue;

                nameTable = sp.ExitType switch {
                    ExitType.Right => AddRightExit(nameTable),
                    ExitType.Left => AddLeftExit(nameTable),
                    ExitType.Bottom => AddBottomExit(nameTable),
                    ExitType.Top => AddTopExit(nameTable),
                    _ => nameTable };
            }

            return nameTable;
        }

        private bool NeedsRightExit(NBitPlane nameTable)
        {
            if (_gameModule.CurrentLevel == Level.Level3_20_Midboss)
                return false;

            bool maybeExit = false;
            var maybeExitBegin = 0;

            for(int y = 2; y < nameTable.Height;y++)
            {
                //only checking the last row, 
                //will adjust if that isn't good enough
                int x = nameTable.Width - 1;
                if (nameTable[x, y] == 1)
                {
                    if(maybeExit && (y - maybeExitBegin) >= 4)
                        return false;

                    maybeExit = false;
                    continue;
                }

                if(!maybeExit)
                {
                    maybeExit = true;
                    maybeExitBegin = y;
                }
            }

            return true;
        }

        private bool NeedsLeftExit(NBitPlane nameTable)
        {
            bool maybeExit = false;
            var maybeExitBegin = 0;

            for (int y = 2; y < nameTable.Height; y++)
            {
                //only checking the first row, 
                //will adjust if that isn't good enough
                int x = 0;
                if (nameTable[x, y] == 1)
                {
                    if (maybeExit && (y - maybeExitBegin) >= 4)
                        return false;

                    maybeExit = false;
                    continue;
                }

                if (!maybeExit)
                {
                    maybeExit = true;
                    maybeExitBegin = y;
                }
            }

            return true;
        }

        private NBitPlane AddRightExit(NBitPlane nameTable)
        {
            if (!NeedsRightExit(nameTable))
                return nameTable;

            nameTable.ForEach((x, y, b) =>
            {
                if(_sceneDefinition.ScrollStyle == ScrollStyle.None && 
                    _sceneDefinition.LevelShape == LevelShape.TShape)
                {
                    int leftWall = 2;
                    int rightWall = nameTable.Width - 2;
                    int leftCeiling = 2 + _sceneDefinition.LeftTiles;
                    int rightCeiling = leftCeiling + _sceneDefinition.RightTiles;
                    int pitX = 4 + _sceneDefinition.TopTiles;
                    int hallSize = 2 + _sceneDefinition.BottomTiles;

                    int leftFloor = leftCeiling + hallSize;
                    int rightFloor = rightCeiling + hallSize;
                    int pitFloor = nameTable.Height - 2;
                    int pitRight = pitX + hallSize;

                    if (x >= pitRight && y >= rightCeiling && y < rightFloor)
                    {
                        nameTable[x, y] = 0;
                    }
                }
                else if(y >= nameTable.Height - _sceneDefinition.RightEdgeFloorTiles - 4
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
            if (!NeedsLeftExit(nameTable))
                return nameTable;

            nameTable.ForEach((x, y, b) =>
            {
                if (_sceneDefinition.ScrollStyle == ScrollStyle.None &&
                    _sceneDefinition.LevelShape == LevelShape.TShape)
                {
                    int leftWall = 2;
                    int rightWall = nameTable.Width - 2;
                    int leftCeiling = 2 + _sceneDefinition.LeftTiles;
                    int rightCeiling = leftCeiling + _sceneDefinition.RightTiles;
                    int pitX = 4 + _sceneDefinition.TopTiles;
                    int hallSize = 2 + _sceneDefinition.BottomTiles;

                    int leftFloor = leftCeiling + hallSize;
                    int rightFloor = rightCeiling + hallSize;
                    int pitFloor = nameTable.Height - 2;
                    int pitRight = pitX + hallSize;

                    if (x < pitX && y >= leftCeiling && y < leftFloor)
                    {
                        nameTable[x, y] = 0;
                    }
                }
                else if (y >= nameTable.Height - _sceneDefinition.LeftEdgeFloorTiles - 4
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

            bool needsExit = true;

            if (_sceneDefinition.ScrollStyle == ScrollStyle.None &&
                    _sceneDefinition.LevelShape == LevelShape.TShape)
            {
                int leftWall = 2;
                int rightWall = nameTable.Width - 2;
                int leftCeiling = 2 + _sceneDefinition.LeftTiles;
                int rightCeiling = leftCeiling + _sceneDefinition.RightTiles;
                int pitX = 4 + _sceneDefinition.TopTiles;
                int hallSize = 2 + _sceneDefinition.BottomTiles;

                int leftFloor = leftCeiling + hallSize;
                int rightFloor = rightCeiling + hallSize;
                int pitFloor = nameTable.Height - 2;
                int pitRight = pitX + hallSize;

                nameTable.ForEach((x, y, b) =>
                {
                    if (x >= pitX && x < pitRight && y > leftFloor)
                    {
                        nameTable[x, y] = 0;
                    }
                });
                return nameTable;
            }


            nameTable.ForEach((x, y, b) =>
            {
                if (x >= xStart
                    && x < xStart + width
                    && y >= nameTable.Height - _sceneDefinition.BottomTiles)
                {
                    if (nameTable[x, y] == 0)
                        needsExit = false;
                }
            });

            if (!needsExit)
                return nameTable;

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

        private NBitPlane AddTopExit(NBitPlane nameTable)
        {
            int xStart = (nameTable.Width / 2) - 2;
            int width = 4;

            bool needsExit = false;

            for(int x = xStart; x <= xStart + width; x++)
            {
                for(int y= 0; y <= _sceneDefinition.TopTiles; y++)
                {
                    if (nameTable[x, y] != 0)
                        needsExit = true;
                }
            }
            if (!needsExit)
                return nameTable;

            nameTable.ForEach((x, y, b) =>
            {

                if (x >= xStart
                    && x < xStart + width
                    && y <= _sceneDefinition.TopTiles)
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

        private NBitPlane AddShapeTiles(NBitPlane nameTable, byte seed)
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
                    LevelShape.Ladder => AddLadderTiles(nameTable),
                    _ => nameTable 
                },
                ScrollStyle.NameTable => _sceneDefinition.LevelShape switch {
                    LevelShape.TwoHorizontalChambers => AddHorizontalChambers(nameTable),
                    LevelShape.TwoVerticalChambers => AddVerticalChambers(nameTable),
                    LevelShape.FourChambers => AddFourChambers(nameTable),
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

        private NBitPlane AddHorizontalChambers(NBitPlane nameTable)
        {
            int midFloorStart = (nameTable.Height / 2) - 2;
            int midFloorEnd = midFloorStart + 4;

            nameTable.ForEach((x, y, b) =>
            {
                if (y >= midFloorStart && y < midFloorEnd)
                    nameTable[x, y] = 1;
            });

            return nameTable;
        }

        private NBitPlane AddVerticalChambers(NBitPlane nameTable)
        {
            int midWallStart = (nameTable.Width / 2) - 2;
            int midWallEnd = midWallStart + 4;

            nameTable.ForEach((x, y, b) =>
            {
                if (x >= midWallStart && x < midWallEnd)
                    nameTable[x, y] = 1;
            });

            return nameTable;
        }

        private NBitPlane AddFourChambers(NBitPlane nameTable)
        {
            int midFloorStart = (nameTable.Height / 2) - 2;
            int midFloorEnd = midFloorStart + 4;

            int midWallStart = (nameTable.Width / 2) - 2;
            int midWallEnd = midWallStart + 2;

            nameTable.ForEach((x, y, b) =>
            {
                if (y >= midFloorStart && y < midFloorEnd)
                    nameTable[x, y] = 1;
                if (x >= midWallStart && x < midWallEnd)
                    nameTable[x, y] = 1;
            });

            return nameTable;
        }

        private NBitPlane AddLadderTiles(NBitPlane nameTable)
        {
            int sectionHeight = 10;
            int stepWidth = 2;

            Point p = new Point(0, _sceneDefinition.TopTiles + sectionHeight);
            while (p.Y < nameTable.Height - _sceneDefinition.BottomTiles)
            {
                for(int i =0; i< stepWidth; i++)
                {
                    nameTable[_sceneDefinition.LeftTiles + i, p.Y] = 1;
                    nameTable[_sceneDefinition.LeftTiles + i, p.Y + 1] = 1;

                    nameTable[nameTable.Width - _sceneDefinition.RightEdgeFloorTiles - i - 1, p.Y] = 1;
                    nameTable[nameTable.Width - _sceneDefinition.RightEdgeFloorTiles - i - 1, p.Y + 1] = 1;
                }

                p.Y += sectionHeight;
            }

            return nameTable;
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
            int leftWall = 2;
            int rightWall = nameTable.Width - 2;
            int leftCeiling = 2 + _sceneDefinition.LeftTiles;
            int rightCeiling = leftCeiling +  _sceneDefinition.RightTiles;
            int pitX = 4 + _sceneDefinition.TopTiles;
            int hallSize = 2 + _sceneDefinition.BottomTiles;

            int leftFloor = leftCeiling + hallSize;
            int rightFloor = rightCeiling + hallSize;
            int pitFloor = nameTable.Height - 2;
            int pitRight = pitX + hallSize;
          
            nameTable.ForEach((x, y, b) =>
            {
                // left hall
                if (x >= leftWall && x < pitRight && y >= leftCeiling && y < leftFloor)
                    nameTable[x, y] = 0;
                // right hall
                else if (x >= pitRight && x < rightWall && y >= rightCeiling && y < rightFloor)
                    nameTable[x, y] = 0;
                // pit
                else if (x >= pitX && x < pitRight && y >= leftCeiling && y < pitFloor)
                    nameTable[x, y] = 0;
                else
                {
                    nameTable[x, y] = 1;
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
                nameTable.Width - stairSize,
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
                        changeY: bigStep ?  2 : 1);

                    AddStairs(nameTable,
                        rightStair,
                        changeX: bigStep ? 2 : 1,
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
              changeY: -2);

            return nameTable;
        }

        private NBitPlane AddStairs(NBitPlane nameTable, Rectangle region, int changeX, int changeY)
        {
            nameTable.ForEach(new Point(region.Left, region.Top), new Point(region.Right, region.Bottom),
                (x, y, b) =>
                {
                    int regionX = x - region.X;
                    int regionY = y - region.Y;
                    int stepNumber = regionX / changeX;

                    int stepHeight = changeY < 0 ? region.Height + ((stepNumber + 1) * changeY)
                                                 : (stepNumber + 1) * changeY;

                    nameTable[x, y] = (byte)((regionY >= stepHeight) ? 1 : 0);
                });

            return nameTable;
        }
        //todo
        private NBitPlane AddStairs2(NBitPlane nameTable, Rectangle region, int changeX, int changeY)
        {
            nameTable.ForEach(new Point(region.Left, region.Top), new Point(region.Right, region.Bottom),
                (x, y, b) =>
                {
                    int regionX = x - region.X;
                    int regionY = y - region.Y;
                    int stepNumber = regionX / changeX;

                    stepNumber--;

                    int stepHeight = changeY < 0 ? region.Height + (stepNumber * changeY)
                                                 : stepNumber * changeY;

                    nameTable[x, y] = (byte)((regionY >= stepHeight) ? 1 : 0);
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

        private NBitPlane AddGroundVariance(NBitPlane nameTable, byte seed)
        {
            var rng = _gameModule.RandomModule;

            int groundMin = _sceneDefinition.LevelTileHeight - _sceneDefinition.BottomTiles;
            int groundMax = _sceneDefinition.LevelTileHeight - 1;

            if (groundMin >= groundMax)
                throw new Exception("Not enough room for variance");

            int sectionBegin = 0;
            int sectionEnd = sectionBegin + GetNextGroundSectionWidth(rng, seed);
            int ground = GetNextGroundHeight(0, rng, seed, groundMin, groundMax);

            for (int x = 0; x < nameTable.Width; x++)
            {
                for(int y = 0; y < nameTable.Height; y++)
                {
                    if (y >= ground)
                        nameTable[x, y] = 1;
                    else if(y >= groundMin)
                        nameTable[x, y] = 0;                   
                }

                if (x == sectionEnd)
                {
                    sectionBegin = x;
                    sectionEnd = sectionBegin + GetNextGroundSectionWidth(rng, (byte)(seed + x));

                    ground = GetNextGroundHeight(ground, rng, (byte)(seed + x + ground), groundMin, groundMax);
                }
            }

            return nameTable;
        }

        private int GetNextGroundSectionWidth(RandomModule rng, byte seed)
        {
            var randomValue = 1 + rng.FixedRandom(seed, 4);

            if (_sceneDefinition.LevelShape == LevelShape.TwoByTwoVariance)
            {
                randomValue = (randomValue / 2) * 2;
                if (randomValue < 2)
                    randomValue = 2;
            }

            return randomValue;            
        }

        private int GetNextGroundHeight(int lastHeight, RandomModule rng, byte seed, int groundMin, int groundMax)
        {
            int newHeight;

            if(lastHeight > 0 && _sceneDefinition.LevelShape == LevelShape.LowVariance)
            {
                newHeight = lastHeight + (rng.FixedRandom(seed, 1) == 1 ? 1 : -1);
                if (newHeight > groundMax)
                    newHeight = groundMax - 1;
                else if (newHeight < groundMin)
                    newHeight = groundMin + 1;

                return newHeight;
            }

            int range = groundMax - groundMin;
            newHeight = groundMin + (rng.FixedRandom(seed,4) % range);

            int tries = 0;
            while(tries < 4 && newHeight == lastHeight)
            {
                tries++;
                newHeight = groundMin + (rng.FixedRandom((byte)(seed+tries), 4) % range);

                if (_sceneDefinition.LevelShape == LevelShape.TwoByTwoVariance)
                {
                    newHeight = (newHeight / 2) * 2;
                    if (newHeight < 0)
                        newHeight = 2;
                }
            }

           

            return newHeight;
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
            EnemyOrBulletSpriteControllerPool<TurretBulletController> turretControllers = null;

            if(_sceneDefinition.IsAutoScroll)
            {
                playerController = new PlayerPlaneController(_gameModule, memoryBuilder);
                bombControllers = new SpriteControllerPool<BombController>(
                   size: 2,
                   _gameModule.SpritesModule,
                   () => new PlaneBombController(_gameModule, playerController, memoryBuilder));

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
                    size: 8,
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
                AssignSpriteControllers(sprite, spritePools, memoryBuilder, playerController, prizeControllers);
            }

            turretControllers = new EnemyOrBulletSpriteControllerPool<TurretBulletController>(4, _gameModule.SpritesModule,
                () => new TurretBulletController(_gameModule, memoryBuilder, SpriteTileIndex.Extra1));

            return new SceneSpriteControllers(_gameModule, playerController,
                bombControllers,
                prizeControllers,
                doorControllers,
                buttonControllers,
                platformControllers,
                explosionControllers,
                turretControllers,
                spritePools);
        }

        private void AssignSpriteControllers(SpriteType spriteType, 
            ICollidableSpriteControllerPool[] spritePools,
            SystemMemoryBuilder memoryBuilder,
            PlayerController playerController,
            SpriteControllerPool<PrizeController> prizeControllers)
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
                    if (_sceneDefinition.SpriteGroup != SpriteGroup.PlaneTakeoff)
                        break;

                    spritePools[enemyIndex] = new EnemyOrBulletSpriteControllerPool<PlaneTakeoffController>(
                         1,
                        _gameModule.SpritesModule,
                        () => new PlaneTakeoffController(_gameModule, memoryBuilder, playerController));
                    break;

                case SpriteType.Rocket:
                    var bulletControllers = new EnemyOrBulletSpriteControllerPool<BossBulletController>(
                      8,
                      _gameModule.SpritesModule,
                      () => new BossBulletController(_gameModule, memoryBuilder, destroyOnCollision:false));

                    spritePools[extraIndex] = bulletControllers;

                    if (_sceneDefinition.IsAutoScroll)
                    {
                        spritePools[enemyIndex] = new EnemyOrBulletSpriteControllerPool<RocketEnemyController>(
                            4,
                            _gameModule.SpritesModule,
                            () => new RocketEnemyController(enemyTileIndex, _gameModule, playerController.WorldSprite, bulletControllers, memoryBuilder));
                    }
                    else
                    {
                        spritePools[enemyIndex] = new EnemyOrBulletSpriteControllerPool<RocketEnemyController2>(
                          2,
                          _gameModule.SpritesModule,
                          () => new RocketEnemyController2(enemyTileIndex, _gameModule, playerController.WorldSprite, bulletControllers, memoryBuilder));

                    }
                    break;

                case SpriteType.Bird:
  
                    spritePools[enemyIndex] = new EnemyOrBulletSpriteControllerPool<BirdEnemyController>(
                    4,
                    _gameModule.SpritesModule,
                    () => new BirdEnemyController(playerController.WorldSprite, _gameModule, memoryBuilder, enemyTileIndex));
                    break;

                case SpriteType.Crocodile:
                    spritePools[enemyIndex] = new EnemyOrBulletSpriteControllerPool<CrocodileController>(
                   2,
                   _gameModule.SpritesModule,
                   () => new CrocodileController(playerController.WorldSprite, enemyTileIndex, _gameModule, memoryBuilder));
                    break;
                case SpriteType.Ogre:
                    spritePools[extraIndex] = new EnemyOrBulletSpriteControllerPool<OgreBulletController>(
                                     3,
                                     _gameModule.SpritesModule,
                                     () => new OgreBulletController( _gameModule, memoryBuilder, extraTileIndex));

                    spritePools[enemyIndex] = new EnemyOrBulletSpriteControllerPool<OgreController>(
                        2,
                        _gameModule.SpritesModule,
                        () => new OgreController(spritePools[extraIndex], enemyTileIndex, _gameModule, memoryBuilder, playerController.WorldSprite));
                    break;
                case SpriteType.Boulder:
                    var boulderBulletControllers = new EnemyOrBulletSpriteControllerPool<BossBulletController>(
                                     4,
                                     _gameModule.SpritesModule,
                                     () => new BossBulletController(_gameModule, memoryBuilder, true));

                    spritePools[extraIndex] = boulderBulletControllers;
                    spritePools[enemyIndex] = new EnemyOrBulletSpriteControllerPool<BoulderEnemyController>(
                        2,
                        _gameModule.SpritesModule,
                        () => new BoulderEnemyController(boulderBulletControllers, enemyTileIndex, _gameModule, memoryBuilder, playerController.WorldSprite));
                    break;
                case SpriteType.Mage:
                    var mageBulletControllers = new EnemyOrBulletSpriteControllerPool<MageBulletController>(
                                     3,
                                     _gameModule.SpritesModule,
                                     () => new MageBulletController(_gameModule, memoryBuilder, extraTileIndex, playerController.WorldSprite));
                   spritePools[extraIndex] = mageBulletControllers;
                   spritePools[enemyIndex] = new EnemyOrBulletSpriteControllerPool<MageController>(
                        2,
                        _gameModule.SpritesModule,
                        () => new MageController(mageBulletControllers, enemyTileIndex, _gameModule, memoryBuilder, playerController.WorldSprite));
                    break;
                case SpriteType.Ufo:
                    var ufoBulletControllers = new EnemyOrBulletSpriteControllerPool<BossBulletController>(
                                     3,
                                     _gameModule.SpritesModule,
                                     () => new BossBulletController(_gameModule, memoryBuilder, true));
                    spritePools[extraIndex] = ufoBulletControllers;
                    spritePools[enemyIndex] = new EnemyOrBulletSpriteControllerPool<UfoController>(
                         8,
                         _gameModule.SpritesModule,
                         () => new UfoController(ufoBulletControllers, enemyTileIndex, _gameModule, memoryBuilder, playerController.WorldSprite));
                    break;
                case SpriteType.Chomp:
                case SpriteType.LevelBoss:

                    AssignBossSpriteControllers(spritePools, memoryBuilder, playerController, prizeControllers);
                    break;
            }
        }

        private void AssignBossSpriteControllers(ICollidableSpriteControllerPool[] spritePools,
            SystemMemoryBuilder memoryBuilder,
            PlayerController playerController,
            SpriteControllerPool<PrizeController> prizeControllers)
        {
            int enemyIndex = spritePools[0] == null ? 0 : 1;
            int extraIndex = spritePools[2] == null ? 2 : 3;

            switch (_gameModule.CurrentLevel)
            {
                case Level.Level1_11_Boss:

                    var bullets = new EnemyOrBulletSpriteControllerPool<BossBulletController>(
                          3,
                          _gameModule.SpritesModule,
                          () => new BossBulletController(_gameModule, memoryBuilder, destroyOnCollision: true));

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
                        () => new BossBulletController(_gameModule, memoryBuilder, destroyOnCollision: true));

                    spritePools[extraIndex] = bossBulletControllers;
                    spritePools[enemyIndex] = new EnemyOrBulletSpriteControllerPool<Level1BossController>(
                          size: 1,
                          spritesModule: _gameModule.SpritesModule,
                          () => new Level1BossController(_gameModule, playerController.WorldSprite, bossBulletControllers, memoryBuilder));
                    break;

                case Level.Level2_2_Fly2:

                    bossBulletControllers = new EnemyOrBulletSpriteControllerPool<BossBulletController>(
                        6,
                        _gameModule.SpritesModule,
                        () => new BossBulletController(_gameModule, memoryBuilder, destroyOnCollision: true));
                    spritePools[extraIndex] = bossBulletControllers;
                    spritePools[enemyIndex] = new EnemyOrBulletSpriteControllerPool<ChompBoss2Controller>(
                            size: 1,
                            spritesModule: _gameModule.SpritesModule,
                            () => new ChompBoss2Controller(playerController, 
                                bossBulletControllers,
                                prizeControllers,
                                _gameModule,
                                memoryBuilder));

                    break;
                case Level.Level2_12_Boss:

                    bossBulletControllers = new EnemyOrBulletSpriteControllerPool<BossBulletController>(
                        6,
                        _gameModule.SpritesModule,
                        () => new BossBulletController(_gameModule, memoryBuilder, destroyOnCollision: true));

                    spritePools[extraIndex] = bossBulletControllers;
                    spritePools[enemyIndex] = new EnemyOrBulletSpriteControllerPool<Level2BossController>(
                         size: 1,
                         spritesModule: _gameModule.SpritesModule,
                         () => new Level2BossController(_gameModule, playerController.WorldSprite, 
                            bossBulletControllers, memoryBuilder));
                    break;
                case Level.Level3_20_Midboss:

                    var bouncingBullets = new EnemyOrBulletSpriteControllerPool<BouncingBossBulletController>(
                          3,
                          _gameModule.SpritesModule,
                          () => new BouncingBossBulletController(_gameModule, memoryBuilder, destroyOnCollision: true));

                    spritePools[extraIndex] = bouncingBullets;
                    spritePools[enemyIndex] = new EnemyOrBulletSpriteControllerPool<ChompBoss3Controller>(
                            size: 1,
                            spritesModule: _gameModule.SpritesModule,
                            () => new ChompBoss3Controller(playerController.WorldSprite, bouncingBullets, _gameModule, memoryBuilder));

                    break;
                case Level.Level3_26_Boss:
                    bossBulletControllers = new EnemyOrBulletSpriteControllerPool<BossBulletController>(
                        6,
                        _gameModule.SpritesModule,
                        () => new BossBulletController(_gameModule, memoryBuilder, destroyOnCollision: true));

                    spritePools[extraIndex] = bossBulletControllers;
                    spritePools[enemyIndex] = new EnemyOrBulletSpriteControllerPool<Level3BossController>(
                         size: 1,
                         spritesModule: _gameModule.SpritesModule,
                         () => new Level3BossController(_gameModule, playerController.WorldSprite,
                            bossBulletControllers, memoryBuilder));
                    break;
                case Level.Level4_31_Midboss:

                    bullets = new EnemyOrBulletSpriteControllerPool<BossBulletController>(
                          10,
                          _gameModule.SpritesModule,
                          () => new BossBulletController(_gameModule, memoryBuilder, destroyOnCollision: true));

                    spritePools[extraIndex] = bullets;
                    spritePools[enemyIndex] = new EnemyOrBulletSpriteControllerPool<ChompBoss4Controller>(
                            size: 1,
                            spritesModule: _gameModule.SpritesModule,
                            () => new ChompBoss4Controller(playerController.WorldSprite, bullets, _gameModule, memoryBuilder));

                    break;
                case Level.Level4_40_Boss:
                    bossBulletControllers = new EnemyOrBulletSpriteControllerPool<BossBulletController>(
                        6,
                        _gameModule.SpritesModule,
                        () => new BossBulletController(_gameModule, memoryBuilder, destroyOnCollision: true));

                    spritePools[extraIndex] = bossBulletControllers;
                    spritePools[enemyIndex] = new EnemyOrBulletSpriteControllerPool<Level4BossController>(
                         size: 1,
                         spritesModule: _gameModule.SpritesModule,
                         () => new Level4BossController(_gameModule, playerController.WorldSprite,
                            bossBulletControllers, memoryBuilder));
                    break;
                case Level.Level5_22_MidBoss:

                    bullets = new EnemyOrBulletSpriteControllerPool<BossBulletController>(
                          10,
                          _gameModule.SpritesModule,
                          () => new BossBulletController(_gameModule, memoryBuilder, destroyOnCollision: true));

                    spritePools[extraIndex] = bullets;
                    spritePools[enemyIndex] = new EnemyOrBulletSpriteControllerPool<ChompBoss5Controller>(
                            size: 1,
                            spritesModule: _gameModule.SpritesModule,
                            () => new ChompBoss5Controller(playerController.WorldSprite, bullets, _gameModule, memoryBuilder));

                    break;
                case Level.Level5_27_Boss:
                    bossBulletControllers = new EnemyOrBulletSpriteControllerPool<BossBulletController>(
                        Level5BossController.MaxBullets,
                        _gameModule.SpritesModule,
                        () => new BossBulletController(_gameModule, memoryBuilder, destroyOnCollision: true));

                    spritePools[extraIndex] = bossBulletControllers;
                    spritePools[enemyIndex] = new EnemyOrBulletSpriteControllerPool<Level5BossController>(
                         size: 1,
                         spritesModule: _gameModule.SpritesModule,
                         () => new Level5BossController(_gameModule, playerController.WorldSprite,
                            bossBulletControllers, memoryBuilder));
                    break;
            }
        }


        public void ApplyLevelAlterations(NBitPlane levelMap)
        {
            DynamicScenePartHeader header = _gameModule.CurrentScenePartHeader;

            for (int i = 0; i < header.PartsCount; i++)
            {
                IScenePart sp = header.GetScenePart(i, _sceneDefinition, _gameModule.Specs);

                if(sp.Type == ScenePartType.Prefab)
                {
                    AddPrefab(sp as PrefabScenePart, levelMap);                   
                    header.MarkActive(i);
                }
                else if(sp.Type == ScenePartType.Turret)
                {
                    AddTurret(sp as TurretScenePart, levelMap);
                }
            }
        }

        public void AddTurrets(NBitPlane levelMap)
        {
            DynamicScenePartHeader header = _gameModule.CurrentScenePartHeader;

            for (int i = 0; i < header.PartsCount; i++)
            {
                IScenePart sp = header.GetScenePart(i, _sceneDefinition, _gameModule.Specs);

                if (sp.Type == ScenePartType.Turret)
                {
                    AddTurret(sp as TurretScenePart, levelMap);
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

        private void AddTurret(TurretScenePart part, NBitPlane levelMap)
        {
            var x = part.X;
            if (part.Direction == Direction.Left)
                x++;

            levelMap[x, part.Y] = (byte)(_spriteTileTable.TurretTile + (int)part.Direction);
        }

        private void AddPrefab(PrefabScenePart part, NBitPlane levelMap)
        {
            levelMap.ForEach((x, y, _) =>
            {
                if (x >= part.X && x < part.XEnd
                    && y >= part.Y && y < part.YEnd)
                {
                    levelMap[x, y] = (byte)(part.Shape == PrefabStyle.Space ? 0 : 1);
                }
            });   
            
            if(part.Shape == PrefabStyle.StairDown)
            {
                int stepSize = 2;
                AddStairs2(levelMap, new Rectangle(part.X, part.Y, part.Width, part.Height),
                    stepSize, stepSize);
            }

            if (part.Shape == PrefabStyle.StairUp)
            {
                int stepSize = 2;
                AddStairs(levelMap, new Rectangle(part.X, part.Y, part.Width, part.Height),
                    stepSize, -stepSize);
            }
        }

        public void SetupVRAMPatternTable(
           NBitPlane masterPatternTable,
           NBitPlane bgPatternTable,
           NBitPlane spritePatternTable,
           SystemMemory memory)
        {
            bgPatternTable.Reset();
            spritePatternTable.Reset();

            var spriteBuilder = new VramBuilder(masterPatternTable, spritePatternTable, _spriteTileTable, memory, _gameModule.Specs);
            var bgBuilder = new VramBuilder(masterPatternTable, bgPatternTable, _spriteTileTable, memory, _gameModule.Specs);

            bgBuilder.AddStatusBarTiles();            
            _sceneDefinition.CreateThemeSetup(_gameModule)
                            .SetupVRAMPatternTable(masterPatternTable, bgPatternTable, memory);
            if(_sceneDefinition.IsAutoScroll)
            {
                //player sprite
                spriteBuilder.AddSprite(SpriteTileIndex.Player, 0, 0, 1, 1);
                spriteBuilder.AddSprite(SpriteTileIndex.Plane, 13, 2, 2, 1);
                spriteBuilder.AddSprite(SpriteTileIndex.Bomb, 4, 1, 1, 1);
            }
            else if (_sceneDefinition.HasSprite(SpriteType.Player))
            {
                spriteBuilder.AddSprite(SpriteTileIndex.Player, 0, 0, 2, 2);
                spriteBuilder.AddSprite(SpriteTileIndex.Bomb, 4, 1, 1, 1);
           
                if(_sceneDefinition.HasSprite(SpriteType.Plane))
                    spriteBuilder.AddSprite(SpriteTileIndex.Plane, 13, 2, 2, 1);                 
            }

            if (_sceneDefinition.HasSprite(SpriteType.LevelBoss) || _sceneDefinition.HasSprite(SpriteType.Chomp))
            {
                spriteBuilder.AddBossSprites(_gameModule.CurrentLevel);

                if(_sceneDefinition.HasSprite(SpriteType.LevelBoss))
                    bgBuilder.AddBossBodyTiles();
            }
            else
            {
                spriteBuilder.AddSprite(SpriteTileIndex.Door, 14, 5, 2, 2);
            }

            if (_sceneDefinition.HasSprite(SpriteType.Lizard))
            {
                spriteBuilder.AddEnemySprite(2, 0, 2, 2);
                spriteBuilder.AddExtraSprite(4, 0, 2, 1);
            }

            if (_sceneDefinition.HasSprite(SpriteType.Bird))
                spriteBuilder.AddEnemySprite(8, 0, 4, 1);

            if (_sceneDefinition.HasSprite(SpriteType.Rocket))
            {
                spriteBuilder.AddEnemySprite(5, 1, 2, 1);
                spriteBuilder.AddExtraSprite(12, 2, 1, 1);
            }

            if (_sceneDefinition.HasSprite(SpriteType.Crocodile))
                spriteBuilder.AddEnemySprite(4, 2, 4, 1);

            if (_sceneDefinition.HasSprite(SpriteType.Ogre))
            {
                spriteBuilder.AddEnemySprite(12, 0, 2, 2);
                spriteBuilder.AddExtraSprite(14, 0, 1, 1);
            }

            if (_sceneDefinition.HasSprite(SpriteType.Boulder))
            {
                spriteBuilder.AddEnemySprite(0, 2, 4, 2);
                spriteBuilder.AddExtraSprite(12, 2, 1, 1);
            }

            if (_sceneDefinition.HasSprite(SpriteType.Mage))
            {
                spriteBuilder.AddEnemySprite(12, 7, 4, 2);
                spriteBuilder.AddExtraSprite(12, 2, 1, 1);
            }

            if (_sceneDefinition.HasSprite(SpriteType.Ufo))
            {
                spriteBuilder.AddEnemySprite(14, 1, 2, 1);
                spriteBuilder.AddExtraSprite(12, 2, 1, 1);
            }


            spriteBuilder.AddExplosionSprite();
            
            if (_sceneDefinition.SpriteGroup == SpriteGroup.Normal || 
                _sceneDefinition.SpriteGroup == SpriteGroup.Boss)
            {
                spriteBuilder.AddSprite(SpriteTileIndex.Platform, 12, 5, 2, 1);
                spriteBuilder.AddSprite(SpriteTileIndex.Button, 11, 6, 2, 1);
            }

            if (_sceneDefinition.HasSprite(SpriteType.Player))
            {
                spriteBuilder.AddSprite(SpriteTileIndex.Prize, 7, 0, 1, 1);
            }

            bgBuilder.AddSprite(SpriteTileIndex.Block, 13, 6, 1, 1);
            bgBuilder.AddSprite(SpriteTileIndex.Coin, 15, 0, 1, 1);

            if (!_sceneDefinition.IsLevelBossScene && !_sceneDefinition.IsMidBossScene)
            {
                bgBuilder.AddSprite(SpriteTileIndex.Turret, 7, 6, 4, 1);
            }
        }
         
    }
}
