using ChompGame.Data;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SceneModels.Themes
{
    class CityThemeSetup : ThemeSetup
    {

        public override void BuildBackgroundNameTable(NBitPlane nameTable)
        {

        }

        public override void SetupVRAMPatternTable(NBitPlane masterPatternTable, NBitPlane vramPatternTable, SystemMemory memory)
        {
            //bg buildings
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 7, 6, 1),
                destinationPoint: new Point(0, 3),
                _specs,
                memory);

            //fg
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 14, 8, 1),
                destinationPoint: new Point(0, 4),
                _specs,
                memory);
        }
    }
}
