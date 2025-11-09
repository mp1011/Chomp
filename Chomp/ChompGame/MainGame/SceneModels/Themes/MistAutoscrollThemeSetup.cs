using ChompGame.Data;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SceneModels.Themes
{
    class MistAutoscrollThemeSetup : ThemeSetup
    {
        private const int Brick = 3;
        private const int BrickLeft = 4;
        private const int BrickRight = 5;
        public MistAutoscrollThemeSetup(ChompGameModule gameModule) : base(gameModule)
        {
        }

        public override void BuildBackgroundNameTable(NBitPlane nameTable)
        {
             
            nameTable.ForEach((x, y, b) =>
            {
                if (y <= _sceneDefinition.GetBackgroundLayerTile(BackgroundPart.Upper, false))
                    nameTable[x, y] = (byte)(1 + _gameModule.RandomModule.Generate(1));

                if (y == 11)
                    nameTable[x, y] = Brick;
                else if(y > 11)
                {
                    if ((x % 16) == 0)
                        nameTable[x, y] = Brick;

                    if (y == 12 && ((x - 1) % 16) == 0)
                        nameTable[x, y] = BrickLeft;

                    if (y == 12 && ((x + 1) % 16) == 0)
                        nameTable[x, y] = BrickRight;


                }
            });
        }

        public override NBitPlane BuildAttributeTable(NBitPlane attributeTable, NBitPlane nameTable)
        {
            attributeTable.ForEach((x, y, b) =>
            {
                attributeTable[x, y] = 0;                
            });

            return attributeTable;
        }

        public override void SetupVRAMPatternTable()
        {
            _gameModule.TileCopier.CopyTilesForMistAutoscrollTheme();
        }
    }
}
