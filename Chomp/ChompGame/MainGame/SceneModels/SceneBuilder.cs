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
                theme: Theme.Plains,
                enemyGroup: EnemyGroup.Lizard_Bird,
                memoryBuilder: memoryBuilder,
                top:0,
                bottom:2
            );

            //TestSceneNoScrollFlat
            new SceneDefinition(
               specs: specs,
               scrollStyle: ScrollStyle.None,
               levelShape: LevelShape.Flat,
               theme: Theme.Plains,
               enemyGroup: EnemyGroup.Lizard_Bird,
               memoryBuilder: memoryBuilder,
               top: 1,
               bottom: 1,
               left: 1,
               right: 1
           );

            //TestSceneNoScrollCornerStairs
            new SceneDefinition(
               specs: specs,
               scrollStyle: ScrollStyle.None,
               levelShape: LevelShape.CornerStairs,
               theme: Theme.Plains,
               enemyGroup: EnemyGroup.Lizard_Bird,
               memoryBuilder: memoryBuilder,
               top: 1,
               bottom: 1,
               left: 1,
               right: 1
           );

            //TestSceneNoScrollBigStair
            new SceneDefinition(
               specs: specs,
               scrollStyle: ScrollStyle.None,
               levelShape: LevelShape.BigStair,
               theme: Theme.Plains,
               enemyGroup: EnemyGroup.Lizard_Bird,
               memoryBuilder: memoryBuilder,
               top: 1,
               bottom: 1,
               left: 1,
               right: 1
           );

            //TestSceneNoScrollTShape
            new SceneDefinition(
               specs: specs,
               scrollStyle: ScrollStyle.None,
               levelShape: LevelShape.TShape,
               theme: Theme.Plains,
               enemyGroup: EnemyGroup.Lizard_Bird,
               memoryBuilder: memoryBuilder,
               top: 1,
               bottom: 1,
               left: 1,
               right: 1
           );
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
