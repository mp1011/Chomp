using ChompGame.Data;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SceneModels
{
    class SceneBuilder
    {
        public static SceneDefinition SetupTestScene(SystemMemoryBuilder memoryBuilder)
        {
            SceneDefinition testScene = new SceneDefinition(memoryBuilder);

            //sprites
            testScene.AddRegion(
                region: new InMemoryByteRectangle(0, 0, 8, 2),
                destination: new Point(0, 2),
                memoryBuilder: memoryBuilder);

            //sky
            testScene.AddRegion(
               region: new InMemoryByteRectangle(0, 6, 8, 1),
               destination: new Point(0, 0),
               memoryBuilder: memoryBuilder);

            //bg1
            testScene.AddRegion(
                region: new InMemoryByteRectangle(0, 7, 6, 1),
                destination: new Point(0, 1),
                memoryBuilder: memoryBuilder);

            //bg2
            testScene.AddRegion(
                region: new InMemoryByteRectangle(5, 7, 3, 1),
                destination: new Point(5, 1),
                memoryBuilder: memoryBuilder);

            //text
            testScene.AddRegion(
               region: new InMemoryByteRectangle(4, 3, 8, 2),
               destination: new Point(0, 5),
               memoryBuilder: memoryBuilder);

            //text2
            testScene.AddRegion(
                region: new InMemoryByteRectangle(12, 4, 2, 1),
                destination: new Point(6, 7),
                memoryBuilder: memoryBuilder);

            //health guage
            testScene.AddRegion(
                region: new InMemoryByteRectangle(0, 4, 4, 1),
                destination: new Point(0, 7),
                memoryBuilder: memoryBuilder);

            //bat
            testScene.AddRegion(
                region: new InMemoryByteRectangle(8, 0, 4, 1),
                destination: new Point(0, 4),
                memoryBuilder: memoryBuilder);

            return testScene;
        }

    }
}
