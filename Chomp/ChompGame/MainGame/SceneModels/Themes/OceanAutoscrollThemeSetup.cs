﻿using ChompGame.Data;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame.SceneModels.Themes
{
    class OceanAutoscrollThemeSetup : ThemeSetup
    {
        public OceanAutoscrollThemeSetup(ChompGameModule m) : base(m) { }

        public override void BuildBackgroundNameTable(NBitPlane nameTable)
        {
            var bgPos = (byte)_sceneDefinition.GetBackgroundLayerTile(BackgroundPart.Upper, includeStatusBar: false);


            string waterBlock = @"89A089A0008989A0
                                  BBBBBBBBBBBBBBBB
                                  BCBCBCBCBCBCBCBC
                                  CCCCCCCCCCCCCCCC
                                  DDDDDDDDDDDDDDDD
                                  CDCDCDCDCDCDCDCD
                                  DDCCDDCCDDCCDDCC";

            nameTable.SetFromString(0, bgPos+1, 16, waterBlock);
            nameTable.SetFromString(16, bgPos + 1, 16,waterBlock);
            nameTable.SetFromString(32, bgPos + 1,16, waterBlock);
            nameTable.SetFromString(48, bgPos + 1, 16,waterBlock);

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
