﻿using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.MainGame.SceneModels.SmartBackground;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ChompGame.MainGame.SceneModels.Themes
{
    class DesertRainThemeSetup : ThemeSetup
    {
        public DesertRainThemeSetup(ChompGameModule m) : base(m) { }

        public override void BuildBackgroundNameTable(NBitPlane nameTable)
        {
            nameTable.ForEach((x, y, b) =>
            {
                if (nameTable[x, y] != 0)
                    return;

                nameTable[x, y] = (byte)(1 + (y % 3));
            });
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
            // sand
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 13, 8, 1),
                destinationPoint: new Point(0, 1),
                _specs,
                memory);

            // rain
            masterPatternTable.CopyTilesTo(
              destination: vramPatternTable,
              source: new InMemoryByteRectangle(8, 15, 4, 1),
              destinationPoint: new Point(1, 0),
              _specs,
              memory);

        }
    }
}
