using ChompGame.Data;

namespace ChompGame.MainGame.SceneModels
{
    class LevelNameTableBuilder
    {
        private NBitPlane _nameTable;
        private SceneDefinition _sceneDefinition;

        public LevelNameTableBuilder(NBitPlane nameTable, SceneDefinition sceneDefinition)
        {
            _nameTable = nameTable;
            _sceneDefinition = sceneDefinition;
        }

        public void BuildNameTable()
        {
            for(int i = 0; i < _sceneDefinition.TileRegionCount; i++)
            {
                TileRegionMap map = _sceneDefinition.GetTileRegion(i);
                Apply(map);
            }
        }

        private void Apply(TileRegionMap map)
        {
            for(int destY = map.Destination.Y; destY < map.Destination.Bottom; destY++)
            {
                for(int destX = map.Destination.X; destX < map.Destination.Right; destX++)
                {
                    int sourceX = (destX - map.Destination.X) % map.Source.Width;
                    int sourceY = (destY - map.Destination.Y) % map.Source.Height;

                    _nameTable[destX, destY] = _sceneDefinition.TileMaster[sourceX, sourceY];
                }
            }
        }
    }
}
