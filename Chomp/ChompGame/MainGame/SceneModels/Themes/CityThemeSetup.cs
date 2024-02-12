using ChompGame.Data;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SceneModels.Themes
{
    class CityThemeSetup : ThemeSetup
    {

        public override void BuildBackgroundNameTable(NBitPlane nameTable)
        {
            var farCityPos = (byte)_sceneDefinition.GetBackgroundLayerTile(BackgroundLayer.Back1, includeStatusBar: false);
            string farCityRow = "EFEEEFFF";
            string nearCityRow1 = "89ABC8B9";
            string nearCityRow2 = "DDDDDDDD";

            nameTable.SetFromString(0, farCityPos+1, 16,
                $@"{farCityRow}{farCityRow}{farCityRow}{farCityRow}
                         {nearCityRow1}{nearCityRow1}{nearCityRow1}{nearCityRow1}
                         {nearCityRow2}{nearCityRow2}{nearCityRow2}{nearCityRow2}",

                shouldReplace: b => b == 0);
        }

        public override void SetupVRAMPatternTable(NBitPlane masterPatternTable, NBitPlane vramPatternTable, SystemMemory memory)
        {
            //bg buildings
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 7, 8, 1),
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
