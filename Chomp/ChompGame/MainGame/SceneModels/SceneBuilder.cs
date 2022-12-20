using ChompGame.Data.Memory;
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
                parallaxSizeB: 2,
                enemyAPalette: 2,
                enemyBPalette: 2,
                extraAPalette: 3,
                extraBPalette: 3);
        }

        public static void AddSceneParts(SystemMemoryBuilder builder, Specs specs)
        {
            SceneDefinition testScene = new SceneDefinition(Level.TestScene, builder.Memory, specs);

            testScene.PartsAddress = (byte)(builder.CurrentAddress - builder.Memory.GetAddress(AddressLabels.SceneParts));

            new ScenePartsHeader(builder, 
                b => new ScenePart(b, ScenePartType.EnemyType1, 8, 6, testScene),
                b => new ScenePart(b, ScenePartType.Bomb, 6, 6, testScene),
                b => new ScenePart(b, ScenePartType.EnemyType1, 50, 6, testScene));

        }
    }
}
