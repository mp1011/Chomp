using ChompGame.Data;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SceneModels
{
    class SceneBuilder
    {
        public static SceneDefinition SetupTestScene(SystemMemoryBuilder memoryBuilder)
        {
            SceneDefinition testScene = new SceneDefinition(
                memoryBuilder: memoryBuilder,
                tileRow: 8,
                mapSize: 3,
                shape: LevelShape.Horizontal,
                groundLowTile: 12,
                groundHighTile: 15,
                groundVariation: 2,
                groundFillTiles: 2,
                groundTopTiles: 1,
                sideTiles: 1);

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

            return testScene;
        }
    }
}
