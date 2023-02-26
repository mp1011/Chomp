using ChompGame.Data.Memory;
using ChompGame.GameSystem;

namespace ChompGame.MainGame.SceneModels
{
    public enum Level:byte 
    {
        Level1_1_Start,
        Level1_2_Horizontal,
        Level1_3_Pit,
        Level1_4_DeadEnd,
        Level1_5_Vertical,
        Level1_6_Platforms,
        Level1_7_Door,
        Level1_8_Door2,
        Level1_9_Platforms2,
        Level1_10_Stair,
        Level1_11_Pillars
    }

    class SceneBuilder
    {
        public static void AddSceneHeaders(SystemMemoryBuilder memoryBuilder, Specs specs)
        {
            //Level1_1_Start
            SceneDefinition.NoScrollFlat(
               specs: specs,
               theme: Theme.Plains,
               enemyGroup: EnemyGroup.Lizard_Bird,
               memoryBuilder: memoryBuilder,
               top: 0,
               bottom: 1,
               left: 1,
               right: 0,
               bgPosition: 2
            );

            //Level1_2_Horizontal,
            SceneDefinition.HorizontalScroll(
                specs: specs,
                variance: LevelShape.MediumVariance,
                theme: Theme.Plains,
                enemyGroup: EnemyGroup.Lizard_Bird,
                memoryBuilder: memoryBuilder,
                top: 0,
                bottom: 2,
                bgPosition1: 3,
                bgPosition2: 2
            );

            //Level1_3_Pit,
            SceneDefinition.NoScrollCornerStairs(
                specs: specs,
                theme: Theme.Plains,
                enemyGroup: EnemyGroup.Lizard_Bird,
                memoryBuilder: memoryBuilder,
                left:0,
                top: 0,
                right:0,
                bottom: 2,
                bgPosition: 1,
                cornerStairStyle: CornerStairStyle.TwoBlockDouble
            );

            //Level1_4_DeadEnd,
            SceneDefinition.NoScrollFlat(
              specs: specs,
              theme: Theme.Plains,
              enemyGroup: EnemyGroup.Lizard_Bird,
              memoryBuilder: memoryBuilder,
              top: 0,
              bottom: 2,
              left: 0,
              right: 2,
              bgPosition: 2
           );

            //Level1_5_Vertical,
            SceneDefinition.VerticalScroll(
              specs: specs,
              variance: LevelShape.ZigZag,
              theme: Theme.Plains,
              enemyGroup: EnemyGroup.Lizard_Bird,
              memoryBuilder: memoryBuilder,
              left: 1,
              right: 1
          );

            //Level1_6_Platforms,
            SceneDefinition.HorizontalScroll(
               specs: specs,
               variance: LevelShape.HighVariance,
               theme: Theme.Plains,
               enemyGroup: EnemyGroup.Lizard_Bird,
               memoryBuilder: memoryBuilder,
               top: 0,
               bottom: 2,
               bgPosition1: 3,
               bgPosition2: 2
           );

            //Level1_7_Door,
            SceneDefinition.NoScrollFlat(
               specs: specs,
               theme: Theme.Plains,
               enemyGroup: EnemyGroup.Lizard_Bird,
               memoryBuilder: memoryBuilder,
               top: 1,
               bottom: 1,
               left: 1,
               right: 1,
               bgPosition: 2
            );

            //Level1_8_Door2,
            SceneDefinition.NoScrollFlat(
               specs: specs,
               theme: Theme.Plains,
               enemyGroup: EnemyGroup.Lizard_Bird,
               memoryBuilder: memoryBuilder,
               top: 1,
               bottom: 1,
               left: 1,
               right: 1,
               bgPosition: 2
            );

            //Level1_9_Platforms2,
            SceneDefinition.HorizontalScroll(
              specs: specs,
              variance: LevelShape.HighVariance,
              theme: Theme.Plains,
              enemyGroup: EnemyGroup.Lizard_Bird,
              memoryBuilder: memoryBuilder,
              top: 0,
              bottom: 2,
              bgPosition1: 3,
              bgPosition2: 2
          );

            //Level1_10_Stair,
            SceneDefinition.NoScrollBigStairs(
             specs: specs,
             theme: Theme.Plains,
             enemyGroup: EnemyGroup.Lizard_Bird,
             memoryBuilder: memoryBuilder,
             top: 0,
             bottom: 2,
             left: 0,
             right: 2,
             bgPosition: 2
          );

            //Level1_11_Pillars
            SceneDefinition.HorizontalScroll(
                 specs: specs,
                 variance: LevelShape.Flat,
                 theme: Theme.Plains,
                 enemyGroup: EnemyGroup.Lizard_Bird,
                 memoryBuilder: memoryBuilder,
                 top: 0,
                 bottom: 2,
                 bgPosition1: 3,
                 bgPosition2: 2
         );
        }

        public static void AddSceneParts(SystemMemoryBuilder builder, Specs specs)
        {
            SceneDefinition scene = new SceneDefinition(Level.Level1_1_Start, builder.Memory, specs);
            new ScenePartsHeader(builder,
                b => new ScenePart(b, ExitType.Right, 1, scene),
                b => new ScenePart(b, ScenePartType.Bomb, 6, 6, scene),
                b => new ScenePart(b, DynamicBlockType.DestructibleBlock, true,true,true,true,14,6, scene),
                b => new ScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 14, 8, scene),
                b => new ScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 14, 10, scene)
                );

            scene = new SceneDefinition(Level.Level1_2_Horizontal, builder.Memory, specs);
            new ScenePartsHeader(builder,
                b => new ScenePart(b, ExitType.Left, -1, scene),
                b => new ScenePart(b, ExitType.Right, 1, scene),
                b => new ScenePart(b, ScenePartType.Bomb, 6, 6, scene),
                b => new ScenePart(b, ScenePartType.EnemyType1, 12, 6, scene)
                );

            scene = new SceneDefinition(Level.Level1_3_Pit, builder.Memory, specs);
            new ScenePartsHeader(builder,
                b => new ScenePart(b, ExitType.Left, -1, scene),
                b => new ScenePart(b, ExitType.Right, 1, scene),
                b => new ScenePart(b, ExitType.Bottom, 2, scene)
                );

            scene = new SceneDefinition(Level.Level1_4_DeadEnd, builder.Memory, specs);
            new ScenePartsHeader(builder,
                b => new ScenePart(b, ExitType.Left, -1, scene)
                );

            scene = new SceneDefinition(Level.Level1_5_Vertical, builder.Memory, specs);
            new ScenePartsHeader(builder,
                b => new ScenePart(b, ScenePartType.Bomb, 6, 6, scene),
                b => new ScenePart(b, ExitType.Right, exitOffset:5, scene),
                b => new ScenePart(b, ScenePartType.DoorFowardExit, 3, 9, scene)
                );

            scene = new SceneDefinition(Level.Level1_6_Platforms, builder.Memory, specs);
            new ScenePartsHeader(builder,
                b => new ScenePart(b, ScenePartType.DoorBackExit, 60, 11, scene),
                b => new ScenePart(b, ScenePartType.Pit, 30,10, scene),
                b => new ScenePart(b, ScenePartType.Platform_UpDown, 35, 12, scene),
                b => new ScenePart(b, ScenePartType.Platform_LeftRight, 12, 12, scene),
                b => new ScenePart(b, ScenePartType.Pit, 10, 10, scene)
                );

        }
    }
}
