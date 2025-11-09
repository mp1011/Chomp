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

        public override void SetupVRAMPatternTable()
        {
            _gameModule.TileCopier.CopyTilesForTechBaseTheme();
        }
    }
}
