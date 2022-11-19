using ChompGame.Data;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SceneModels
{
    class SceneBuilder
    {
        public static SceneDefinition SetupTestScene(SystemMemoryBuilder memoryBuilder)
        {
            SceneDefinition testScene = new SceneDefinition(memoryBuilder, 8, GenerateLevel1TileMaster);

            //tiles 
            testScene.AddPatternTableRegion(
                region: new InMemoryByteRectangle(0, (int)testScene.TileRow, 8, 1),
                destination: new Point(0, 0),
                memoryBuilder: memoryBuilder);

            testScene.AddPatternTableRegion(
               region: new InMemoryByteRectangle(8, (int)testScene.TileRow, 8, 1),
               destination: new Point(0, 1),
               memoryBuilder: memoryBuilder);

            //sprites
            testScene.AddPatternTableRegion(
                region: new InMemoryByteRectangle(0, 0, 8, 2),
                destination: new Point(0, 2),
                memoryBuilder: memoryBuilder);

            //bat
            testScene.AddPatternTableRegion(
                region: new InMemoryByteRectangle(8, 0, 4, 1),
                destination: new Point(0, 4),
                memoryBuilder: memoryBuilder);

            //text
            testScene.AddPatternTableRegion(
               region: new InMemoryByteRectangle(4, 3, 8, 2),
               destination: new Point(0, 5),
               memoryBuilder: memoryBuilder);

            //text2
            testScene.AddPatternTableRegion(
                region: new InMemoryByteRectangle(12, 4, 2, 1),
                destination: new Point(6, 7),
                memoryBuilder: memoryBuilder);

            //health guage
            testScene.AddPatternTableRegion(
                region: new InMemoryByteRectangle(0, 4, 4, 1),
                destination: new Point(0, 7),
                memoryBuilder: memoryBuilder);

            testScene.AddTileRegion(memoryBuilder)
                .SetSource(0, 0, 2, 2)
                .SetDestination(0, 12, 20, 8);

            return testScene;
        }


        private enum Level1Tiles : byte
        {
            Blank = 0,
            GreenCross1 = 9,
            GreenCross2 = 10
        }

        public static TileMasterNameTable GenerateLevel1TileMaster(SystemMemoryBuilder memoryBuilder)
        {
            var nameTable = new TileMasterNameTable(memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            memoryBuilder.AddBytes(nameTable.Bytes);

            int x = 0, y = 0;

            nameTable[x, y] = (byte)Level1Tiles.GreenCross1;
            nameTable[x + 1, y] = (byte)Level1Tiles.GreenCross2;
            nameTable[x, y + 1] = (byte)Level1Tiles.GreenCross2;
            nameTable[x + 1, y + 1] = (byte)Level1Tiles.GreenCross1;

            return nameTable;
        }

    }
}
