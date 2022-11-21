using ChompGame.Data;
using ChompGame.GameSystem;
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

        public void BuildNameTable()
        {
            var rnd = new Random(_sceneDefinition.Address);
            int tiles = (_sceneDefinition.MapSize + 1) * _specs.NameTableWidth;

            //todo, work with scrolling
            tiles = _specs.NameTableWidth;

            int groundPosition = rnd.Next(_sceneDefinition.GroundLow, _sceneDefinition.GroundHigh + 1);
            int tilesUntilNextChange = GetTilesUntilNextChange(rnd);

            for (int col = 0; col < tiles; col++)
            {
                if (tilesUntilNextChange == 0)
                {
                    var change = rnd.Next(1, 4);
                    if (rnd.NextDouble() < 0.5)
                        groundPosition -= change;
                    else
                        groundPosition += change;


                    if (groundPosition > _sceneDefinition.GroundHigh)
                    {
                        int extra = groundPosition - _sceneDefinition.GroundHigh;
                        groundPosition = _sceneDefinition.GroundHigh - extra;
                        if (groundPosition < _sceneDefinition.GroundLow)
                            groundPosition = _sceneDefinition.GroundLow;
                    }
                    else if (groundPosition < _sceneDefinition.GroundLow)
                    {
                        int extra = _sceneDefinition.GroundLow - groundPosition;
                        groundPosition = _sceneDefinition.GroundLow + extra;

                        groundPosition = _sceneDefinition.GroundHigh - extra;
                        if (groundPosition > _sceneDefinition.GroundHigh)
                            groundPosition = _sceneDefinition.GroundHigh;
                    }

                    tilesUntilNextChange = GetTilesUntilNextChange(rnd);
                }

                for (int row = groundPosition; row < _specs.NameTableHeight; row++)
                {
                    _nameTable[col, row] = 8;
                }

                tilesUntilNextChange--;
            }

        }

        private int GetTilesUntilNextChange(Random rng)
        {
            switch (_sceneDefinition.GroundVariation)
            {
                case 0: return rng.Next(6, 12);
                case 1: return rng.Next(3, 8);
                case 2: return rng.Next(3, 6);
                default: return rng.Next(1, 2);
            }
        }
    }
}
