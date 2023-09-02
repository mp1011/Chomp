using ChompGame.Data;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SceneModels.Themes
{
    class PlainsBossThemeSetup : ThemeSetup
    {
        public override void BuildBackgroundNameTable(NBitPlane nameTable)
        {

        }

        public override void SetupVRAMPatternTable(
         NBitPlane masterPatternTable,
         NBitPlane vramPatternTable,
         SystemMemory memory)
        {          
            masterPatternTable.CopyTilesTo(
                    destination: vramPatternTable,
                    source: new InMemoryByteRectangle(0, 12, 6, 1),
                    destinationPoint: new Point(0, 3),
                    _specs,
                    memory);          
        }

        public override NBitPlane BuildAttributeTable(NBitPlane attributeTable, NBitPlane nameTable)
        {
            int floorPos = attributeTable.Height - 1;
            attributeTable.ForEach((x, y, b) =>
            {
                if (y == floorPos)
                    attributeTable[x, y] = 1;
                else
                    attributeTable[x, y] = 0;
            });

            return attributeTable;
        }
    }
}
