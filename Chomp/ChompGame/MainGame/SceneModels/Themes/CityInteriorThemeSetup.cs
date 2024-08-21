using ChompGame.Data;
using ChompGame.MainGame.SceneModels.SmartBackground;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ChompGame.MainGame.SceneModels.Themes
{
    class CityInteriorThemeSetup : ThemeSetup
    {
        public CityInteriorThemeSetup(ChompGameModule m) : base(m) { }

        public override void BuildBackgroundNameTable(NBitPlane nameTable)
        {
           
        }

        public override void SetupVRAMPatternTable(NBitPlane masterPatternTable, NBitPlane vramPatternTable, SystemMemory memory)
        {
            //bg buildings
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 7, 8, 1),
                destinationPoint: new Point(0, 3),
                _specs,
                memory);

            // fg buildings
            masterPatternTable.CopyTilesTo(
               destination: vramPatternTable,
               source: new InMemoryByteRectangle(0, 8, 8, 1),
               destinationPoint: new Point(0, 4),
               _specs,
               memory);

            masterPatternTable.CopyTilesTo(
              destination: vramPatternTable,
              source: new InMemoryByteRectangle(8, 8, 4, 1),
              destinationPoint: new Point(1, 0),
              _specs,
              memory);

            //fg
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 14, 8, 1),
                destinationPoint: new Point(0, 1),
                _specs,
                memory);
        }
    }
}
