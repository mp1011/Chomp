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

        public override void SetupVRAMPatternTable(NBitPlane masterPatternTable, NBitPlane vramPatternTable, SystemMemory memory)
        {
            //mist
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(15, 2, 1, 1),
                destinationPoint: new Point(1, 0),
                _specs,
                memory);

            //mist
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(15, 4, 1, 1),
                destinationPoint: new Point(2, 0),
                _specs,
                memory);

            //solid
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(5, 7, 1, 1),
                destinationPoint: new Point(6, 0),
                _specs,
                memory);

            //bg
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 12, 8, 1),
                destinationPoint: new Point(0, 1),
                _specs,
                memory);

            //brick
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(2, 8, 1, 1),
                destinationPoint: new Point(3, 0),
                _specs,
                memory);

            //brick l
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(7, 9, 1, 1),
                destinationPoint: new Point(4, 0),
                _specs,
                memory);

            //brick r
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(7, 10, 1, 1),
                destinationPoint: new Point(5, 0),
                _specs,
                memory);

            // star 
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(5, 6, 1, 1),
                destinationPoint: new Point(6, 0),
                _specs,
                memory);
        }
    }
}
