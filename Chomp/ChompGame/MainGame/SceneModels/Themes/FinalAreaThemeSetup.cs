using ChompGame.Data;
using ChompGame.GameSystem;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SceneModels.Themes
{
    class FinalAreaThemeSetup : ThemeSetup
    {
        private readonly RandomModule _rng;

        public FinalAreaThemeSetup(ChompGameModule gameModule) : base(gameModule)
        {
            _rng = gameModule.RandomModule;
        }

        public override void BuildBackgroundNameTable(NBitPlane nameTable)
        {
            
        }

        public override NBitPlane BuildAttributeTable(NBitPlane attributeTable, NBitPlane nameTable)
        {
            return base.BuildAttributeTable(attributeTable, nameTable);
        }

        public override void SetupVRAMPatternTable(NBitPlane masterPatternTable, NBitPlane vramPatternTable, SystemMemory memory)
        {            
            masterPatternTable.CopyTilesTo(
              destination: vramPatternTable,
              source: new InMemoryByteRectangle(0, 15, 6, 1),
              destinationPoint: new Point(0, 1),
              _specs,
              memory);

            masterPatternTable.CopyTilesTo(
             destination: vramPatternTable,
             source: new InMemoryByteRectangle(14, 15, 1, 1),
             destinationPoint: new Point(6, 1),
             _specs,
             memory);


            masterPatternTable.CopyTilesTo(
             destination: vramPatternTable,
             source: new InMemoryByteRectangle(15, 13, 1, 1),
             destinationPoint: new Point(7, 1),
             _specs,
             memory);


        }
    }
}
