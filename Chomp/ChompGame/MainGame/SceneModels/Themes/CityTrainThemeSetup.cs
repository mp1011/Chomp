using ChompGame.Data;
using ChompGame.MainGame.SceneModels.SmartBackground;
using System.Collections.Generic;

namespace ChompGame.MainGame.SceneModels.Themes
{
    class CityTrainThemeSetup : ThemeSetup
    {
        public CityTrainThemeSetup(ChompGameModule m) : base(m) { }

        public override void BuildBackgroundNameTable(NBitPlane nameTable)
        {
            var farCityPos = (byte)_sceneDefinition.GetBackgroundLayerTile(BackgroundPart.Upper, includeStatusBar: false);
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

        public override void SetupVRAMPatternTable()
        {
            _gameModule.TileCopier.CopyTilesForCityTrainTheme();
        }
    }
}
