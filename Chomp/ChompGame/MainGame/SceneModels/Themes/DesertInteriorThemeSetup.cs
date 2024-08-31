using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels.SmartBackground;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ChompGame.MainGame.SceneModels.Themes
{
    class DesertInteriorThemeSetup : ThemeSetup
    {
        private const int Torch = 1;
        private const int BackTile1 = 2;
        private const int BackTile2 = 3;

        public DesertInteriorThemeSetup(ChompGameModule m) : base(m) 
        {
        }


        public override void BuildBackgroundNameTable(NBitPlane nameTable)
        {
            switch(_sceneDefinition.HorizontalScrollStyle)
            {
                case HorizontalScrollStyle.Interior:
                    nameTable.ForEach((x, y, b) =>
                    {
                        if (nameTable[x, y] != 0)
                            return;

                        nameTable[x, y] = (byte)((x % 2) == (y % 2) ? BackTile1 : BackTile2);
                    });
                    return;
                default:
                    nameTable.ForEach((x, y, b) =>
                    {
                        if (nameTable[x, y] != 0)
                            return;

                        if ((y % 6) != 0)
                            return;

                        if ((y % 12) != 0)
                        {
                            if ((x % 5) != 0)
                                return;
                        }
                        else
                        {
                            if (((x + 2) % 5) != 0)
                                return;
                        }

                        nameTable[x, y] = Torch;
                    });
                    return;
            }
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
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(8, 14, 2, 1),
                destinationPoint: new Point(0, 1),
                _specs,
                memory);

            masterPatternTable.CopyTilesTo(
                 destination: vramPatternTable,
                 source: new InMemoryByteRectangle(2, 12, 6, 1),
                 destinationPoint: new Point(2, 1),
                 _specs,
                 memory);


            masterPatternTable.CopyTilesTo(
                 destination: vramPatternTable,
                 source: new InMemoryByteRectangle(10, 14, 3, 1),
                 destinationPoint: new Point(1, 0),
                 _specs,
                 memory);

        }
    }
}
