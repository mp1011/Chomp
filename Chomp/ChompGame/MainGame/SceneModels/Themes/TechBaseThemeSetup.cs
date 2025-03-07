using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels.SmartBackground;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ChompGame.MainGame.SceneModels.Themes
{
    class TechBaseThemeSetup : ThemeSetup
    {
        public TechBaseThemeSetup(ChompGameModule gameModule) : base(gameModule)
        {
        }

        public override IEnumerable<SmartBackgroundBlock> SmartBackgroundBlocks
        {
            get
            {
                if(_sceneDefinition.ScrollStyle == ScrollStyle.Vertical || _sceneDefinition.ScrollStyle == ScrollStyle.NameTable)
                    yield return new TechBasePillar(_sceneDefinition);
            }
        }

        public override NBitPlane BuildAttributeTable(NBitPlane attributeTable, NBitPlane nameTable)
        {
            attributeTable.ForEach((x, y, b) =>
            {
                var tile = nameTable[x * 2, y * 2];

                if(tile != 0)
                    attributeTable[x, y] = 1;
                else
                    attributeTable[x, y] = 0;

            });

            return attributeTable;
        }

        public override void BuildBackgroundNameTable(NBitPlane nameTable)
        {
            if (_sceneDefinition.ScrollStyle == ScrollStyle.Vertical || _sceneDefinition.ScrollStyle == ScrollStyle.NameTable)
                return;

            nameTable.ForEach((x, y, b) =>
            {
                if (nameTable[x, y] != 0)
                    return;

                var top = _sceneDefinition.GetBackgroundLayerTile(BackgroundPart.Upper, false);
                if (y < top)
                    nameTable[x, y] = 7;
                else if (y == top)
                {
                    if (x.IsMod(4))
                        nameTable[x, y] = 4;
                    else if ((x-1).IsMod(4))
                        nameTable[x, y] = 5;
                    else
                        nameTable[x, y] = 6;
                }

                var mid = _sceneDefinition.GetBackgroundLayerTile(BackgroundPart.Middle, false);
                if (y == mid && !x.IsMod(4))
                    nameTable[x, y] = 24;

                var bottom = _sceneDefinition.GetBackgroundLayerTile(BackgroundPart.Lower, false);
                if (y > bottom)
                    nameTable[x, y] = 7;
                else if (y == bottom)
                {
                    if (x.IsMod(4))
                        nameTable[x, y] = 1;
                    else if ((x - 1).IsMod(4))
                        nameTable[x, y] = 2;
                    else
                        nameTable[x, y] = 3;
                }

            });
             
        }

        public override void SetupVRAMPatternTable(NBitPlane masterPatternTable, NBitPlane vramPatternTable, SystemMemory memory)
        {
            //bg
            masterPatternTable.CopyTilesTo(
              destination: vramPatternTable,
              source: new InMemoryByteRectangle(0, 10, 3, 1),
              destinationPoint: new Point(1, 0),
              _specs,
              memory);

            masterPatternTable.CopyTilesTo(
             destination: vramPatternTable,
             source: new InMemoryByteRectangle(0, 11, 3, 1),
             destinationPoint: new Point(4, 0),
             _specs,
             memory);

            masterPatternTable.CopyTilesTo(
               destination: vramPatternTable,
               source: new InMemoryByteRectangle(5, 7, 1, 1),
               destinationPoint: new Point(7, 0),
               _specs,
               memory);

            //bg extra
            masterPatternTable.CopyTilesTo(
             destination: vramPatternTable,
             source: new InMemoryByteRectangle(6, 11, 1, 1),
             destinationPoint: new Point(0, 3),
             _specs,
             memory);

            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(15, 10, 1, 1),
                destinationPoint: new Point(1, 3),
                _specs,
                memory);

            masterPatternTable.CopyTilesTo(
              destination: vramPatternTable,
              source: new InMemoryByteRectangle(3, 11, 2, 1),
              destinationPoint: new Point(2, 3),
              _specs,
              memory);


            // fg
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(5, 7, 1, 1),
                destinationPoint: new Point(0, 1),
                _specs,
                memory);

            masterPatternTable.CopyTilesTo(
              destination: vramPatternTable,
              source: new InMemoryByteRectangle(5, 7, 1, 1),
              destinationPoint: new Point(1, 1),
              _specs,
              memory);

            masterPatternTable.CopyTilesTo(
             destination: vramPatternTable,
             source: new InMemoryByteRectangle(2, 12, 3, 1),
             destinationPoint: new Point(2, 1),
             _specs,
             memory);

            //masterPatternTable.CopyTilesTo(
            //   destination: vramPatternTable,
            //   source: new InMemoryByteRectangle(3, 12, 1, 1),
            //   destinationPoint: new Point(4, 1),
            //   _specs,
            //   memory);


            masterPatternTable.CopyTilesTo(
               destination: vramPatternTable,
               source: new InMemoryByteRectangle(5, 12, 1, 1),
               destinationPoint: new Point(5, 1),
               _specs,
               memory);

            masterPatternTable.CopyTilesTo(
             destination: vramPatternTable,
             source: new InMemoryByteRectangle(8, 12, 2, 1),
             destinationPoint: new Point(6, 1),
             _specs,
             memory);
        }
    }
}
