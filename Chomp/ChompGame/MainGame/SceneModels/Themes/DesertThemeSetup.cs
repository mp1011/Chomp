using ChompGame.Data;
using ChompGame.MainGame.SceneModels.SmartBackground;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ChompGame.MainGame.SceneModels.Themes
{
    class DesertThemeSetup : ThemeSetup
    {


        public DesertThemeSetup()
        {
        }

        public override IEnumerable<SmartBackgroundBlock> SmartBackgroundBlocks
        {
            get
            {
                yield return new Pyramid(_sceneDefinition);
            }
        }

        public override void BuildBackgroundNameTable(NBitPlane nameTable)
        {

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

            // pyramid
            masterPatternTable.CopyTilesTo(
               destination: vramPatternTable,
               source: new InMemoryByteRectangle(11, 11, 3, 1),
               destinationPoint: new Point(0, 3),
               _specs,
               memory);

            // pyramid2
            masterPatternTable.CopyTilesTo(
               destination: vramPatternTable,
               source: new InMemoryByteRectangle(6, 15, 2, 1),
               destinationPoint: new Point(3, 3),
               _specs,
               memory);

            masterPatternTable.CopyTilesTo(
               destination: vramPatternTable,
               source: new InMemoryByteRectangle(4, 4, 1, 1),
               destinationPoint: new Point(5, 3),
               _specs,
               memory);

            masterPatternTable.CopyTilesTo(
             destination: vramPatternTable,
             source: new InMemoryByteRectangle(9, 13, 2, 1),
             destinationPoint: new Point(6, 3),
             _specs,
             memory);

        }
    }
}
