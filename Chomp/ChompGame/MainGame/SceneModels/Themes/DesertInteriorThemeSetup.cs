using ChompGame.Data;

namespace ChompGame.MainGame.SceneModels.Themes
{
    class DesertInteriorThemeSetup : ThemeSetup
    {
        private const int Torch = 1;
        private const int BackTile1 = 2;
        private const int BackTile2 = 3;

        public DesertInteriorThemeSetup(ChompGameModule m) : base(m) 
        {
        }


        public override void BuildBackgroundNameTable(NBitPlane nameTable)
        {
            if(_sceneDefinition.ScrollStyle == ScrollStyle.Horizontal 
                && _sceneDefinition.HorizontalScrollStyle == HorizontalScrollStyle.Interior)
            {
                var layer2Start = _sceneDefinition.GetBackgroundLayerTile(BackgroundPart.Upper, false);
                var layer2End = _sceneDefinition.GetBackgroundLayerTile(BackgroundPart.Middle, false);
                var fgStart = _sceneDefinition.GetBackgroundLayerTile(BackgroundPart.Lower, false);
                var layer2Mid = layer2Start + (layer2End - layer2Start) / 2;

                nameTable.ForEach((x, y, b) =>
                {
                    if(y < _sceneDefinition.TopTiles || y >= fgStart || nameTable[x, y] != 0)
                        return;
                   
                    if (y >= layer2Start && y < layer2End)
                    {
                        nameTable[x, y] = (byte)((y == layer2Mid && (x % 4) == 0) ? Torch : 0);
                    }
                    else
                    {
                        nameTable[x, y] = (byte)((x % 2) == (y % 2) ? BackTile1 : BackTile2);
                    }
                });
            }
            else
            {
                nameTable.ForEach((x, y, b) =>
                {
                    if (nameTable[x, y] != 0)
                        return;

                    if ((y % 6) != 0)
                        return;

                    if ((y % 12) != 0)
                    {
                        if ((x % 5) != 0)
                            return;
                    }
                    else
                    {
                        if (((x + 2) % 5) != 0)
                            return;
                    }

                    nameTable[x, y] = Torch;
                });
                return;
            }
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

        public override void SetupVRAMPatternTable()
        {
            _gameModule.TileCopier.CopyTilesForDesertInteriorTheme();
        }
    }
}
