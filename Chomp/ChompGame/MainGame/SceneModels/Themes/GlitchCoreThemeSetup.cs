using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels.SmartBackground;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ChompGame.MainGame.SceneModels.Themes
{
    class GlitchCoreThemeSetup : ThemeSetup
    {
        private readonly RandomModule _rng;

        public GlitchCoreThemeSetup(ChompGameModule gameModule) : base(gameModule)
        {
            _rng = gameModule.RandomModule;
        }

        public override void BuildBackgroundNameTable(NBitPlane nameTable)
        {
            nameTable.ForEach((x, y, b) =>
            {
                if(b == 0)
                {
                    if (_rng.Generate(1) == 0)
                        nameTable[x, y] = 24;
                    else
                        nameTable[x, y] = 25;
                }
            });
        }

        public override NBitPlane BuildAttributeTable(NBitPlane attributeTable, NBitPlane nameTable)
        {
            attributeTable.ForEach((x, y, b) =>
            {
                var tile = nameTable[x * 2, y * 2];

                if (tile != 0)
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
              source: new InMemoryByteRectangle(0, 0, 7, 1),
              destinationPoint: new Point(1, 0),
              _specs,
              memory);

            masterPatternTable.CopyTilesTo(
             destination: vramPatternTable,
             source: new InMemoryByteRectangle(2, 1, 8, 2),
             destinationPoint: new Point(0, 1),
             _specs,
             memory);

        }
    }
}
