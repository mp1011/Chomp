using ChompGame.Data;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SceneModels.Themes
{
    class ForestThemeSetup : ThemeSetup
    {
        public ForestThemeSetup(ChompGameModule gameModule) : base(gameModule)
        {
        }

        public override void BuildBackgroundNameTable(NBitPlane nameTable)
        {
            string layer1Row1 = "66661266";
            string layer1Row2 = "34450035";

            //trees layer 1
            nameTable.SetFromString(0, 2, 0,
                $@"{layer1Row1}{layer1Row1}{layer1Row1}{layer1Row1}
                         {layer1Row2}{layer1Row2}{layer1Row2}{layer1Row2}",
                shouldReplace: b => b == 0);

            string layer2Row1 = "6666666612666666";
            string layer2Row2 = "3453444500345345";
            //trees layer 2
            nameTable.SetFromString(0, 0, 0,
              $@"{layer2Row1}{layer2Row1}
                       {layer2Row2}{layer2Row2}",
                shouldReplace: b => b == 0);

            //nameTable.ForEach((x, y, b) =>
            //{
            //    if (x == 5 && y >= 4 && y <= 10)
            //        nameTable[x, y] = 7;
            //});
        }

        public override NBitPlane BuildAttributeTable(NBitPlane attributeTable, NBitPlane nameTable)
        {
            attributeTable.ForEach((x, y, b) =>
            {
                bool isSolid = nameTable[x * 2, y * 2] != 0
                    || nameTable[(x * 2) + 1, (y * 2) + 1] != 0
                    || nameTable[(x * 2), (y * 2) + 1] != 0
                     || nameTable[(x * 2) + 1, (y * 2)] != 0;


                if (isSolid)
                    attributeTable[x, y] = 1;
                else
                    attributeTable[x, y] = 0;

            });

            return attributeTable;
        }

        public override void SetupVRAMPatternTable(NBitPlane masterPatternTable, NBitPlane vramPatternTable, SystemMemory memory)
        {
            //trees1
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(9, 8, 2, 1),
                destinationPoint: new Point(1, 0),
                _specs,
                memory);

            //trees2
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(11, 15, 3, 1),
                destinationPoint: new Point(3, 0),
                _specs,
                memory);

            //solid
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(5, 7, 1, 1),
                destinationPoint: new Point(6, 0),
                _specs,
                memory);

            //trunk
            masterPatternTable.CopyTilesTo(
               destination: vramPatternTable,
               source: new InMemoryByteRectangle(6, 12, 1, 1),
               destinationPoint: new Point(7, 0),
               _specs,
               memory);

            //bg
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 12, 8, 1),
                destinationPoint: new Point(0, 1),
                _specs,
                memory);
        }
    }
}
