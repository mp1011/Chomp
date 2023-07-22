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

    }
}
