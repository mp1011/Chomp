using ChompGame.Data;
using ChompGame.GameSystem;
using System;

namespace ChompGame.MainGame.SceneModels
{
    class LevelNameTableBuilder
    {
        private Specs _specs;
        private SceneDefinition _sceneDefinition;
        private TileModule _tileModule;

        public LevelNameTableBuilder(SceneDefinition sceneDefinition, TileModule tileModule, Specs specs)
        {
            _sceneDefinition = sceneDefinition;
            _specs = specs;
            _tileModule = tileModule;
        }

        private void FillStatusBarTopLine()
        {
            _tileModule.NameTable
                .SetFromString(@"01277734562777777");            
        }

        private int GetForegroundWidth()
        {
            switch(_sceneDefinition.Shape)
            {
                case LevelShape.Horizontal:
                    return (_sceneDefinition.MapSize + 1) * _specs.NameTableWidth;
                default:
                    throw new NotImplementedException();
            }
        }

        private int GetForegroundHeight()
        {
            switch (_sceneDefinition.Shape)
            {
                case LevelShape.Horizontal:
                    return (_sceneDefinition.GroundHigh - _sceneDefinition.GroundLow) + 1;
                default:
                    throw new NotImplementedException();
            }
        }

        public void BuildBackgroundNametable()
        {
            FillStatusBarTopLine();

            //mountain layer 1
            _tileModule.NameTable.SetFromString(0, 7,
                @"00000500000005000000050000000500
                        34121625341216253412162534121625");

            //mountain layer 2
            _tileModule.NameTable.SetFromString(0, 9,
              @"00000500001200000050000120000000
                      12341623416621234162341662123434");
        }
        public NBitPlane BuildNameTable_Test(SystemMemory memory)
        {
            NBitPlane nameTable = NBitPlane.Create(memory.GetAddress(AddressLabels.FreeRAM), memory, _specs.NameTableBitPlanes,
                 GetForegroundWidth(), GetForegroundHeight());

            int y = 3;
            int i = 12;
            for (int col = 0; col < nameTable.Width; col++)
            {
                i--;
                if(i==0)
                {
                    i = 12;
                    y--;
                    if (y <= 0)
                        y = 0;
                }
                nameTable[col, y] = (byte)_sceneDefinition.BlockTile;
            }


            return nameTable;
        }

        public NBitPlane BuildNameTable(SystemMemory memory)
        {
            NBitPlane nameTable = NBitPlane.Create(memory.GetAddress(AddressLabels.FreeRAM), memory, _specs.NameTableBitPlanes,
                GetForegroundWidth(), GetForegroundHeight());

            var rnd = new Random(_sceneDefinition.Address);
                    
            int groundPosition = rnd.Next(nameTable.Height);
            int tilesUntilNextChange = GetTilesUntilNextChange(rnd);

            bool groundStart = true;

            for (int col = 0; col < nameTable.Width; col++)
            {                
                if (tilesUntilNextChange == 0)
                {
                    int lastGroundPosition = groundPosition;

                    var change = rnd.Next(1, 4);
                    if (rnd.NextDouble() < 0.5)
                        groundPosition -= change;
                    else
                        groundPosition += change;

                    if (groundPosition >= nameTable.Height)
                    {
                        int extra = groundPosition - nameTable.Height;

                        groundPosition = nameTable.Height-1;
                        
                        if(groundPosition == lastGroundPosition)
                            groundPosition = nameTable.Height - extra;

                        if (groundPosition < 0)
                            groundPosition = 0;
                    }
                    else if (groundPosition < 0)
                    {
                        int extra = _sceneDefinition.GroundLow - groundPosition;

                        groundPosition = _sceneDefinition.GroundLow;

                        if(groundPosition == lastGroundPosition)
                            groundPosition = _sceneDefinition.GroundLow + extra;

                        if (groundPosition >= nameTable.Height)
                            groundPosition = nameTable.Height - 1;
                    }

                    tilesUntilNextChange = GetTilesUntilNextChange(rnd);
                    groundStart = true;
                }

                for (int row = groundPosition; row < nameTable.Height; row++)
                {
                    int tile = _sceneDefinition.BlockTile;

                    if (groundStart)
                    {
                        if (row == groundPosition)
                        {
                            tile = _sceneDefinition.GroundLeftCorner;
                        }
                        else
                        {
                            tile = _sceneDefinition.GetLeftSideTile(row);
                        }
                    }
                    else if (tilesUntilNextChange == 1)
                    {
                        if (row == groundPosition)
                        {
                            tile =  _sceneDefinition.GroundRightCorner;
                        }
                        else
                        {
                            tile = _sceneDefinition.GetRightSideTile(row);
                        }
                    }
                    else
                    {
                        if (row == groundPosition)
                        {
                            tile = _sceneDefinition.GetGroundTopTile(col);
                        }
                        else
                        {
                            tile = _sceneDefinition.GetGroundFillTile(row, col);
                        }
                    }

                    nameTable[col, row] = (byte)tile;
                }

                groundStart = false;

                tilesUntilNextChange--;
            }

            return nameTable;
        }

        private int GetTilesUntilNextChange(Random rng)
        {
            switch (_sceneDefinition.GroundVariation)
            {
                case 0: return 2 + _sceneDefinition.GroundFillTileCount * rng.Next(6, 12);
                case 1: return 2 + _sceneDefinition.GroundFillTileCount * rng.Next(3, 8);
                case 2: return 2 + _sceneDefinition.GroundFillTileCount * rng.Next(2, 4);
                default: return 2 + _sceneDefinition.GroundFillTileCount * rng.Next(1, 2);
            }
        }
    }
}
