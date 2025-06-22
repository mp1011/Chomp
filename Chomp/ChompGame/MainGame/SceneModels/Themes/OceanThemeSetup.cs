using ChompGame.Data;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame.SceneModels.Themes
{
    class OceanThemeSetup : ThemeSetup
    {
        public OceanThemeSetup(ChompGameModule m) : base(m) { }

        public override void BuildBackgroundNameTable(NBitPlane nameTable)
        {
            if (_sceneDefinition.ScrollStyle == ScrollStyle.NameTable)
                return;

            var bgPos = (byte)_sceneDefinition.GetBackgroundLayerTile(BackgroundPart.Upper, includeStatusBar: false);


            string waterBlock = @"1230123000121230
                                  4444444444444444
                                  4545454545454545
                                  0000000000000000
                                  0000000000000000
                                  0000000000000000
                                  0000000000000000";

            nameTable.SetFromString(0, bgPos + 1, 0, waterBlock, shouldReplace: b => b == 0);
            nameTable.SetFromString(16, bgPos + 1, 0, waterBlock, shouldReplace: b => b == 0);
            nameTable.SetFromString(48, bgPos + 1, 0, waterBlock, shouldReplace: b => b == 0);
        }

        public override NBitPlane BuildAttributeTable(NBitPlane attributeTable, NBitPlane nameTable)
        {
            attributeTable.ForEach((x, y, b) =>
            {
                bool isSolid = nameTable[x * 2, y * 2] != 0
                    || nameTable[(x * 2) + 1, (y * 2) + 1] != 0;

                if (isSolid || y > 3)
                    attributeTable[x, y] = 1;
                else
                    attributeTable[x, y] = 0;

            });
            return attributeTable;
        }

        public override void SetupVRAMPatternTable(NBitPlane masterPatternTable, NBitPlane vramPatternTable, SystemMemory memory)
        {
            //tile row 1
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(2, 5, 3, 1),
                destinationPoint: new Point(1, 0),
                _specs,
                memory);

            masterPatternTable.CopyTilesTo(
              destination: vramPatternTable,
              source: new InMemoryByteRectangle(0, 6, 4, 1),
              destinationPoint: new Point(4, 0),
              _specs,
              memory);

            //tile row 2
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 13, 8, 1),
                destinationPoint: new Point(0, 1),
                _specs,
                memory);
        }
    }
}
