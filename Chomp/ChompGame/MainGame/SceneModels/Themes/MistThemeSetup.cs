using ChompGame.Data;
using ChompGame.MainGame.SceneModels.SmartBackground;
using System.Collections.Generic;

namespace ChompGame.MainGame.SceneModels.Themes
{
    class MistThemeSetup : ThemeSetup
    {
        public MistThemeSetup(ChompGameModule gameModule) : base(gameModule)
        {
        }

        public override IEnumerable<SmartBackgroundBlock> SmartBackgroundBlocks
        {
            get
            {
                yield return new Bridge(_sceneDefinition, _gameModule);
            }
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

        public override void SetupVRAMPatternTable()
        {
            _gameModule.TileCopier.CopyTilesForMistTheme();
        }
    }
}
