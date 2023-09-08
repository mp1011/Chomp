using ChompGame.Data;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame.SceneModels.Themes
{
    class OceanThemeSetup : ThemeSetup
    {
        public override void BuildBackgroundNameTable(NBitPlane nameTable)
        {
            var bgPos = (byte)_sceneDefinition.GetBackgroundLayerTile(BackgroundLayer.Back1, includeStatusBar: false);


            string waterBlock = @"89A089A0008989A0
                                  BBBBBBBBBBBBBBBB
                                  BCBCBCBCBCBCBCBC
                                  0000000000000000
                                  0000000000000000
                                  0000000000000000
                                  0000000000000000";

            nameTable.SetFromString(0, bgPos+1, 0, waterBlock, shouldReplace: b => b == 0);
            nameTable.SetFromString(16, bgPos + 1, 0, waterBlock, shouldReplace: b => b == 0);
            nameTable.SetFromString(48, bgPos + 1, 0, waterBlock, shouldReplace: b => b == 0);
        }

        public override NBitPlane BuildAttributeTable(NBitPlane attributeTable, NBitPlane nameTable)
        {
            attributeTable.ForEach((x, y, b) =>
            {
                bool isSolid = nameTable[x * 2, y * 2] != 0
                    || nameTable[(x * 2) + 1, (y * 2) + 1] != 0;

                if (isSolid)
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
                destinationPoint: new Point(0, 3),
                _specs,
                memory);

            masterPatternTable.CopyTilesTo(
              destination: vramPatternTable,
              source: new InMemoryByteRectangle(0, 6, 4, 1),
              destinationPoint: new Point(3, 3),
              _specs,
              memory);

            //tile row 2
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 13, 8, 1),
                destinationPoint: new Point(0, 4),
                _specs,
                memory);
        }
    }
}
