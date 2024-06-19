using ChompGame.Data;
using ChompGame.MainGame.SceneModels.SmartBackground;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ChompGame.MainGame.SceneModels.Themes
{
    class CityTrainThemeSetup : ThemeSetup
    {
        public override void BuildBackgroundNameTable(NBitPlane nameTable)
        {
            var farCityPos = (byte)_sceneDefinition.GetBackgroundLayerTile(BackgroundLayer.Back1, includeStatusBar: false);
            string farCityRow = "EFEEEFFF";
            string nearCityRow1 = "89ABC8B9";
            string nearCityRow2 = "DDDDDDDD";

            nameTable.SetFromString(0, farCityPos + 1, 16,
                $@"{farCityRow}{farCityRow}{farCityRow}{farCityRow}
                         {nearCityRow1}{nearCityRow1}{nearCityRow1}{nearCityRow1}
                         {nearCityRow2}{nearCityRow2}{nearCityRow2}{nearCityRow2}",

                shouldReplace: b => b == 0);
        }

        public override IEnumerable<SmartBackgroundBlock> SmartBackgroundBlocks
        {
            get
            {
                yield return new TrainTracks(_sceneDefinition);
                yield return new TrainCar(_sceneDefinition);
            }
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

            //train car
            masterPatternTable.CopyTilesTo(
               destination: vramPatternTable,
               source: new InMemoryByteRectangle(8, 7, 4, 1),
               destinationPoint: new Point(0, 1),
               _specs,
               memory);

            masterPatternTable.CopyTilesTo(
               destination: vramPatternTable,
               source: new InMemoryByteRectangle(8, 12, 3, 1),
               destinationPoint: new Point(4, 1),
               _specs,
               memory);

            masterPatternTable.CopyTilesTo(
               destination: vramPatternTable,
               source: new InMemoryByteRectangle(13, 12, 3, 1),
               destinationPoint: new Point(0, 2),
               _specs,
               memory);

            masterPatternTable.CopyTilesTo(
               destination: vramPatternTable,
               source: new InMemoryByteRectangle(0, 14, 2, 1),
               destinationPoint: new Point(1, 0),
               _specs,
               memory);

            masterPatternTable.CopyTilesTo(
              destination: vramPatternTable,
              source: new InMemoryByteRectangle(13, 10, 2, 1),
              destinationPoint: new Point(3, 0),
              _specs,
              memory);
        }
    }
}
