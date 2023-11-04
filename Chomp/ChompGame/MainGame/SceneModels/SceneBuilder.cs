using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels.SceneParts;
using ChompGame.MainGame.SpriteControllers;

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
        Level1_12_Horizontal2,
        Level1_13_Column,
        Level1_14_BigRoom,
        Level1_15_Vertical2,
        Level1_16_BeforeBoss,
        Level1_17_Boss,
        Level2_1_Intro,
        Level2_2_Fly,
        Level2_3_Beach
    }

    class SceneBuilder
    {
        public static void AddSceneHeaders(SystemMemoryBuilder memoryBuilder, Specs specs)
        {
            //Level1_1_Start
            SceneDefinition.NoScrollFlat(
               specs: specs,
               theme: ThemeType.Plains,
               enemyGroup: EnemyGroup.Lizard_Bird,
               memoryBuilder: memoryBuilder,
               top: 0,
               bottom: 1,
               left: 1,
               right: 0,
               bgPosition: 1
            );

            //Level1_2_Horizontal,
            SceneDefinition.HorizontalScroll(
                specs: specs,
                variance: LevelShape.MediumVariance,
                theme: ThemeType.Plains,
                enemyGroup: EnemyGroup.Lizard_Bird,
                memoryBuilder: memoryBuilder,
                top: 0,
                bottom: 2,
                bgPosition1: 2
            );

            //Level1_3_Pit,
            SceneDefinition.NoScrollCornerStairs(
                specs: specs,
                theme: ThemeType.Plains,
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
              theme: ThemeType.Plains,
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
              shape: LevelShape.ZigZag,
              theme: ThemeType.Plains,
              enemyGroup: EnemyGroup.Lizard_Bird,
              memoryBuilder: memoryBuilder,
              left: 1,
              right: 1
          );

            //Level1_6_Platforms,
            SceneDefinition.HorizontalScroll(
               specs: specs,
               variance: LevelShape.HighVariance,
               theme: ThemeType.Plains,
               enemyGroup: EnemyGroup.Lizard_Bird,
               memoryBuilder: memoryBuilder,
               top: 0,
               bottom: 2,
               bgPosition1: 1
           );

            //Level1_7_Door,
            SceneDefinition.NoScrollFlat(
               specs: specs,
               theme: ThemeType.Plains,
               enemyGroup: EnemyGroup.Lizard_Bird,
               memoryBuilder: memoryBuilder,
               top: 1,
               bottom: 1,
               left: 1,
               right: 1,
               bgPosition: 0
            );

            //Level1_8_Door2,
            SceneDefinition.NoScrollCornerStairs(
               specs: specs,
               theme: ThemeType.Plains,
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
              theme: ThemeType.Plains,
              enemyGroup: EnemyGroup.Lizard_Bird,
              memoryBuilder: memoryBuilder,
              top: 0,
              bottom: 2,
              bgPosition1: 2
          );

            //Level1_10_Stair,
            SceneDefinition.NoScrollBigStairs(
             specs: specs,
             theme: ThemeType.Plains,
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
                    theme: ThemeType.Plains,
                    enemyGroup: EnemyGroup.MidBoss,
                    memoryBuilder: memoryBuilder,
                    top: 1,
                    left: 0,
                    right: 0,
                    bottom: 1,
                    bgPosition: 0);

            //Level1_12_Horizontal2,
            SceneDefinition.HorizontalScroll(
                specs: specs,
                variance: LevelShape.HighVariance,
                theme: ThemeType.PlainsEvening,
                enemyGroup: EnemyGroup.Lizard_Bird,
                memoryBuilder: memoryBuilder,
                top: 0,
                bottom: 1,
                bgPosition1: 1
            );

            //Level1_13_Columns
            SceneDefinition.HorizontalScroll(
                specs: specs,
                variance: LevelShape.Flat,
                theme: ThemeType.PlainsEvening,
                enemyGroup: EnemyGroup.Lizard_Bird,
                memoryBuilder: memoryBuilder,
                top: 0,
                bottom: 1,
                bgPosition1: 0
            );

            //Level1_14_BigRoom
            SceneDefinition.NametableScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.PlainsEvening,
                enemyGroup: EnemyGroup.Lizard_Bird,
                shape: LevelShape.Flat,
                top: 0,
                bottom: 2,
                left: 0,
                right: 1);

            //Level1_15_Vertical2
            SceneDefinition.VerticalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.PlainsEvening,
                enemyGroup: EnemyGroup.Lizard_Bird,
                shape: LevelShape.ZigZag,
                left: 2,
                right: 2);

            //Level1_16_BeforeBoss
            SceneDefinition.NoScrollCornerStairs(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.PlainsEvening,
                enemyGroup: EnemyGroup.Lizard_Bird,
                cornerStairStyle: CornerStairStyle.OneBlockDouble,
                left:1,
                top:1,
                right:1,
                bottom:1,
                bgPosition:0);

            //Level1_17_Boss
            SceneDefinition.BossScene(memoryBuilder, specs, ThemeType.PlainsBoss);

            //Level2_1_Intro,
            SceneDefinition.HorizontalScroll(
                specs: specs,
                theme: ThemeType.Ocean,
                variance: LevelShape.Flat,
                enemyGroup: EnemyGroup.PlaneTakeoff,
                memoryBuilder: memoryBuilder,
                top: 0,
                bottom: 1,
                bgPosition1: 3);
            
            //Level2_2_Flying,
            SceneDefinition.HorizontalScroll(
                specs: specs,
                variance: LevelShape.Flat,
                theme: ThemeType.OceanAutoscroll,
                enemyGroup: EnemyGroup.Rocket_Bird,
                memoryBuilder: memoryBuilder,
                top: 0,
                bottom: 0,
                bgPosition1: 3
            );

            //Level2_3_Beach,
            SceneDefinition.HorizontalScroll(
                specs: specs,
                theme: ThemeType.Ocean,
                variance: LevelShape.Flat,
                enemyGroup: EnemyGroup.Lizard_Bird,
                memoryBuilder: memoryBuilder,
                top: 0,
                bottom: 1,
                bgPosition1: 3);
        }

        public static void AddSceneParts(SystemMemoryBuilder builder, Specs specs)
        {
            int destroyBitsNeeded = 0;

            SceneDefinition scene = new SceneDefinition(Level.Level1_1_Start, builder.Memory, specs);
            var header = new ScenePartsHeader(builder,
                b => new ExitScenePart(b, ExitType.Right, 1, scene),
                b => new SpriteScenePart(b, ScenePartType.Bomb, 8, 12, scene),
                b => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 12, 6, scene),
                b => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 12, 8, scene),
                b => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 12, 10, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: true, x: 4, y: 8, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 6, y: 8, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: false, x: 8, y: 8, definition: scene)
                );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_2_Horizontal, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: false, x: 8, y: 8, definition: scene)
                ,b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: false, x: 10, y: 10, definition: scene)
                ,b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: false, x: 12, y: 8, definition: scene)
                ,b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 18, y: 8, definition: scene)
                ,b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 22, y: 8, definition: scene)
                ,b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: false, x: 36, y: 8, definition: scene)
                ,b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: false, x: 38, y: 8, definition: scene)
                ,b => new ExitScenePart(b, ExitType.Left, -1, scene)
                ,b => new ExitScenePart(b, ExitType.Right, 1, scene)
                ,b => new SpriteScenePart(b, ScenePartType.Bomb, 47, 9, scene)
                ,b => new SpriteScenePart(b, ScenePartType.EnemyType1, 12, 12, scene)
                ,b => new SpriteScenePart(b, ScenePartType.EnemyType1, 33, 8, scene)
                ,b => new SpriteScenePart(b, ScenePartType.EnemyType1, 58, 12, scene)
                );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_3_Pit, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new ExitScenePart(b, ExitType.Left, -1, scene),
                b => new ExitScenePart(b, ExitType.Right, 1, scene),
                b => new ExitScenePart(b, ExitType.Bottom, 2, scene),
                b => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true,true,false,false,8,10, scene),
                b => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, false, false, 6, 10, scene)
                );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_4_DeadEnd, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new ExitScenePart(b, ExitType.Left, -1, scene)
                , b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 4, y: 8, definition: scene)
                , b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 6, y: 8, definition: scene)
                , b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 8, y: 8, definition: scene)
                , b => new SpriteScenePart(b, ScenePartType.Bomb, 11, 9, scene)
                );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_5_Vertical, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new SpriteScenePart(b, ScenePartType.Bomb, 6, 19, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType2, 4, 6, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType2, 8, 34, scene),
                b => new ExitScenePart(b, ExitType.Right, exitOffset:5, scene),
                b => new SpriteScenePart(b, ScenePartType.DoorFowardExit, 3, 36, scene),
                b => new PrefabScenePart(b, scene, position:36, PrefabSize.Full, PrefabSize.Full, PrefabOrigin.BottomOrLeft, PrefabShape.Block),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 2, y: 4, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 2, y: 6, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 12, y: 34, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 12, y: 32, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 14, y: 34, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 14, y: 32, definition: scene)

                );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_6_Platforms, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new SpriteScenePart(b, ScenePartType.DoorBackExit, 60, 11, scene),
                b => new ExitScenePart(b, ExitType.Left, exitOffset: 1, scene),
                b => new PitScenePart(b, 30,10, scene),
                b => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len24, 34, 8, scene),
                b => new PlatformScenePart(b, ScenePartType.Platform_LeftRight, PlatformDistance.Len24, 10, 12, scene),
                b => new PitScenePart(b, 10, 10, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType1, 48, 8, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType2, 17, 6, scene),
                b => new SpriteScenePart(b, ScenePartType.Bomb, 28,12, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: false, bottomLeft: true, bottomRight: false, x: 50, y: 10, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: false, bottomLeft: true, bottomRight: false, x: 50, y: 8, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: false, bottomLeft: true, bottomRight: false, x: 52, y: 10, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: false, bottomLeft: true, bottomRight: false, x: 52, y: 8, definition: scene)

                );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_7_Door, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new SpriteScenePart(b, ScenePartType.DoorFowardExit, 4, 12, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 6, y: 8, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 8, y: 8, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 10, y: 8, definition: scene),
                b => new ExitScenePart(b, ExitType.Right, exitOffset: -1, scene)
                );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_8_Door2, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new SpriteScenePart(b, ScenePartType.DoorBackExit, 8, 12, scene),
                b => new ExitScenePart(b, ExitType.Right, exitOffset: 1, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType1, 12, 7, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType2, 4, 7, scene)
                );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_9_Platforms2, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new ExitScenePart(b, ExitType.Left, exitOffset: -1, scene),
                b => new PitScenePart(b, 10, 3, scene),
                b => new PitScenePart(b, 22, 3, scene),
                b => new PitScenePart(b, 29, 10, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType1, 45, 9, scene),
                b => new PlatformScenePart(b, ScenePartType.Platform_LeftRight, PlatformDistance.Len16, 30, 12, scene),
                b => new PitScenePart(b, 48, 10, scene),
                b => new PlatformScenePart(b, ScenePartType.Platform_LeftRight, PlatformDistance.Len24, 48, 12, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 10, y: 8, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 12, y: 8, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 22, y: 8, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 48, y: 12, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 50, y: 12, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 52, y: 12, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 54, y: 12, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 56, y: 12, definition: scene),
                b => new SpriteScenePart(b, ScenePartType.Button, x: 60, y: 11, definition: scene)
                );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_10_Stair, builder.Memory, specs);
                header = new ScenePartsHeader(builder,
                    b => new ExitScenePart(b, ExitType.Right, exitOffset: 1, scene));
                destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_11_Boss, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new SpriteScenePart(b, ScenePartType.EnemyType1, 12, 9, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 2, y: 11, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 4, y: 11, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 6, y: 11, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 8, y: 11, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 10, y: 11, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 12, y: 11, definition: scene),
                b => new ExitScenePart(b, ExitType.Left, exitOffset: -1, scene),
                b => new ExitScenePart(b, ExitType.Right, exitOffset: 1, scene));

            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_12_Horizontal2, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 12, y: 10, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 12, y: 8, definition: scene),
                  b => new SpriteScenePart(b, ScenePartType.EnemyType2, 12, 9, scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 28, y: 10, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 28, y: 8, definition: scene),
                  b => new SpriteScenePart(b, ScenePartType.EnemyType1, 20, 9, scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 36, y: 10, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 36, y: 8, definition: scene),
                  b => new SpriteScenePart(b, ScenePartType.EnemyType1, 50, 8, scene),
                  b => new ExitScenePart(b, ExitType.Right, exitOffset: 1, scene)
              );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_13_Column, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                  b => new ExitScenePart(b, ExitType.Left, exitOffset: -1, scene),
                  b => new ExitScenePart(b, ExitType.Right, exitOffset: 1, scene),
                  b => new PrefabScenePart(b, scene, 14, PrefabSize.Full, PrefabSize.ThreeQuarter, PrefabOrigin.BottomOrLeft, PrefabShape.Block),
                  b => new PlatformScenePart(b, ScenePartType.Platform_UpDown,PlatformDistance.Len16, 12, 8,  scene),
                  b => new SpriteScenePart(b, ScenePartType.EnemyType1, 16, 4, scene),
                  b => new SpriteScenePart(b, ScenePartType.EnemyType2, 42, 8, scene),
                  b => new SpriteScenePart(b, ScenePartType.Bomb, 30, 8, scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: true, bottomRight: true, x: 18, y: 4, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: true, bottomRight: true, x: 20, y: 4, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: true, bottomRight: true, x: 22, y: 4, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: true, bottomRight: true, x: 24, y: 4, definition: scene),

                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 54, y: 9, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 54, y: 11, definition: scene),

                  b => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 36, y: 11, definition: scene),

                  b => new PrefabScenePart(b, scene, 50, PrefabSize.Quarter, PrefabSize.Full, PrefabOrigin.BottomOrLeft, PrefabShape.Block),
                  b => new PitScenePart(b, 40,8, scene),
                  b => new PlatformScenePart(b, ScenePartType.Platform_LeftRight, PlatformDistance.Len24, 40, 8, scene)
              );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_14_BigRoom, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                  b => new ExitScenePart(b, ExitType.Left, exitOffset: -1, scene),
                  b => new PrefabScenePart(b, scene, 12, PrefabSize.Full, PrefabSize.Quarter, PrefabOrigin.BottomOrLeft, PrefabShape.StairLeft),
                  b => new PrefabScenePart(b, scene, 28, PrefabSize.Quarter, PrefabSize.ThreeQuarter, PrefabOrigin.BottomOrLeft, PrefabShape.Block),
                  b => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len16, 20, 13, scene),
                  b => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len32, 24, 14, scene),
                  b => new SpriteScenePart(b, ScenePartType.EnemyType1, 16, 16, scene),
                  b => new SpriteScenePart(b, ScenePartType.EnemyType1, 24, 16, scene),
                  b => new SpriteScenePart(b, ScenePartType.DoorFowardExit, 28, 8, scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 18, y: 16, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: true, bottomRight: true, x: 18, y: 14, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: true, bottomLeft: false, bottomRight: true, x: 16, y: 16, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: false, bottomLeft: true, bottomRight: false, x: 20, y: 16, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: false, x: 18, y: 18, definition: scene)
                  );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_15_Vertical2, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                  b => new SpriteScenePart(b, ScenePartType.DoorBackExit, 10, 44, scene),
                  b => new PrefabScenePart(b, scene, 44, PrefabSize.Full, PrefabSize.Full, PrefabOrigin.BottomOrLeft, PrefabShape.Block),
                  b => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 4, y: 42, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, topLeft: true, topRight: false, bottomLeft: true, bottomRight: false, x: 4, y: 40, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 10, y: 32, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 10, y: 34, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, topLeft: false, topRight: true, bottomLeft: false, bottomRight: true, x: 10, y: 30, definition: scene),
                  b => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len24, 4, 21, scene),             
                  b => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len16, 10, 12, scene),
                  b => new SpriteScenePart(b, ScenePartType.EnemyType1, 4, 21, scene),

                  b => new SpriteScenePart(b, ScenePartType.DoorFowardExit, 4, 9, scene)
                  );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_16_BeforeBoss, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                  b => new SpriteScenePart(b, ScenePartType.DoorBackExit, 8, 12, scene),
                  b => new ExitScenePart(b, ExitType.Right, 1, scene)
                  );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level1_17_Boss, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new SpriteScenePart(b, ScenePartType.Bomb, x: 12, y: 16, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 0, y: 0, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 2, y: 0, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 4, y: 0, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 6, y: 0, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 8, y: 0, definition: scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType1, x: 18, y: 16, definition: scene)
            );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level2_1_Intro, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                 b => new SpriteScenePart(b, ScenePartType.EnemyType1, x: 15, y:12, definition: scene),
                 b => new PitScenePart(b, 18,15, scene),
                 b => new PitScenePart(b, 33, 15, scene),
                 b => new ExitScenePart(b, ExitType.Right, 1, scene)
            );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

            scene = new SceneDefinition(Level.Level2_2_Fly, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                //b => new AutoscrollScenePart(b, ScenePartType.Coin, position: 4, delay: 2, variation: PrizeController.Coin3, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.Coin, position: 12, delay: 6, variation: PrizeController.Coin3, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.Coin, position: 8, delay: 6, variation: PrizeController.Coin5Diag, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.EnemyType2, position: 6, delay: 8, variation: 2, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.EnemyType2, position: 6, delay: 8, variation: 2, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.EnemyType2, position: 6, delay: 8, variation: 2, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.EnemyType2, position: 6, delay: 8, variation: 2, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.EnemyType1, position: 4, delay: 8, variation: 0, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.EnemyType1, position: 12, delay: 8, variation: 0, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.EnemyType2, position: 6, delay: 8, variation: 1, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.EnemyType2, position: 9, delay: 4, variation: 1, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.Coin, position: 8, delay: 0, variation: PrizeController.Coin5Diag2, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.EnemyType1, position: 8, delay: 15, variation: 1, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.Coin, position: 4, delay: 4, variation: PrizeController.Coin3, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.Coin, position: 12, delay: 0, variation: PrizeController.Coin3, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.EnemyType1, position: 4, delay: 8, variation: 0, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.EnemyType1, position: 12, delay: 8, variation: 0, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.EnemyType2, position: 10, delay: 8, variation: 1, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.Coin, position: 2, delay: 6, variation: PrizeController.Coin5Diag, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.EnemyType1, position: 6, delay: 4, variation: 1, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.EnemyType1, position: 14, delay: 4, variation: 1, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.Coin, position: 8, delay: 8, variation: PrizeController.Coin5Diag2, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.EnemyType1, position: 4, delay: 8, variation: 0, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.EnemyType1, position: 12, delay: 8, variation: 0, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.Coin, position: 4, delay: 6, variation: PrizeController.Coin3, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.Coin, position: 12, delay: 0, variation: PrizeController.Coin3, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.Coin, position: 8, delay: 8, variation: PrizeController.Coin5Diag2, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.Coin, position: 8, delay: 8, variation: PrizeController.Coin5Diag, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.EnemyType1, position: 8, delay: 6, variation: 1, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.EnemyType1, position: 8, delay: 6, variation: 1, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.EnemyType2, position: 9, delay: 8, variation: 1, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.EnemyType2, position: 9, delay: 8, variation: 1, scene),
                //b => new AutoscrollScenePart(b, ScenePartType.EnemyType2, position: 9, delay: 8, variation: 1, scene),
                b => new AutoscrollScenePart(b, ScenePartType.AutoScrollEnemyType3, position: 9, delay: 15, variation: 1, scene)
            );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);


            scene = new SceneDefinition(Level.Level2_3_Beach, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                 b => new SpriteScenePart(b, ScenePartType.EnemyType1, x: 15, y: 12, definition: scene),                 
                 b => new ExitScenePart(b, ExitType.Right, 1, scene)
            );
            destroyBitsNeeded += header.DestroyBitsNeeded(scene, builder.Specs);

        }
    }
}
