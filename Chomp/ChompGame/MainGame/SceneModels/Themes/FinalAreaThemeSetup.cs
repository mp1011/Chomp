﻿using ChompGame.Data;
using ChompGame.Extensions;
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
            if(_sceneDefinition.ScrollStyle == ScrollStyle.NameTable)
            {
                nameTable.ForEach((x, y, b) =>
                {
                    if (b == 0)
                    {
                        if (y == 6 || y == 8 || y == 14 || y == 16)
                        {
                            if (x.IsMod(3))
                                nameTable[x, y] = 3;
                            else
                                nameTable[x, y] = 4;
                        }
                    }
                });
            }    
            else if (_sceneDefinition.ScrollStyle == ScrollStyle.Horizontal)
            {
                nameTable.ForEach((x, y, b) =>
                {
                    if(y == 4 || y == 2)
                    {
                        if (x.IsMod(3))
                            nameTable[x, y] = 3;
                        else
                            nameTable[x, y] = 4;
                    }
                });
            }
            else
            {
                nameTable.ForEach((x, y, b) =>
                {
                    if (b == 0)
                    {
                        if (x.IsMod(2) || y.IsMod(2))
                            nameTable[x, y] = 1;
                        else
                            nameTable[x, y] = 2;
                    }
                });
            }
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

            // bricks
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 14, 2, 1),
                destinationPoint: new Point(1, 0),
                _specs,
                memory);

            // pipe
            masterPatternTable.CopyTilesTo(
              destination: vramPatternTable,
              source: new InMemoryByteRectangle(6, 0, 1, 1),
              destinationPoint: new Point(3, 0),
              _specs,
              memory);

            masterPatternTable.CopyTilesTo(
             destination: vramPatternTable,
             source: new InMemoryByteRectangle(6, 6, 1, 1),
             destinationPoint: new Point(4, 0),
             _specs,
             memory);

        }
    }
}
