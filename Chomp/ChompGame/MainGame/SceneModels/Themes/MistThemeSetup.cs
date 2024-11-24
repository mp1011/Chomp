using ChompGame.Data;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SceneModels.Themes
{
    class MistThemeSetup : ThemeSetup
    {
        public MistThemeSetup(ChompGameModule gameModule) : base(gameModule)
        {
        }

        public override void BuildBackgroundNameTable(NBitPlane nameTable)
        {
             
            nameTable.ForEach((x, y, b) =>
            {
                if (y >= _sceneDefinition.GetBackgroundLayerTile(BackgroundPart.Upper, false) &&
                    y < _sceneDefinition.GetBackgroundLayerTile(BackgroundPart.Lower, false))

                    nameTable[x, y] = (byte)(1 + _gameModule.RandomModule.Generate(1));
            });
        }

     
        public override void SetupVRAMPatternTable(NBitPlane masterPatternTable, NBitPlane vramPatternTable, SystemMemory memory)
        {
            //mist
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(15, 2, 1, 1),
                destinationPoint: new Point(1, 0),
                _specs,
                memory);

            //mist
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(15, 4, 1, 1),
                destinationPoint: new Point(2, 0),
                _specs,
                memory);

            //solid
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(5, 7, 1, 1),
                destinationPoint: new Point(6, 0),
                _specs,
                memory);

            //bg
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 12, 8, 1),
                destinationPoint: new Point(0, 1),
                _specs,
                memory);
        }
    }
}
