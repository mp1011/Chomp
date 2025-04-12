using ChompGame.Data;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SceneModels.Themes
{
    abstract class BossThemeSetup : ThemeSetup
    {
        public BossThemeSetup(ChompGameModule m) : base(m) { }

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
                    destinationPoint: new Point(0, 1),
                    _specs,
                    memory);          
        }

        public override NBitPlane BuildAttributeTable(NBitPlane attributeTable, NBitPlane nameTable)
        {
            int floorPos = attributeTable.Height - 1;
            attributeTable.ForEach((x, y, b) =>
            {
                if (y == floorPos)
                    attributeTable[x, y] = 1;//
                else
                    attributeTable[x, y] = 0;
            });

            return attributeTable;
        }
    }

    class PlainsBossThemeSetup : BossThemeSetup
    {
        public PlainsBossThemeSetup(ChompGameModule m) : base(m) { }

        protected override ByteRectangleBase FloorTiles => new InMemoryByteRectangle(0, 12, 6, 1);
    }

    class OceanBossThemeSetup : BossThemeSetup
    {
        public OceanBossThemeSetup(ChompGameModule m) : base(m) { }
        protected override ByteRectangleBase FloorTiles => new InMemoryByteRectangle(0, 13, 6, 1);
    }

    class CityBossThemeSetup : BossThemeSetup
    {
        public CityBossThemeSetup(ChompGameModule m) : base(m) { }
        protected override ByteRectangleBase FloorTiles => new InMemoryByteRectangle(0, 14, 6, 1);
    }

    class TechBaseBossThemeSetup : BossThemeSetup
    {
        public TechBaseBossThemeSetup(ChompGameModule m) : base(m) { }
        protected override ByteRectangleBase FloorTiles => new InMemoryByteRectangle(0, 12, 6, 1);
    }

    class FinalBossThemeSetup : BossThemeSetup
    {
        public FinalBossThemeSetup(ChompGameModule m) : base(m) { }
        protected override ByteRectangleBase FloorTiles => new InMemoryByteRectangle(2, 15, 6, 1);
    }
}
