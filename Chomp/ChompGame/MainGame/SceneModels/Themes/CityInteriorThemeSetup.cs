using ChompGame.Data;
using ChompGame.MainGame.SceneModels.SmartBackground;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ChompGame.MainGame.SceneModels.Themes
{
    class CityInteriorThemeSetup : ThemeSetup
    {
        public CityInteriorThemeSetup(ChompGameModule m) : base(m) { }

        public override void BuildBackgroundNameTable(NBitPlane nameTable)
        {

        }

        public override void SetupVRAMPatternTable()
        {
            _gameModule.TileCopier.CopyTilesForCityInterior();
        }
    }

    class CityInteriorWindowsThemeSetup : CityInteriorThemeSetup
    {
        public CityInteriorWindowsThemeSetup(ChompGameModule m) : base(m) { }


        public override IEnumerable<SmartBackgroundBlock> SmartBackgroundBlocks
        {
            get
            {
                yield return new BuildingWindow(_sceneDefinition, _gameModule);
            }
        }
    }
}
