using ChompGame.Data;
using ChompGame.MainGame.SceneModels.SmartBackground;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ChompGame.MainGame.SceneModels.Themes
{
    class DesertThemeSetup : ThemeSetup
    {

        public DesertThemeSetup(ChompGameModule m) : base(m) { }

        public override IEnumerable<SmartBackgroundBlock> SmartBackgroundBlocks
        {
            get
            {
                yield return new Pyramid(_sceneDefinition, _gameModule);
            }
        }

        public override void BuildBackgroundNameTable(NBitPlane nameTable)
        {

        }

        public override NBitPlane BuildAttributeTable(NBitPlane attributeTable, NBitPlane nameTable)
        {
            int foreGroundAttributePosition = _sceneDefinition.GetBackgroundLayerTile(BackgroundPart.Lower, false) / _specs.AttributeTableBlockSize;

            var begin = _sceneDefinition.GetBackgroundLayerTile(BackgroundPart.Bottom, false) / 2;
            begin--;

            if (_sceneDefinition.ScrollStyle == ScrollStyle.NameTable)
                begin = 8;
           else if (_sceneDefinition.ScrollStyle == ScrollStyle.None &&
                (_sceneDefinition.LeftTiles > 0 || _sceneDefinition.RightTiles > 0))
            {
                begin = -1;
            }

            attributeTable.ForEach((x, y, b) =>
            {
                attributeTable[x, y] = (byte)((y > begin) ? 1: 0);
            });

            return attributeTable;
        }

        public override void SetupVRAMPatternTable()
        {
            _gameModule.TileCopier.CopyTilesForDesertTheme();
        }
    }
}
