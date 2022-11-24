using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using System;

namespace ChompGame.MainGame.SceneModels
{
    class LevelNameTableBuilder
    {
        private Specs _specs;
        private NBitPlane _nameTable;
        private SceneDefinition _sceneDefinition;

        public LevelNameTableBuilder(NBitPlane nameTable, SceneDefinition sceneDefinition, Specs specs)
        {
            _nameTable = nameTable;
            _sceneDefinition = sceneDefinition;
            _specs = specs;
        }

        private void FillStatusBarTopLine()
        {
            //fill status bar
            for (int x = 0; x < _specs.NameTableWidth; x++)
            {
                _nameTable[x, 0] = 7;
            }

            _nameTable[0, 0] = 0;
            _nameTable[1, 0] = 1;
            _nameTable[2, 0] = 2;

            _nameTable[7, 0] = 3;
            _nameTable[8, 0] = 4;

            _nameTable[9, 0] = 5;

            _nameTable[10, 0] = 6;
            _nameTable[11, 0] = 2;
        }

        public void BuildNameTable(int startColumn)
        {

            FillStatusBarTopLine();

            //32
            _nameTable.SetFromString(0, 7,
                @"00000500001200000050000120000000
                        12341623416621234162341662123434");

            _nameTable.SetFromString(0, 9,
              @"00000500001200000050000120000000
                        12341623416621234162341662123434");


            var rnd = new Random(_sceneDefinition.Address + startColumn);
            int tiles = (_sceneDefinition.MapSize + 1) * _specs.NameTableWidth;

            //todo, work with scrolling
            tiles = _specs.NameTableWidth;

            int groundPosition = rnd.Next(_sceneDefinition.GroundLow, _sceneDefinition.GroundHigh + 1);
            int tilesUntilNextChange = GetTilesUntilNextChange(rnd);

            bool groundStart = true;

            for (int col = startColumn; col < startColumn + tiles; col++)
            {
                rnd = new Random(_sceneDefinition.Address + startColumn);

                if (tilesUntilNextChange == 0)
                {
                    int lastGroundPosition = groundPosition;

                    var change = rnd.Next(1, 4);
                    if (rnd.NextDouble() < 0.5)
                        groundPosition -= change;
                    else
                        groundPosition += change;

                    if (groundPosition > _sceneDefinition.GroundHigh)
                    {
                        int extra = groundPosition - _sceneDefinition.GroundHigh;

                        groundPosition = _sceneDefinition.GroundHigh;
                        
                        if(groundPosition == lastGroundPosition)
                            groundPosition = _sceneDefinition.GroundHigh - extra;

                        if (groundPosition < _sceneDefinition.GroundLow)
                            groundPosition = _sceneDefinition.GroundLow;
                    }
                    else if (groundPosition < _sceneDefinition.GroundLow)
                    {
                        int extra = _sceneDefinition.GroundLow - groundPosition;

                        groundPosition = _sceneDefinition.GroundLow;

                        if(groundPosition == lastGroundPosition)
                            groundPosition = _sceneDefinition.GroundLow + extra;

                        if (groundPosition > _sceneDefinition.GroundHigh)
                            groundPosition = _sceneDefinition.GroundHigh;
                    }

                    tilesUntilNextChange = GetTilesUntilNextChange(rnd);
                    groundStart = true;
                }

                for (int row = groundPosition; row < _specs.NameTableHeight; row++)
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

                    _nameTable[col, row] = (byte)tile;
                }

                groundStart = false;

                tilesUntilNextChange--;
            }

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
