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
        Level1_11_Boss,
        Level1_12_Horizontal2

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
            SceneDefinition.NoScrollCornerStairs(
               specs: specs,
               theme: Theme.Plains,
               enemyGroup: EnemyGroup.Lizard_Bird,
               memoryBuilder: memoryBuilder,
               cornerStairStyle: CornerStairStyle.TwoBlockDouble,
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
             right: 0,
             bgPosition: 2
          );

            //Level1_11_Boss
            SceneDefinition.NoScrollFlat(
                    specs: specs,
                    theme: Theme.Plains,
                    enemyGroup: EnemyGroup.Boss,
                    memoryBuilder: memoryBuilder,
                    top: 1,
                    left: 0,
                    right: 0,
                    bottom: 1,
                    bgPosition: 2);

            //Level1_12_Horizontal2,
            SceneDefinition.HorizontalScroll(
                specs: specs,
                variance: LevelShape.HighVariance,
                theme: Theme.PlainsEvening,
                enemyGroup: EnemyGroup.Lizard_Bird,
                memoryBuilder: memoryBuilder,
                top: 0,
                bottom: 1,
                bgPosition1: 3,
                bgPosition2: 2
            );

    }

        public static void AddSceneParts(SystemMemoryBuilder builder, Specs specs)
        {
            int destroyBitsNeeded = 0;

            SceneDefinition scene = new SceneDefinition(Level.Level1_1_Start, builder.Memory, specs);
            var header = new ScenePartsHeader(builder,
                b => new ScenePart(b, ExitType.Right, 1, scene),
                b => new ScenePart(b, ScenePartType.Bomb, 8, 12, scene),
                b => new ScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 12, 6, scene),
                b => new ScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 12, 8, scene),
                b => new ScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 12, 10, scene),
                b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: true, x: 4, y: 8, definition: scene),
                b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 6, y: 8, definition: scene),
                b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: false, x: 8, y: 8, definition: scene)
                );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_2_Horizontal, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: false, x: 8, y: 8, definition: scene)
                ,b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: false, x: 10, y: 10, definition: scene)
                ,b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: false, x: 12, y: 8, definition: scene)
                ,b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 18, y: 8, definition: scene)
                ,b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 22, y: 8, definition: scene)
                ,b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: false, x: 36, y: 8, definition: scene)
                ,b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: false, x: 38, y: 8, definition: scene)
                ,b => new ScenePart(b, ExitType.Left, -1, scene)
                ,b => new ScenePart(b, ExitType.Right, 1, scene)
             //   ,b => new ScenePart(b, ScenePartType.Bomb, 20, 8, scene)
                ,b => new ScenePart(b, ScenePartType.Bomb, 47, 9, scene)
                ,b => new ScenePart(b, ScenePartType.EnemyType1, 12, 12, scene)
                ,b => new ScenePart(b, ScenePartType.EnemyType1, 33, 8, scene)
                ,b => new ScenePart(b, ScenePartType.EnemyType1, 58, 12, scene)
                );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_3_Pit, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new ScenePart(b, ExitType.Left, -1, scene),
                b => new ScenePart(b, ExitType.Right, 1, scene),
                b => new ScenePart(b, ExitType.Bottom, 2, scene),
                b => new ScenePart(b, DynamicBlockType.DestructibleBlock, true,true,false,false,8,10, scene),
                b => new ScenePart(b, DynamicBlockType.DestructibleBlock, true, true, false, false, 6, 10, scene)
                );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_4_DeadEnd, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new ScenePart(b, ExitType.Left, -1, scene)
                , b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 4, y: 8, definition: scene)
                , b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 6, y: 8, definition: scene)
                , b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 8, y: 8, definition: scene)
                , b => new ScenePart(b, ScenePartType.Bomb, 11, 9, scene)
                );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_5_Vertical, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new ScenePart(b, ScenePartType.Bomb, 6, 19, scene),
                b => new ScenePart(b, ScenePartType.EnemyType2, 6, 10, scene),
                b => new ScenePart(b, ScenePartType.EnemyType2, 8, 34, scene),
                b => new ScenePart(b, ExitType.Right, exitOffset:5, scene),
                b => new ScenePart(b, ScenePartType.DoorFowardExit, 3, 36, scene),
                b => new ScenePart(b, ScenePartType.Wall, x: 12, y: 36, scene),
                b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 2, y: 4, definition: scene),
                b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 2, y: 6, definition: scene),
                b => new ScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 12, y: 34, definition: scene),
                b => new ScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 12, y: 32, definition: scene),
                b => new ScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 14, y: 34, definition: scene),
                b => new ScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 14, y: 32, definition: scene)

                );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_6_Platforms, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new ScenePart(b, ScenePartType.DoorBackExit, 60, 11, scene),
                b => new ScenePart(b, ExitType.Left, exitOffset: 1, scene),
                b => new ScenePart(b, ScenePartType.Pit, 30,10, scene),
                b => new ScenePart(b, ScenePartType.Platform_UpDown, 35, 12, scene),
                b => new ScenePart(b, ScenePartType.Platform_LeftRight, 12, 12, scene),
                b => new ScenePart(b, ScenePartType.Pit, 10, 10, scene),
                b => new ScenePart(b, ScenePartType.EnemyType1, 48, 8, scene),
                b => new ScenePart(b, ScenePartType.EnemyType2, 17, 6, scene),
                b => new ScenePart(b, ScenePartType.Bomb, 28,12, scene),
                b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: false, bottomLeft: true, bottomRight: false, x: 50, y: 10, definition: scene),
                b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: false, bottomLeft: true, bottomRight: false, x: 50, y: 8, definition: scene),
                b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: false, bottomLeft: true, bottomRight: false, x: 52, y: 10, definition: scene),
                b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: false, bottomLeft: true, bottomRight: false, x: 52, y: 8, definition: scene)

                );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_7_Door, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new ScenePart(b, ScenePartType.DoorFowardExit, 4, 12, scene),
                b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 6, y: 8, definition: scene),
                b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 8, y: 8, definition: scene),
                b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 10, y: 8, definition: scene),
                b => new ScenePart(b, ExitType.Right, exitOffset: -1, scene)
                );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_8_Door2, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new ScenePart(b, ScenePartType.DoorBackExit, 8, 12, scene),
                b => new ScenePart(b, ExitType.Right, exitOffset: 1, scene),
                b => new ScenePart(b, ScenePartType.EnemyType1, 12, 7, scene),
                b => new ScenePart(b, ScenePartType.EnemyType2, 4, 7, scene)
                );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_9_Platforms2, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new ScenePart(b, ExitType.Left, exitOffset: -1, scene),
                b => new ScenePart(b, ScenePartType.Pit, 10, 3, scene),
                b => new ScenePart(b, ScenePartType.Pit, 22, 3, scene),
                b => new ScenePart(b, ScenePartType.Pit, 29, 10, scene),
                b => new ScenePart(b, ScenePartType.EnemyType1, 45, 9, scene),
                b => new ScenePart(b, ScenePartType.Platform_LeftRight, 33, 12, scene),
                b => new ScenePart(b, ScenePartType.Pit, 48, 10, scene),
                b => new ScenePart(b, ScenePartType.Platform_LeftRight, 52, 12, scene),
                b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 10, y: 8, definition: scene),
                b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 12, y: 8, definition: scene),
                b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 22, y: 8, definition: scene),
                b => new ScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 48, y: 12, definition: scene),
                b => new ScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 50, y: 12, definition: scene),
                b => new ScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 52, y: 12, definition: scene),
                b => new ScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 54, y: 12, definition: scene),
                b => new ScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 56, y: 12, definition: scene),
               // b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 58, y: 12, definition: scene),
                b => new ScenePart(b, ScenePartType.Button, x: 60, y: 11, definition: scene)
                );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_10_Stair, builder.Memory, specs);
                header = new ScenePartsHeader(builder,
                    b => new ScenePart(b, ExitType.Right, exitOffset: 1, scene));
                destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_11_Boss, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new ScenePart(b, ScenePartType.EnemyType1, 12, 9, scene),
                b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 2, y: 11, definition: scene),
                b => new ScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 4, y: 11, definition: scene),
                b => new ScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 6, y: 11, definition: scene),
                b => new ScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 8, y: 11, definition: scene),
                b => new ScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 10, y: 11, definition: scene),
                b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 12, y: 11, definition: scene),
                b => new ScenePart(b, ExitType.Left, exitOffset: -1, scene),
                b => new ScenePart(b, ExitType.Right, exitOffset: 1, scene));

            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_12_Horizontal2, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                  b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 12, y: 10, definition: scene),
                  b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 12, y: 8, definition: scene),
                  b => new ScenePart(b, ScenePartType.EnemyType2, 12, 9, scene),
                  b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 28, y: 10, definition: scene),
                  b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 28, y: 8, definition: scene),
                  b => new ScenePart(b, ScenePartType.EnemyType1, 20, 9, scene),
                  b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 36, y: 10, definition: scene),
                  b => new ScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 36, y: 8, definition: scene),
                  b => new ScenePart(b, ScenePartType.EnemyType1, 50, 8, scene)




              );

            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

        }
    }
}
