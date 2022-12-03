using ChompGame.Data;
using ChompGame.GameSystem;

namespace ChompGame.MainGame.SceneModels
{
    public enum Level:byte 
    {
        TestScene=0,
    }

    class SceneBuilder
    {
        public static void AddSceneHeaders(SystemMemoryBuilder memoryBuilder, Specs specs)
        {
            new SceneDefinition(
                specs: specs,
                scrollStyle: ScrollStyle.Horizontal,
                levelShape: LevelShape.MediumVariance,
                beginTiles:0,
                endTiles:2,
                spriteLoadFlags: SpriteLoadFlags.Player | SpriteLoadFlags.Bird | SpriteLoadFlags.Lizard,
                memoryBuilder: memoryBuilder,
                tileRow: 8,         
                groundFillTiles: 2,
                groundTopTiles: 1,
                sideTiles: 1,
                parallaxTileBegin:3,
                parallaxSizeA: 2,
                parallaxSizeB: 2);
        }

        public static void AddSceneParts(SystemMemoryBuilder builder, Specs specs)
        {
            SceneDefinition testScene = new SceneDefinition(Level.TestScene, builder.Memory, specs);

            testScene.PartsAddress = 0;

            new ScenePart(builder, ScenePartType.EnemyType1, 6, 6, testScene);

        }
    }
}
