using ChompGame.Data.Memory;
using ChompGame.GameSystem;

namespace ChompGame.MainGame.SceneModels
{
    public enum Level:byte 
    {
        TestSceneHorizontal=0,
        TestSceneNoScrollFlat = 1,
        TestSceneNoScrollCornerStairs = 2,
        TestSceneNoScrollBigStair = 3,
        TestSceneNoScrollTShape = 4,

    }

    class SceneBuilder
    {
        public static void AddSceneHeaders(SystemMemoryBuilder memoryBuilder, Specs specs)
        {
            //TestSceneHorizontal
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

            //TestSceneNoScrollFlat
            new SceneDefinition(
               specs: specs,
               scrollStyle: ScrollStyle.None,
               levelShape: LevelShape.Flat,
               beginTiles: 1,
               endTiles: 1,
               spriteLoadFlags: SpriteLoadFlags.Player,
               memoryBuilder: memoryBuilder,
               tileRow: 8,
               groundFillTiles: 2,
               groundTopTiles: 1,
               sideTiles: 1,
               parallaxTileBegin: 3,
               parallaxSizeA: 2,
               parallaxSizeB: 2,
               enemyAPalette: 2,
               enemyBPalette: 2,
               extraAPalette: 3,
               extraBPalette: 3);

            //TestSceneNoScrollCornerStairs
            new SceneDefinition(
               specs: specs,
               scrollStyle: ScrollStyle.None,
               levelShape: LevelShape.CornerStairs,
               beginTiles: 1,
               endTiles: 1,
               spriteLoadFlags: SpriteLoadFlags.Player,
               memoryBuilder: memoryBuilder,
               tileRow: 8,
               groundFillTiles: 2,
               groundTopTiles: 1,
               sideTiles: 1,
               parallaxTileBegin: 3,
               parallaxSizeA: 2,
               parallaxSizeB: 2,
               enemyAPalette: 2,
               enemyBPalette: 2,
               extraAPalette: 3,
               extraBPalette: 3);

            //TestSceneNoScrollBigStair
            new SceneDefinition(
               specs: specs,
               scrollStyle: ScrollStyle.None,
               levelShape: LevelShape.BigStair,
               beginTiles: 1,
               endTiles: 1,
               spriteLoadFlags: SpriteLoadFlags.Player,
               memoryBuilder: memoryBuilder,
               tileRow: 8,
               groundFillTiles: 2,
               groundTopTiles: 1,
               sideTiles: 1,
               parallaxTileBegin: 3,
               parallaxSizeA: 2,
               parallaxSizeB: 2,
               enemyAPalette: 2,
               enemyBPalette: 2,
               extraAPalette: 3,
               extraBPalette: 3);

            //TestSceneNoScrollTShape
            new SceneDefinition(
               specs: specs,
               scrollStyle: ScrollStyle.None,
               levelShape: LevelShape.TShape,
               beginTiles: 1,
               endTiles: 1,
               spriteLoadFlags: SpriteLoadFlags.Player,
               memoryBuilder: memoryBuilder,
               tileRow: 8,
               groundFillTiles: 2,
               groundTopTiles: 1,
               sideTiles: 1,
               parallaxTileBegin: 3,
               parallaxSizeA: 2,
               parallaxSizeB: 2,
               enemyAPalette: 2,
               enemyBPalette: 2,
               extraAPalette: 3,
               extraBPalette: 3);
        }



        public static void AddSceneParts(SystemMemoryBuilder builder, Specs specs)
        {
            SceneDefinition scene = new SceneDefinition(Level.TestSceneHorizontal, builder.Memory, specs);
            new ScenePartsHeader(builder,
                b => new ScenePart(b, ScenePartType.Pit, 10, 3, scene),
                b => new ScenePart(b, ScenePartType.Pit, 20, 2, scene),
                b => new ScenePart(b, ScenePartType.EnemyType1, 8, 6, scene),
                b => new ScenePart(b, ScenePartType.Bomb, 6, 6, scene),
                b => new ScenePart(b, ScenePartType.EnemyType1, 50, 6, scene));

            scene = new SceneDefinition(Level.TestSceneNoScrollFlat, builder.Memory, specs);
            new ScenePartsHeader(builder,
                b => new ScenePart(b, ScenePartType.Bomb, 6, 6, scene));

            scene = new SceneDefinition(Level.TestSceneNoScrollCornerStairs, builder.Memory, specs);
            new ScenePartsHeader(builder,
                b => new ScenePart(b, ScenePartType.Bomb, 6, 6, scene));

            scene = new SceneDefinition(Level.TestSceneNoScrollBigStair, builder.Memory, specs);
            new ScenePartsHeader(builder,
                b => new ScenePart(b, ScenePartType.Bomb, 6, 6, scene));

            scene = new SceneDefinition(Level.TestSceneNoScrollTShape, builder.Memory, specs);
            new ScenePartsHeader(builder,
                b => new ScenePart(b, ScenePartType.Bomb, 6, 6, scene));



        }
    }
}
