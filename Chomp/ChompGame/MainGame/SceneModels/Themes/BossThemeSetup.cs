using ChompGame.Data;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SceneModels.Themes
{
    abstract class BossThemeSetup : ThemeSetup
    {
        public override void BuildBackgroundNameTable(NBitPlane nameTable)
        {
        }

        protected abstract ByteRectangleBase FloorTiles { get; }

        public override void SetupVRAMPatternTable(
         NBitPlane masterPatternTable,
         NBitPlane vramPatternTable,
         SystemMemory memory)
        {          
            masterPatternTable.CopyTilesTo(
                    destination: vramPatternTable,
                    source: FloorTiles,
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

    class PlainsBossThemeSetup : BossThemeSetup
    {
        protected override ByteRectangleBase FloorTiles => new InMemoryByteRectangle(0, 12, 6, 1);
    }

    class OceanBossThemeSetup : BossThemeSetup
    {
        protected override ByteRectangleBase FloorTiles => new InMemoryByteRectangle(0, 13, 6, 1);
    }
}
