using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels.SceneParts;
using ChompGame.MainGame.SpriteControllers;
using ChompGame.MainGame.SpriteModels;
using System;
using System.Linq;

namespace ChompGame.MainGame.SceneModels
{
    public enum Level : byte
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
        Level2_2_Fly2,
        Level2_3_Beach,
        Level2_4_Beach2,
        Level2_5_Hub,
        Level2_6_After_Hub,
        Level2_7_Door,
        Level2_8_Platforms,
        Level2_9_Pit,
        Level2_10_Beach3,
        Level2_11_Beach4,
        Level2_12_Boss,
        Level3_1_City,
        Level3_2_Building1,
        Level3_3_Building1_Room1,
        Level3_4_Building1_Room2,
        Level3_5_City2,
        Level3_6_City3,
        Level3_7_Building2,
        Level3_8_Building2_Room1,
        Level3_9_Building2_Room2,
        Level3_10_Building3,
        Level3_11_Building3_Room1,
        Level3_12_Building3_Room2,
        Level3_13_Building3_Room3,
        Level3_14_Building3_Room4,
        Level3_15_Building3_Room5,
        Level3_16_Building3_Room6,
        Level3_17_Building3_Room7,
        Level3_18_Building3_Room8,
        Level3_19_Building3_Room9,
        Level3_20_Midboss,
        Level3_21_CityAfterMidboss,
        Level3_22_CityTrain,
        Level3_23_CityTrain2,
        Level3_24_CityTrain3,
        Level3_25_CityBeforeBoss,
        Level3_26_Boss,
        Level4_1_Desert,
        Level4_2_Desert2,
        Level4_3_MageIntro,
        Level4_4_Bonus,
        Level4_5_Desert3,
        Level4_6_PyramidEntrance,
        Level4_7_Pit,
        Level4_8_Chamber,
        Level4_9_Chamber2,
        Level4_10_SwitchRoom,
        Level4_11_DeadEnd,
        Level4_12_DeadEnd2,
        Level4_13_PyramidExit,
        Level4_14_DeadEnd3,
        Level4_15_Steps,
        Level4_16_PyramidEntrance2,
        Level4_17_Before_Midboss,
        Level4_18_Pit2,
        Level4_19_Chamber3,
        Level4_20_DeadEnd4,
        Level4_21_Chamber4,
        Level4_22_Chamber5,
        Level4_23_Chamber6,
        Level4_24_BeforeMidboss2,
        Level4_25_DeadEnd5,
        Level4_26_Pit3,
        Level4_27_Chamber8,
        Level4_28_SwitchRoom2,
        Level4_29_DeadEnd6,
        Level4_30_Chamber7,
        Level4_31_Midboss,
        Level4_32_Desert4,
        Level4_33_Desert5,
        Level4_34_DesertRain1,
        Level4_35_DesertRain2,
        Level4_36_DesertRain3,
        Level4_37_DesertRain4,
        Level4_38_DesertRain5,
        Level4_39_BeforeBoss,
        Level4_40_Boss,
        Level5_1_Mist,
        Level5_2_Bridges,
        Level5_3_Bridges2,
        Level5_4_Mist2,
        Level5_5_Forest,
        Level5_6_Forest_Hub,
        Level5_7_Forest2,
        Level5_8_Forest2,
        Level5_9_Forest_Corner,
        Level5_10_Forest3,
        Level5_11_Forest_Path,
        Level5_12_Forest_Hub2,
        Level5_13_Forest_Corner2,
        Level5_14_Forest4,
        Level5_15_Forest_Corner3,
        Level5_16_Forest_Corner4,
        Level5_17_Forest5,
        Level5_18_Forest_Hub3,
        Level5_19_Forest6,
        Level5_20_Mist3,
        Level5_21_Mist4,
        Level5_22_MidBoss
    }

    class SceneBuilder
    {
        public static Level[] TransitionLevels = {
            Level.Level1_1_Start,
            Level.Level1_5_Vertical,
            Level.Level1_10_Stair,
            Level.Level1_17_Boss,
            Level.Level2_1_Intro,
            Level.Level2_3_Beach,
            Level.Level3_1_City,
            Level.Level3_7_Building2,
            Level.Level3_13_Building3_Room3,
            Level.Level3_21_CityAfterMidboss,
            Level.Level3_23_CityTrain2,
            Level.Level3_24_CityTrain3,
            Level.Level4_1_Desert,
            Level.Level4_16_PyramidEntrance2,
            Level.Level4_31_Midboss,
            Level.Level4_34_DesertRain1,
            Level.Level5_1_Mist
        };

        public static void AddSceneHeaders(SystemMemoryBuilder memoryBuilder, Specs specs)
        {
            Level _;

            _ = Level.Level1_1_Start;
            SceneDefinition.NoScrollFlat(
               specs: specs,
               theme: ThemeType.Plains,
               enemy1: EnemyIndex.Lizard,
               enemy2: EnemyIndex.Bird,
               spriteGroup: SpriteGroup.Normal,
               memoryBuilder: memoryBuilder,
               top: 0,
               bottom: 1,
               left: 1,
               right: 0,
               upper: 1,
               mid: 1,
               lower: 1
            ); ;

            //Level1_2_Horizontal,
            SceneDefinition.HorizontalScroll(
                specs: specs,
                variance: LevelShape.LowVariance,
                theme: ThemeType.Plains,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                spriteGroup: SpriteGroup.Normal,
                memoryBuilder: memoryBuilder,
                top: 0,
                bottom: 2,
                upper: 2
            );

            //Level1_3_Pit,
            SceneDefinition.NoScrollCornerStairs(
                specs: specs,
                theme: ThemeType.Plains,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                spriteGroup: SpriteGroup.Normal,
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
              enemy1: EnemyIndex.Lizard,
              enemy2: EnemyIndex.Bird,
              spriteGroup: SpriteGroup.Normal,
              memoryBuilder: memoryBuilder,
              top: 0,
              bottom: 2,
              left: 0,
              right: 2,
              upper: 2
           );

            //Level1_5_Vertical,
            SceneDefinition.VerticalScroll(
              specs: specs,
              shape: LevelShape.ZigZag,
              theme: ThemeType.Plains,
              enemy1: EnemyIndex.Lizard,
              enemy2: EnemyIndex.Bird,
              spriteGroup: SpriteGroup.Normal,
              memoryBuilder: memoryBuilder,
              left: 1,
              right: 1
          );

            _ = Level.Level1_6_Platforms;
            SceneDefinition.HorizontalScroll(
               specs: specs,
               variance: LevelShape.HighVariance,
               theme: ThemeType.Plains,
               enemy1: EnemyIndex.Lizard,
               enemy2: EnemyIndex.Bird,
               spriteGroup: SpriteGroup.Normal,
               memoryBuilder: memoryBuilder,
               top: 0,
               bottom: 2,
               upper: 1
           );

            //Level1_7_Door,
            SceneDefinition.NoScrollFlat(
               specs: specs,
               theme: ThemeType.Plains,
               enemy1: EnemyIndex.Lizard,
               enemy2: EnemyIndex.Bird,
               spriteGroup: SpriteGroup.Normal,
               memoryBuilder: memoryBuilder,
               top: 1,
               bottom: 1,
               left: 1,
               right: 1,
               upper: 0
            );

            //Level1_8_Door2,
            SceneDefinition.NoScrollCornerStairs(
               specs: specs,
               theme: ThemeType.Plains,
               enemy1: EnemyIndex.Lizard,
               enemy2: EnemyIndex.Bird,
               spriteGroup: SpriteGroup.Normal,
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
              enemy1: EnemyIndex.Lizard,
              enemy2: EnemyIndex.Bird,
              spriteGroup: SpriteGroup.Normal,
              memoryBuilder: memoryBuilder,
              top: 0,
              bottom: 2,
              upper: 2
          );

            _ = Level.Level1_10_Stair;
            SceneDefinition.NoScrollBigStairs(
             specs: specs,
             theme: ThemeType.Plains,
             enemy1: EnemyIndex.Lizard,
             enemy2: EnemyIndex.Bird,
             spriteGroup: SpriteGroup.Normal,
             memoryBuilder: memoryBuilder,
             top: 0,
             bottom: 2,
             left: 0,
             right: 0,
             bgPosition: 0
          );

            //Level1_11_Boss
            SceneDefinition.NoScrollFlat(
                    specs: specs,
                    theme: ThemeType.Plains,
                    enemy1: EnemyIndex.Midboss,
                    enemy2: EnemyIndex.Midboss,
                    spriteGroup: SpriteGroup.Boss,
                    memoryBuilder: memoryBuilder,
                    top: 1,
                    left: 0,
                    right: 0,
                    bottom: 1,
                    upper: 0);

            //Level1_12_Horizontal2,
            SceneDefinition.HorizontalScroll(
                specs: specs,
                variance: LevelShape.LowVariance,
                theme: ThemeType.PlainsEvening,
                enemy1: EnemyIndex.Lizard,
                enemy2:EnemyIndex.Bird,
                spriteGroup: SpriteGroup.Normal,
                memoryBuilder: memoryBuilder,
                top: 0,
                bottom: 1,
                upper: 1
            );

            _ = Level.Level1_13_Column;
            SceneDefinition.HorizontalScroll(
                specs: specs,
                variance: LevelShape.Flat,
                theme: ThemeType.PlainsEvening,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                spriteGroup: SpriteGroup.Normal,
                memoryBuilder: memoryBuilder,
                top: 0,
                bottom: 1,
                upper: 0
            );

            //Level1_14_BigRoom
            SceneDefinition.NametableScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.PlainsEvening,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                spriteGroup: SpriteGroup.Normal,
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
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                spriteGroup: SpriteGroup.Normal,
                shape: LevelShape.ZigZag,
                left: 2,
                right: 2);

            _ = Level.Level1_16_BeforeBoss;
            SceneDefinition.NoScrollCornerStairs(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.PlainsEvening,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                spriteGroup: SpriteGroup.Normal,
                cornerStairStyle: CornerStairStyle.OneBlockDouble,
                left:1,
                top:1,
                right:1,
                bottom:1,
                bgPosition:0);

            //Level1_17_Boss
            SceneDefinition.BossScene(memoryBuilder, specs, ThemeType.PlainsBoss);

            _ = Level.Level2_1_Intro;
            SceneDefinition.HorizontalScroll(
                specs: specs,
                theme: ThemeType.Ocean,
                variance: LevelShape.Flat,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                spriteGroup: SpriteGroup.PlaneTakeoff,
                memoryBuilder: memoryBuilder,
                top: 0,
                bottom: 1,
                upper: 3);

            _ = Level.Level2_2_Fly;
            SceneDefinition.HorizontalScroll(
                specs: specs,
                variance: LevelShape.Flat,
                theme: ThemeType.OceanAutoscroll,
                enemy1:EnemyIndex.Bird,
                enemy2: EnemyIndex.Rocket,
                spriteGroup: SpriteGroup.Plane,
                memoryBuilder: memoryBuilder,
                top: 0,
                bottom: 0,
                upper: 3
            );

            _ = Level.Level2_2_Fly2;
            SceneDefinition.HorizontalScroll(
                specs: specs,
                variance: LevelShape.Flat,
                theme: ThemeType.OceanAutoscroll,
                enemy1: EnemyIndex.Midboss,
                enemy2: EnemyIndex.Midboss,
                spriteGroup: SpriteGroup.Plane,
                memoryBuilder: memoryBuilder,
                top: 0,
                bottom: 0,
                upper: 3
            );

            //Level2_3_Beach,
            SceneDefinition.HorizontalScroll(
                specs: specs,
                theme: ThemeType.Ocean,
                variance: LevelShape.LowVariance,
                enemy1: EnemyIndex.Bird,
                enemy2: EnemyIndex.Crocodile,
                spriteGroup: SpriteGroup.Normal,
                memoryBuilder: memoryBuilder,
                top: 0,
                bottom: 3,
                upper: 1);

            //Level2_4_Beach2,
            SceneDefinition.HorizontalScroll(
                specs: specs,
                theme: ThemeType.Ocean,
                variance: LevelShape.Flat,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Crocodile,
                spriteGroup: SpriteGroup.Normal,
                memoryBuilder: memoryBuilder,
                top: 0,
                bottom: 1,
                upper: 1);

            //Level2_5_Hub
            SceneDefinition.NametableScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.Ocean,
                shape: LevelShape.Flat,
                enemy1: EnemyIndex.Bird,
                enemy2: EnemyIndex.Crocodile,
                spriteGroup: SpriteGroup.Normal,
                top: 0,
                left: 0,
                right: 0,
                bottom: 2);

            //Level2_6_After_Hub,
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.Ocean,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Bird,
                enemy2: EnemyIndex.Crocodile,
                top: 0,
                bottom: 2,
                right: 0,
                left: 0,
                upper: 1);

            //Level2_7_Door,
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.Ocean,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Bird,
                enemy2: EnemyIndex.Crocodile,
                top: 0,
                bottom: 2,
                right: 0,
                left: 0,
                upper: 1);

            //Level2_8_Platforms,
            SceneDefinition.HorizontalScroll(
                specs: specs,
                theme: ThemeType.Ocean,
                variance: LevelShape.Flat,
                enemy1: EnemyIndex.Bird,
                enemy2: EnemyIndex.Crocodile,
                spriteGroup: SpriteGroup.Normal,
                memoryBuilder: memoryBuilder,
                top: 0,
                bottom: 2,
                upper: 1);

            _ = Level.Level2_9_Pit;
            SceneDefinition.NoScrollTShape(
                specs: specs,
                theme: ThemeType.Ocean,
                enemy1: EnemyIndex.Bird,
                enemy2: EnemyIndex.Crocodile,
                spriteGroup: SpriteGroup.Normal,
                memoryBuilder: memoryBuilder,
                leftY: 0,
                rightY: 0,
                pitX: 1,
                hallSize: 2,
                bgPosition: 1
            );

            //Level2_10_Beach3,
            SceneDefinition.HorizontalScroll(
                specs: specs,
                theme: ThemeType.Ocean,
                variance: LevelShape.LowVariance,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Crocodile,
                spriteGroup: SpriteGroup.Normal,
                memoryBuilder: memoryBuilder,
                top: 0,
                bottom: 1,
                upper: 1);

            //Level2_11_Beach4,
            SceneDefinition.HorizontalScroll(
                specs: specs,
                theme: ThemeType.Ocean,
                variance: LevelShape.LowVariance,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Crocodile,
                spriteGroup: SpriteGroup.Normal,
                memoryBuilder: memoryBuilder,
                top: 0,
                bottom: 3,
                upper: 1);

            //Level2_12_Boss
            SceneDefinition.BossScene(memoryBuilder, specs, ThemeType.OceanBoss);

            //Level3_1_City
            SceneDefinition.HorizontalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                variance: LevelShape.TwoByTwoVariance,
                theme: ThemeType.City,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                bottom: 2,
                top: 0,
                upper: 1);

            //Level3_2_Building1
            SceneDefinition.VerticalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                shape: LevelShape.Ladder,
                theme: ThemeType.CityInterior,
                spriteGroup: SpriteGroup.Normal,
                left: 2,
                right: 2,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird);

            // Level3_3_Building1_Room1,
            SceneDefinition.NoScrollTShape(
                 memoryBuilder: memoryBuilder,
                 specs: specs,
                 theme: ThemeType.CityInterior,
                 spriteGroup: SpriteGroup.Normal,
                 leftY:0,
                 rightY:2,
                 pitX:1,
                 hallSize:1,
                 bgPosition:0,
                 enemy1: EnemyIndex.Lizard,
                 enemy2: EnemyIndex.Bird);

            //Level3_4_Building1_Room2,
            SceneDefinition.NametableScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.CityInterior,
                shape: LevelShape.TwoHorizontalChambers,
                spriteGroup: SpriteGroup.Normal,
                top: 2,
                bottom: 2,
                left: 2,
                right: 2,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird);

            //Level3_5_City2
            SceneDefinition.HorizontalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                variance: LevelShape.TwoByTwoVariance,
                theme: ThemeType.City,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Crocodile,
                enemy2: EnemyIndex.Ogre,
                bottom: 2,
                top: 0,
                upper: 1);

            
            _ = Level.Level3_6_City3;
            SceneDefinition.HorizontalScroll(
               memoryBuilder: memoryBuilder,
               specs: specs,
               variance: LevelShape.Flat,
               theme: ThemeType.City,
               spriteGroup: SpriteGroup.Normal,
               enemy1: EnemyIndex.Bird,
               enemy2: EnemyIndex.Ogre,
               bottom: 1,
               top: 0,
               upper: 1);

            _ = Level.Level3_7_Building2;
            SceneDefinition.VerticalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                shape: LevelShape.Ladder,
                theme: ThemeType.CityInterior,                
                spriteGroup: SpriteGroup.Normal,
                left: 2,
                right: 2,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird);

            _ = Level.Level3_8_Building2_Room1;
            SceneDefinition.NoScrollFlat(
               memoryBuilder: memoryBuilder,
               specs: specs,
               theme: ThemeType.CityInterior,
               spriteGroup: SpriteGroup.Normal,
               enemy1: EnemyIndex.Bird,
               enemy2: EnemyIndex.Ogre,
               top: 2,
               bottom: 1,
               right: 1,
               left: 2,
               upper: 1);

            _ = Level.Level3_9_Building2_Room2;
            SceneDefinition.NoScrollFlat(
               memoryBuilder: memoryBuilder,
               specs: specs,
               theme: ThemeType.CityInterior,
               spriteGroup: SpriteGroup.Normal,
               enemy1: EnemyIndex.Bird,
               enemy2: EnemyIndex.Ogre,
               top: 2,
               bottom: 2,
               right: 2,
               left: 2,
               upper: 1);

            _ = Level.Level3_10_Building3;
            SceneDefinition.VerticalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                shape: LevelShape.ZigZag,
                theme: ThemeType.CityInterior,
                spriteGroup: SpriteGroup.Normal,
                left: 2,
                right: 2,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird);

            _ = Level.Level3_11_Building3_Room1;
            SceneDefinition.NametableScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                spriteGroup: SpriteGroup.Normal,
                theme: ThemeType.CityInterior,
                shape: LevelShape.FourChambers,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Ogre,
                left: 2,
                top: 2,
                right: 2,
                bottom: 2);

            _ = Level.Level3_12_Building3_Room2;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.CityInterior,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                left: 1,
                top: 1,
                right: 1,
                bottom: 1,
                upper: 0);

            _ = Level.Level3_13_Building3_Room3;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.CityInterior,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                left: 1,
                top: 0,
                right: 1,
                bottom: 1,
                upper: 0);

            _ = Level.Level3_14_Building3_Room4;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.CityInterior,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                left: 1,
                top: 1,
                right: 1,
                bottom: 1,
                upper: 0);

            _ = Level.Level3_15_Building3_Room5;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.CityInterior,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Ogre,
                left: 1,
                top: 1,
                right: 1,
                bottom: 2,
                upper: 0);

            _ = Level.Level3_16_Building3_Room6;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.CityInterior,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                left: 1,
                top: 1,
                right: 1,
                bottom: 1,
                upper: 0);

            _ = Level.Level3_17_Building3_Room7;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.CityInterior,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                left: 1,
                top: 0,
                right: 1,
                bottom: 1,
                upper: 0);

            _ = Level.Level3_18_Building3_Room8;
            SceneDefinition.NoScrollTShape(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.CityInterior,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                leftY: 0,
                rightY: 2,
                pitX: 2,
                hallSize: 1,
                bgPosition: 0);

            _ = Level.Level3_19_Building3_Room9;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.CityInterior,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                left: 1,
                top: 0,
                right: 1,
                bottom: 1,
                upper: 0);

            _ = Level.Level3_20_Midboss;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.CityInterior,
                spriteGroup: SpriteGroup.Boss,
                enemy1: EnemyIndex.Midboss,
                enemy2: EnemyIndex.Bird,
                left: 1,
                top: 0,
                right: 1,
                bottom: 1,
                upper: 0);

            _ = Level.Level3_21_CityAfterMidboss;
            SceneDefinition.HorizontalScroll(
               memoryBuilder: memoryBuilder,
               specs: specs,
               variance: LevelShape.LowVariance,
               theme: ThemeType.CityEvening,
               spriteGroup: SpriteGroup.Normal,
               enemy1: EnemyIndex.Bird,
               enemy2: EnemyIndex.Ogre,
               bottom: 2,
               top: 0,
               upper: 1);

            _ = Level.Level3_22_CityTrain;
            SceneDefinition.HorizontalScroll(
               memoryBuilder: memoryBuilder,
               specs: specs,
               variance: LevelShape.Flat,
               theme: ThemeType.CityTrain,
               spriteGroup: SpriteGroup.Normal,
               enemy1: EnemyIndex.Lizard,
               enemy2: EnemyIndex.Rocket,
               bottom: 0,
               top: 0,
               upper: 1);

            _ = Level.Level3_23_CityTrain2;
            SceneDefinition.HorizontalScroll(
               memoryBuilder: memoryBuilder,
               specs: specs,
               variance: LevelShape.Flat,
               theme: ThemeType.CityTrain,
               spriteGroup: SpriteGroup.Normal,
               enemy1: EnemyIndex.Rocket,
               enemy2: EnemyIndex.Ogre,
               bottom: 0,
               top: 0,
               upper: 1);

            _ = Level.Level3_24_CityTrain3;
            SceneDefinition.HorizontalScroll(
               memoryBuilder: memoryBuilder,
               specs: specs,
               variance: LevelShape.Flat,
               theme: ThemeType.CityTrain,
               spriteGroup: SpriteGroup.Normal,
               enemy1: EnemyIndex.Bird,
               enemy2: EnemyIndex.Rocket,
               bottom: 0,
               top: 0,
               upper: 1);

            _ = Level.Level3_25_CityBeforeBoss;
            SceneDefinition.HorizontalScroll(
               memoryBuilder: memoryBuilder,
               specs: specs,
               variance: LevelShape.TwoByTwoVariance,
               theme: ThemeType.CityEvening,
               spriteGroup: SpriteGroup.Normal,
               enemy1: EnemyIndex.Bird,
               enemy2: EnemyIndex.Ogre,
               bottom: 2,
               top: 0,
               upper: 1);

            _ = Level.Level3_26_Boss;
            SceneDefinition.BossScene(memoryBuilder, specs, ThemeType.CityBoss);

            _ = Level.Level4_1_Desert;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.Desert,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Crocodile,
                left: 0,
                right: 0,
                top: 0,
                bottom: 1,
                upper: 2,
                mid: 2,
                lower: 1
                );

            _ = Level.Level4_2_Desert2;
            SceneDefinition.HorizontalScroll(
               memoryBuilder: memoryBuilder,
               specs: specs,
               variance: LevelShape.LowVariance,
               theme: ThemeType.Desert,
               spriteGroup: SpriteGroup.Normal,
               enemy1: EnemyIndex.Lizard,
               enemy2: EnemyIndex.Crocodile,
               bottom: 2,
               top: 0,
               upper: 1);

            _ = Level.Level4_3_MageIntro;
            SceneDefinition.NametableScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.Desert,
                shape: LevelShape.Flat,
                enemy1: EnemyIndex.Ogre,
                enemy2: EnemyIndex.Mage,
                spriteGroup: SpriteGroup.Normal,
                left: 0,
                right: 0,
                top: 0,
                bottom: 2);

            _ = Level.Level4_4_Bonus;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.Desert,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Crocodile,
                left: 1,
                right: 1,
                top: 0,
                bottom: 2,
                upper: 2);

            _ = Level.Level4_5_Desert3;
            SceneDefinition.HorizontalScroll(
               memoryBuilder: memoryBuilder,
               specs: specs,
               variance: LevelShape.LowVariance,
               theme: ThemeType.Desert,
               spriteGroup: SpriteGroup.Normal,
               enemy1: EnemyIndex.Crocodile,
               enemy2: EnemyIndex.Mage,
               bottom: 2,
               top: 0,
               upper: 1);

            _ = Level.Level4_6_PyramidEntrance;
            SceneDefinition.NametableScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.Desert,
                shape: LevelShape.Flat,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Crocodile,
                spriteGroup: SpriteGroup.Normal,
                left: 0,
                right: 0,
                top: 0,
                bottom: 2);

            _ = Level.Level4_7_Pit;
            SceneDefinition.VerticalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.DesertInterior,
                shape: LevelShape.Flat,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Crocodile,
                spriteGroup: SpriteGroup.Normal,
                left: 2,
                right: 2);

            _ = Level.Level4_8_Chamber;
            SceneDefinition.NoScrollCornerStairs(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.DesertInterior,
                cornerStairStyle: CornerStairStyle.TwoBlockDouble,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Crocodile,
                spriteGroup: SpriteGroup.Normal,
                bgPosition:0,
                top: 0,
                left: 1,
                bottom:1,
                right: 1);

            _ = Level.Level4_9_Chamber2;
            SceneDefinition.HorizontalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                variance: LevelShape.Flat,
                theme: ThemeType.DesertInterior,
                enemy1: EnemyIndex.Ogre,
                enemy2: EnemyIndex.Mage,
                spriteGroup: SpriteGroup.Normal,
                upper: 2,
                top: 1,
                bottom: 1,
                lower: 2,
                style: HorizontalScrollStyle.Interior);

            _ = Level.Level4_10_SwitchRoom;
            SceneDefinition.NoScrollBigStairs(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.DesertInterior,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Crocodile,
                spriteGroup: SpriteGroup.Normal,
                bgPosition: 0,
                top: 1,
                left: 1,
                bottom: 1,
                right: 1);

            _ = Level.Level4_11_DeadEnd;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.DesertInterior,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                spriteGroup: SpriteGroup.Normal,
                upper: 0,
                top: 1,
                left: 2,
                bottom: 1,
                right: 0);

            _ = Level.Level4_12_DeadEnd2;
            SceneDefinition.HorizontalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                variance: LevelShape.Flat,
                theme: ThemeType.DesertInterior,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                spriteGroup: SpriteGroup.Normal,
                upper: 1,
                top: 1,
                middle:2,
                bottom: 2,
                lower: 1,
                style: HorizontalScrollStyle.Interior);

            _ = Level.Level4_13_PyramidExit;
            SceneDefinition.HorizontalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                variance: LevelShape.HighVariance,
                theme: ThemeType.Desert,
                enemy1: EnemyIndex.Ogre,
                enemy2: EnemyIndex.Mage,
                spriteGroup: SpriteGroup.Normal,
                upper: 1,
                top: 0,
                bottom: 2,
                style: HorizontalScrollStyle.HorizonAndSky);

            _ = Level.Level4_14_DeadEnd3;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.Desert,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Crocodile,
                left: 1,
                right: 1,
                top: 0,
                bottom: 2,
                upper: 2);

            _ = Level.Level4_15_Steps;
            SceneDefinition.NoScrollBigStairs(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.Desert,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Crocodile,
                left: 1,
                right: 1,
                top: 0,
                bottom: 2,
                bgPosition: 2);

            _ = Level.Level4_16_PyramidEntrance2;
            SceneDefinition.NametableScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.Desert,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Crocodile,
                left: 0,
                right: 0,
                top: 0,
                bottom: 2,
                shape: LevelShape.Flat);

            _ = Level.Level4_17_Before_Midboss;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.Desert,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Crocodile,
                upper:0,
                left: 1,
                right: 1,
                bottom: 1,
                top: 1);

            _ = Level.Level4_18_Pit2;
            SceneDefinition.VerticalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.DesertInterior,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Ogre,
                enemy2: EnemyIndex.Mage,
                shape: LevelShape.ZigZag,
                left: 1,
                right: 1);

            _ = Level.Level4_19_Chamber3;
            SceneDefinition.NoScrollTShape(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.DesertInterior,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Crocodile,
                bgPosition: 0,
                leftY:0,
                rightY:1,
                hallSize:2,
                pitX:1);

            _ = Level.Level4_20_DeadEnd4;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.DesertInterior,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Crocodile,
                upper: 0,
                left: 1,
                right: 1,
                bottom: 1,
                top: 1);

            _ = Level.Level4_21_Chamber4;
            SceneDefinition.HorizontalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                variance: LevelShape.Flat,
                theme: ThemeType.DesertInterior,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                spriteGroup: SpriteGroup.Normal,
                upper: 1,
                top: 2,
                bottom: 1,
                lower:2,
                style: HorizontalScrollStyle.Interior);

            _ = Level.Level4_22_Chamber5;
            SceneDefinition.NametableScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                shape: LevelShape.TwoVerticalChambers,
                theme: ThemeType.DesertInterior,
                enemy1: EnemyIndex.Crocodile,
                enemy2: EnemyIndex.Mage,
                spriteGroup: SpriteGroup.Normal,
                top: 2,
                bottom: 2,
                left: 2,
                right: 2);

            _ = Level.Level4_23_Chamber6;
            SceneDefinition.HorizontalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                variance: LevelShape.Flat,
                theme: ThemeType.DesertInterior,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Ogre,
                spriteGroup: SpriteGroup.Normal,
                upper: 1,
                top: 1,
                bottom: 1,
                middle: 2,
                lower:2,
                style: HorizontalScrollStyle.Interior);

            _ = Level.Level4_24_BeforeMidboss2;
            SceneDefinition.NoScrollCornerStairs(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.Desert,
                spriteGroup: SpriteGroup.Boss,
                enemy1: EnemyIndex.Midboss,
                enemy2: EnemyIndex.Midboss,
                cornerStairStyle: CornerStairStyle.TwoBlockRight,
                bgPosition: 0,
                left: 1,
                right: 1,
                bottom: 1,
                top: 1);

            _ = Level.Level4_25_DeadEnd5;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.DesertInterior,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Crocodile,
                upper: 0,
                left: 1,
                right: 1,
                bottom: 1,
                top: 1);

            _ = Level.Level4_26_Pit3;
            SceneDefinition.VerticalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.DesertInterior,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Crocodile,
                shape: LevelShape.ZigZag,
                left: 1,
                right: 1);

            _ = Level.Level4_27_Chamber8;
            SceneDefinition.NoScrollTShape(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.DesertInterior,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Crocodile,
                bgPosition: 0,
                leftY: 0,
                rightY: 1,
                hallSize: 2,
                pitX: 1);

            _ = Level.Level4_28_SwitchRoom2;
            SceneDefinition.NametableScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                shape: LevelShape.TwoHorizontalChambers,
                theme: ThemeType.DesertInterior,
                enemy1: EnemyIndex.Ogre,
                enemy2: EnemyIndex.Mage,
                spriteGroup: SpriteGroup.Normal,
                top: 2,
                bottom: 2,
                left: 2,
                right: 2);

            _ = Level.Level4_29_DeadEnd6;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.DesertInterior,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Crocodile,
                upper: 0,
                left: 1,
                right: 1,
                bottom: 1,
                top: 1);

            _ = Level.Level4_30_Chamber7;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.DesertInterior,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Crocodile,
                upper: 0,
                left: 1,
                right: 1,
                bottom: 1,
                top: 1);

            _ = Level.Level4_31_Midboss;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.Desert,
                spriteGroup: SpriteGroup.Boss,
                enemy1: EnemyIndex.Midboss,
                enemy2: EnemyIndex.Midboss,
                upper: 0,
                left: 1,
                right: 1,
                bottom: 1,
                top: 1);

            _ = Level.Level4_32_Desert4;
            SceneDefinition.HorizontalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                variance: LevelShape.Flat,
                theme: ThemeType.DesertNight,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Ogre,
                spriteGroup: SpriteGroup.Normal,
                upper: 1,
                top: 0,
                bottom: 2,
                style: HorizontalScrollStyle.Flat);

            _ = Level.Level4_33_Desert5;
            SceneDefinition.HorizontalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                variance: LevelShape.Flat,
                theme: ThemeType.DesertNight,
                enemy1: EnemyIndex.Crocodile,
                enemy2: EnemyIndex.Mage,
                spriteGroup: SpriteGroup.Normal,
                upper: 1,
                top: 0,
                bottom: 2,
                style: HorizontalScrollStyle.Flat);

            _ = Level.Level4_34_DesertRain1;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.DesertRain,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                upper: 0,
                left: 0,
                right: 0,
                bottom: 1,
                top: 0);

            _ = Level.Level4_35_DesertRain2;
            SceneDefinition.NoScrollBigStairs(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.DesertRain,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Crocodile,
                enemy2: EnemyIndex.Mage,
                bgPosition: 0,
                left: 0,
                right: 0,
                bottom: 1,
                top: 0);


            _ = Level.Level4_36_DesertRain3;
            SceneDefinition.NoScrollBigStairs(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.DesertRain,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Ogre,
                enemy2: EnemyIndex.Mage,
                bgPosition: 0,
                left: 0,
                right: 0,
                bottom: 1,
                top: 0);

            _ = Level.Level4_37_DesertRain4;
            SceneDefinition.NametableScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.DesertRain,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                shape: LevelShape.Flat,
                left: 0,
                right: 0,
                bottom: 2,
                top: 0);

            _ = Level.Level4_38_DesertRain5;
            SceneDefinition.NametableScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.DesertRain,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                shape: LevelShape.Flat,
                left: 0,
                right: 0,
                bottom: 2,
                top: 0);

            _ = Level.Level4_39_BeforeBoss;
            SceneDefinition.NoScrollFlat(
                           memoryBuilder: memoryBuilder,
                           specs: specs,
                           theme: ThemeType.DesertNight,
                           spriteGroup: SpriteGroup.Normal,
                           enemy1: EnemyIndex.Lizard,
                           enemy2: EnemyIndex.Bird,
                           upper: 0,
                           left: 0,
                           right: 0,
                           bottom: 1,
                           top: 0);

            _ = Level.Level4_40_Boss;
            SceneDefinition.BossScene(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.OceanBoss);
          
            _ = Level.Level5_1_Mist;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.Mist,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                upper: 1,
                mid: 2,
                left: 0,
                right: 0,
                bottom: 2,
                top: 0);

            _ = Level.Level5_2_Bridges;
            SceneDefinition.HorizontalScroll(
              memoryBuilder: memoryBuilder,
              specs: specs,
              variance: LevelShape.HighVariance,
              theme: ThemeType.Mist,
              enemy1: EnemyIndex.Bird,
              enemy2: EnemyIndex.Boulder,
              spriteGroup: SpriteGroup.Normal,
              upper: 1,
              middle: 2,
              top: 0,
              bottom: 2,
              style: HorizontalScrollStyle.Flat);

            _ = Level.Level5_3_Bridges2;
            SceneDefinition.HorizontalScroll(
              memoryBuilder: memoryBuilder,
              specs: specs,
              variance: LevelShape.HighVariance,
              theme: ThemeType.Mist,
              enemy1: EnemyIndex.Crocodile,
              enemy2: EnemyIndex.Mage,
              spriteGroup: SpriteGroup.Normal,
              upper: 1,
              middle: 2,
              top: 0,
              bottom: 2,
              style: HorizontalScrollStyle.Flat);

            _ = Level.Level5_4_Mist2;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.Mist,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                upper: 1,
                mid: 2,
                left: 0,
                right: 0,
                bottom: 2,
                top: 0);

            _ = Level.Level5_5_Forest;
            SceneDefinition.HorizontalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                variance: LevelShape.LowVariance,
                theme: ThemeType.Forest,
                enemy1: EnemyIndex.Crocodile,
                enemy2: EnemyIndex.Mage,
                spriteGroup: SpriteGroup.Normal,
                upper: 1,
                middle: 1,
                top: 0,
                bottom: 2,
                style: HorizontalScrollStyle.Interior);

            _ = Level.Level5_6_Forest_Hub;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.Forest,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                upper: 1,
                mid: 2,
                left: 0,
                right: 0,
                bottom: 3,
                top: 0);

            _ = Level.Level5_7_Forest2;
            SceneDefinition.HorizontalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                variance: LevelShape.LowVariance,
                theme: ThemeType.Forest,
                enemy1: EnemyIndex.Crocodile,
                enemy2: EnemyIndex.Mage,
                spriteGroup: SpriteGroup.Normal,
                upper: 1,
                middle: 1,
                top: 0,
                bottom: 1,
                style: HorizontalScrollStyle.Interior);

            _ = Level.Level5_8_Forest2;
            SceneDefinition.HorizontalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                variance: LevelShape.LowVariance,
                theme: ThemeType.Forest,
                enemy1: EnemyIndex.Crocodile,
                enemy2: EnemyIndex.Mage,
                spriteGroup: SpriteGroup.Normal,
                upper: 1,
                middle: 1,
                top: 0,
                bottom: 1,
                style: HorizontalScrollStyle.Interior);

            _ = Level.Level5_9_Forest_Corner;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.Forest,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                upper: 1,
                mid: 2,
                left: 0,
                right: 2,
                bottom: 1,
                top: 0);

            _ = Level.Level5_10_Forest3;
            SceneDefinition.HorizontalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                variance: LevelShape.HighVariance,
                theme: ThemeType.Forest,
                enemy1: EnemyIndex.Crocodile,
                enemy2: EnemyIndex.Mage,
                spriteGroup: SpriteGroup.Normal,
                upper: 1,
                middle: 1,
                top: 0,
                bottom: 2,
                style: HorizontalScrollStyle.Interior);

            _ = Level.Level5_11_Forest_Path;
            SceneDefinition.NoScrollCornerStairs(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.Forest,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                bgPosition: 1,
                cornerStairStyle: CornerStairStyle.TwoBlockRight,
                left: 0,
                right: 0,
                bottom: 1,
                top: 0);

            _ = Level.Level5_12_Forest_Hub2;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.Forest,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                upper: 1,
                mid: 2,
                left: 0,
                right: 0,
                bottom: 1,
                top: 0);

            _ = Level.Level5_13_Forest_Corner2;
            SceneDefinition.NoScrollCornerStairs(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.Forest,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                bgPosition: 1,
                cornerStairStyle: CornerStairStyle.OneBlockDouble,
                left: 0,
                right: 1,
                bottom: 1,
                top: 0);

            _ = Level.Level5_14_Forest4;
            SceneDefinition.HorizontalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                variance: LevelShape.HighVariance,
                theme: ThemeType.Forest,
                enemy1: EnemyIndex.Crocodile,
                enemy2: EnemyIndex.Mage,
                spriteGroup: SpriteGroup.Normal,
                upper: 1,
                middle: 1,
                top: 0,
                bottom: 2,
                style: HorizontalScrollStyle.Interior);

            _ = Level.Level5_15_Forest_Corner3;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.Forest,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                upper: 1,
                mid: 2,
                left: 1,
                right: 0,
                bottom: 1,
                top: 0);

            _ = Level.Level5_16_Forest_Corner4;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.Forest,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                upper: 1,
                mid: 2,
                left: 2,
                right: 0,
                bottom: 1,
                top: 0);

            _ = Level.Level5_17_Forest5;
            SceneDefinition.HorizontalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                variance: LevelShape.LowVariance,
                theme: ThemeType.Forest,
                enemy1: EnemyIndex.Crocodile,
                enemy2: EnemyIndex.Mage,
                spriteGroup: SpriteGroup.Normal,
                upper: 1,
                middle: 1,
                top: 0,
                bottom: 2,
                style: HorizontalScrollStyle.Interior);

            _ = Level.Level5_18_Forest_Hub3;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.Forest,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                upper: 1,
                mid: 2,
                left: 0,
                right: 0,
                bottom: 1,
                top: 0);

            _ = Level.Level5_19_Forest6;
            SceneDefinition.HorizontalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                variance: LevelShape.LowVariance,
                theme: ThemeType.Forest,
                enemy1: EnemyIndex.Crocodile,
                enemy2: EnemyIndex.Mage,
                spriteGroup: SpriteGroup.Normal,
                upper: 1,
                middle: 1,
                top: 0,
                bottom: 3,
                style: HorizontalScrollStyle.Interior);

            _ = Level.Level5_20_Mist3;
            SceneDefinition.HorizontalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                variance: LevelShape.LowVariance,
                theme: ThemeType.Mist,
                enemy1: EnemyIndex.Crocodile,
                enemy2: EnemyIndex.Mage,
                spriteGroup: SpriteGroup.Normal,
                upper: 1,
                middle: 1,
                top: 0,
                bottom: 2,
                style: HorizontalScrollStyle.Interior);

            _ = Level.Level5_21_Mist4;
            SceneDefinition.HorizontalScroll(
                memoryBuilder: memoryBuilder,
                specs: specs,
                variance: LevelShape.LowVariance,
                theme: ThemeType.Mist,
                enemy1: EnemyIndex.Crocodile,
                enemy2: EnemyIndex.Mage,
                spriteGroup: SpriteGroup.Normal,
                upper: 1,
                middle: 1,
                top: 0,
                bottom: 2,
                style: HorizontalScrollStyle.Interior);

            _ = Level.Level5_22_MidBoss;
            SceneDefinition.NoScrollFlat(
                memoryBuilder: memoryBuilder,
                specs: specs,
                theme: ThemeType.Mist,
                spriteGroup: SpriteGroup.Normal,
                enemy1: EnemyIndex.Lizard,
                enemy2: EnemyIndex.Bird,
                upper: 1,
                mid: 2,
                left: 0,
                right: 0,
                bottom: 1,
                top: 0);

        }

        public static void AddSceneParts(SystemMemoryBuilder builder, Specs specs)
        {
            int destroyBitsNeeded = 0;
            int maxDestroyBitsNeeded = 0;

            SceneDefinition scene = new SceneDefinition(Level.Level1_1_Start, builder.Memory, specs);
            var header = new ScenePartsHeader(builder,
                b => new ExitScenePart(b, ExitType.Right, 1, scene),
                b => new SpriteScenePart(b, ScenePartType.Bomb, 8, 0, scene),
                b => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 12, 6, scene),
                b => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 12, 8, scene),
                b => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 12, 10, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: true, x: 4, y: 8, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 6, y: 8, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: false, x: 8, y: 8, definition: scene)
                );
            CheckDestroyBits(Level.Level1_1_Start, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level1_2_Horizontal, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: false, x: 8, y: 8, definition: scene)
               // , b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: false, x: 10, y: 10, definition: scene)
              //  , b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: false, x: 12, y: 8, definition: scene)
              //  , b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 18, y: 8, definition: scene)
                , b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 22, y: 8, definition: scene)
                , b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: false, x: 36, y: 8, definition: scene)
                , b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: false, x: 38, y: 8, definition: scene)
                , b => new ExitScenePart(b, ExitType.Left, -1, scene)
                , b => new ExitScenePart(b, ExitType.Right, 1, scene)
                , b => new SpriteScenePart(b, ScenePartType.Bomb, 47, 0, scene)
              //  , b => new SpriteScenePart(b, ScenePartType.EnemyType1, 18, 0, scene)
                , b => new SpriteScenePart(b, ScenePartType.EnemyType1, 28, 0, scene)
                , b => new SpriteScenePart(b, ScenePartType.EnemyType1, 58, 0, scene)
                );
            CheckDestroyBits(Level.Level1_2_Horizontal, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level1_3_Pit, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new ExitScenePart(b, ExitType.Left, -1, scene),
                b => new ExitScenePart(b, ExitType.Right, 1, scene),
                b => new ExitScenePart(b, ExitType.Bottom, 2, scene),
                b => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, false, false, 8, 10, scene),
                b => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, false, false, 6, 10, scene)
                );
            CheckDestroyBits(Level.Level1_3_Pit, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level1_4_DeadEnd, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new ExitScenePart(b, ExitType.Left, -1, scene)
                , b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 4, y: 8, definition: scene)
                , b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 6, y: 8, definition: scene)
                , b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 8, y: 8, definition: scene)
                , b => new SpriteScenePart(b, ScenePartType.Bomb, 11, 0, scene)
                );
            CheckDestroyBits(Level.Level1_4_DeadEnd, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level1_5_Vertical, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new SpriteScenePart(b, ScenePartType.Bomb, 6, 19, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType2, 4, 6, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType2, 8, 34, scene),
                b => new ExitScenePart(b, ExitType.Right, exitOffset: 5, scene),
                b => new SpriteScenePart(b, ScenePartType.DoorFowardExit, 3, 36, scene),
                b => new PrefabScenePart(b, scene, 2, 38, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Block),
                b => new PrefabScenePart(b, scene, 10, 38, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Block),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 4, y: 6, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 2, y: 6, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 12, y: 34, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 12, y: 32, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 14, y: 34, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 14, y: 32, definition: scene)

                );
            CheckDestroyBits(Level.Level1_5_Vertical, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level1_6_Platforms, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new SpriteScenePart(b, ScenePartType.DoorBackExit, 60, 12, scene),
                b => new ExitScenePart(b, ExitType.Left, exitOffset: 1, scene),
                b => PitScenePart(b, 20, PrefabSize.Four, scene),
                b => PitScenePart(b, 32, PrefabSize.Eight, scene),
                b => PitScenePart(b, 40, PrefabSize.Four, scene),

                b => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len24, 36, 8, scene),
                b => new PlatformScenePart(b, ScenePartType.Platform_LeftRight, PlatformDistance.Len32, 14, 12, scene),
                b => PitScenePart(b, 8, PrefabSize.Eight, scene),
                b => PitScenePart(b, 16, PrefabSize.Four, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType1, 48, 0, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType2, 20, 4, scene),
                b => new SpriteScenePart(b, ScenePartType.Bomb, 26, 0, scene),
              //  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: false, bottomLeft: true, bottomRight: false, x: 50, y: 10, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: false, bottomLeft: true, bottomRight: false, x: 50, y: 8, definition: scene),
             //   b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: false, bottomLeft: true, bottomRight: false, x: 52, y: 10, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: false, bottomLeft: true, bottomRight: false, x: 52, y: 8, definition: scene)

                );
            CheckDestroyBits(Level.Level1_6_Platforms, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level1_7_Door, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new SpriteScenePart(b, ScenePartType.DoorFowardExit, 4, 12, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 6, y: 8, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 8, y: 8, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 10, y: 8, definition: scene),
                b => new ExitScenePart(b, ExitType.Right, exitOffset: -1, scene)
                );
            CheckDestroyBits(Level.Level1_7_Door, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level1_8_Door2, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new SpriteScenePart(b, ScenePartType.DoorBackExit, 8, 12, scene),
                b => new ExitScenePart(b, ExitType.Right, exitOffset: 1, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType1, 12, 7, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType2, 4, 7, scene)
                );
            CheckDestroyBits(Level.Level1_8_Door2, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level1_9_Platforms2, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new ExitScenePart(b, ExitType.Left, exitOffset: -1, scene),
                b => PitScenePart(b, 12, PrefabSize.Four, scene),
                b => PitScenePart(b, 24, PrefabSize.Four, scene),
                b => PitScenePart(b, 28, PrefabSize.Eight, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType1, 40, 0, scene),
                b => new PlatformScenePart(b, ScenePartType.Platform_LeftRight, PlatformDistance.Len16, 26, 12, scene),
                b => PitScenePart(b, 48, PrefabSize.Eight, scene),
                b => new PlatformScenePart(b, ScenePartType.Platform_LeftRight, PlatformDistance.Len24, 48, 12, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 10, y: 8, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 12, y: 8, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 22, y: 8, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 48, y: 12, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 50, y: 12, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 52, y: 12, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 54, y: 12, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 56, y: 12, definition: scene),
                b => new SpriteScenePart(b, ScenePartType.Button, x: 60, y: 13, definition: scene)
                );
            CheckDestroyBits(Level.Level1_9_Platforms2, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level1_10_Stair, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new ExitScenePart(b, ExitType.Right, exitOffset: 1, scene));

            CheckDestroyBits(Level.Level1_10_Stair, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level1_11_Boss, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new SpriteScenePart(b, ScenePartType.EnemyType1, 12, 0, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 2, y: 11, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 4, y: 11, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 6, y: 11, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 8, y: 11, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 10, y: 11, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 12, y: 11, definition: scene),
                b => new ExitScenePart(b, ExitType.Left, exitOffset: -1, scene),
                b => new ExitScenePart(b, ExitType.Right, exitOffset: 1, scene));

            CheckDestroyBits(Level.Level1_11_Boss, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level1_12_Horizontal2, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 12, y: 10, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 12, y: 8, definition: scene),
                  b => new SpriteScenePart(b, ScenePartType.EnemyType2, 12, 9, scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 28, y: 10, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 28, y: 8, definition: scene),
                  b => new SpriteScenePart(b, ScenePartType.EnemyType1, 20, 0, scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 36, y: 10, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 36, y: 8, definition: scene),
                  b => new SpriteScenePart(b, ScenePartType.EnemyType1, 50, 0, scene),
                  b => new ExitScenePart(b, ExitType.Right, exitOffset: 1, scene)
              );
            CheckDestroyBits(Level.Level1_12_Horizontal2, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level1_13_Column, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                  b => new ExitScenePart(b, ExitType.Left, exitOffset: -1, scene),
                  b => new ExitScenePart(b, ExitType.Right, exitOffset: 1, scene),
                  b => new PrefabScenePart(b, scene, 16, 2, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Block),
                  b => new PrefabScenePart(b, scene, 24, 2, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Block),
                  b => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len16, 12, 8, scene),
                  b => new SpriteScenePart(b, ScenePartType.EnemyType1, 18, 0, scene),
                  b => new SpriteScenePart(b, ScenePartType.EnemyType2, 42, 8, scene),
                  b => new SpriteScenePart(b, ScenePartType.Bomb, 30, 0, scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: true, bottomRight: true, x: 18, y: 4, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: true, bottomRight: true, x: 20, y: 4, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: true, bottomRight: true, x: 22, y: 4, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: true, bottomRight: true, x: 24, y: 4, definition: scene),

                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 54, y: 9, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 54, y: 11, definition: scene),

                  b => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 36, y: 11, definition: scene),

                  b => new PrefabScenePart(b, scene, 48, 0, PrefabSize.Four, PrefabSize.Eight, PrefabStyle.Block),
                  b => PitScenePart(b, 40, PrefabSize.Eight, scene),
                  b => new PlatformScenePart(b, ScenePartType.Platform_LeftRight, PlatformDistance.Len24, 40, 8, scene)
              );
            CheckDestroyBits(Level.Level1_13_Column, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level1_14_BigRoom, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                  b => new ExitScenePart(b, ExitType.Left, exitOffset: -1, scene),
                  b => new PrefabScenePart(b, scene, 12, 24, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Block),
                  b => new PrefabScenePart(b, scene, 16, 20, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Block),
                  b => new PrefabScenePart(b, scene, 24, 20, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Block),
                  b => new PrefabScenePart(b, scene, 28, 8, PrefabSize.Four, PrefabSize.Eight, PrefabStyle.Block),
                  b => new PrefabScenePart(b, scene, 28, 16, PrefabSize.Four, PrefabSize.Eight, PrefabStyle.Block),

                  b => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len16, 20, 13, scene),
                  b => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len32, 24, 14, scene),
                  b => new SpriteScenePart(b, ScenePartType.EnemyType1, 8, 0, scene),
                  b => new SpriteScenePart(b, ScenePartType.EnemyType1, 24, 0, scene),
                  b => new SpriteScenePart(b, ScenePartType.DoorFowardExit, 28, 8, scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 18, y: 16, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: true, bottomRight: true, x: 18, y: 14, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: true, bottomLeft: false, bottomRight: true, x: 16, y: 16, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: false, bottomLeft: true, bottomRight: false, x: 20, y: 16, definition: scene),
                  b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: false, x: 18, y: 18, definition: scene)
                  );
            CheckDestroyBits(Level.Level1_14_BigRoom, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level1_15_Vertical2, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                  b => new SpriteScenePart(b, ScenePartType.DoorBackExit, 10, 44, scene),
                  b => new PrefabScenePart(b, scene, 4, 44, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Block),
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
            CheckDestroyBits(Level.Level1_15_Vertical2, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level1_16_BeforeBoss, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                  b => new SpriteScenePart(b, ScenePartType.DoorBackExit, 8, 12, scene),
                  b => new ExitScenePart(b, ExitType.Right, 1, scene)
                  );
            CheckDestroyBits(Level.Level1_16_BeforeBoss, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level1_17_Boss, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new SpriteScenePart(b, ScenePartType.Bomb, x: 12, y: 16, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 0, y: 0, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 2, y: 0, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 4, y: 0, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 6, y: 0, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 8, y: 0, definition: scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType1, x: 0, y: 16, definition: scene)
            );
            CheckDestroyBits(Level.Level1_17_Boss, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level2_1_Intro, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                 b => new SpriteScenePart(b, ScenePartType.EnemyType1, x: 15, y: 12, definition: scene),
                 b => PitScenePart(b, 16, PrefabSize.Eight, scene),
                 b => PitScenePart(b, 24, PrefabSize.Eight, scene),
                 b => PitScenePart(b, 32, PrefabSize.Eight, scene),
                 b => PitScenePart(b, 40, PrefabSize.Eight, scene),
                 b => new ExitScenePart(b, ExitType.Right, 1, scene)
            );
            CheckDestroyBits(Level.Level2_1_Intro, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level2_2_Fly, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new AutoscrollScenePart(b, ScenePartType.Coin, position: 4, delay: 2, variation: PrizeController.Coin3, scene),
                b => new AutoscrollScenePart(b, ScenePartType.Coin, position: 12, delay: 6, variation: PrizeController.Coin3, scene),
                b => new AutoscrollScenePart(b, ScenePartType.Coin, position: 8, delay: 6, variation: PrizeController.Coin5Diag, scene),
                b => new AutoscrollScenePart(b, ScenePartType.EnemyType2, position: 6, delay: 8, variation: 2, scene),
                b => new AutoscrollScenePart(b, ScenePartType.EnemyType2, position: 6, delay: 8, variation: 2, scene),
                b => new AutoscrollScenePart(b, ScenePartType.EnemyType2, position: 6, delay: 8, variation: 2, scene),
                b => new AutoscrollScenePart(b, ScenePartType.EnemyType2, position: 6, delay: 8, variation: 2, scene),
                b => new AutoscrollScenePart(b, ScenePartType.EnemyType1, position: 4, delay: 8, variation: 1, scene),
                b => new AutoscrollScenePart(b, ScenePartType.EnemyType1, position: 12, delay: 8, variation: 1, scene),
                b => new AutoscrollScenePart(b, ScenePartType.EnemyType2, position: 6, delay: 8, variation: 1, scene),
                b => new AutoscrollScenePart(b, ScenePartType.EnemyType2, position: 9, delay: 4, variation: 1, scene),
                b => new AutoscrollScenePart(b, ScenePartType.Coin, position: 8, delay: 0, variation: PrizeController.Coin5Diag2, scene),
                b => new AutoscrollScenePart(b, ScenePartType.EnemyType1, position: 8, delay: 15, variation: 1, scene),
                b => new AutoscrollScenePart(b, ScenePartType.Coin, position: 4, delay: 4, variation: PrizeController.Coin3, scene),
                b => new AutoscrollScenePart(b, ScenePartType.Coin, position: 12, delay: 0, variation: PrizeController.Coin3, scene),
                b => new AutoscrollScenePart(b, ScenePartType.EnemyType1, position: 4, delay: 8, variation: 1, scene),
                b => new AutoscrollScenePart(b, ScenePartType.EnemyType1, position: 12, delay: 8, variation: 1, scene),
                b => new AutoscrollScenePart(b, ScenePartType.EnemyType2, position: 10, delay: 8, variation: 1, scene),
                b => new AutoscrollScenePart(b, ScenePartType.Coin, position: 2, delay: 6, variation: PrizeController.Coin5Diag, scene),
                b => new AutoscrollScenePart(b, ScenePartType.EnemyType1, position: 6, delay: 4, variation: 1, scene),
                b => new AutoscrollScenePart(b, ScenePartType.EnemyType1, position: 14, delay: 4, variation: 1, scene),
                b => new AutoscrollScenePart(b, ScenePartType.Coin, position: 8, delay: 8, variation: PrizeController.Coin5Diag2, scene),
                b => new AutoscrollScenePart(b, ScenePartType.EnemyType1, position: 4, delay: 8, variation: 1, scene),
                b => new AutoscrollScenePart(b, ScenePartType.EnemyType1, position: 12, delay: 8, variation: 1, scene),
                b => new AutoscrollScenePart(b, ScenePartType.Coin, position: 4, delay: 6, variation: PrizeController.Coin3, scene),
                b => new AutoscrollScenePart(b, ScenePartType.Coin, position: 12, delay: 0, variation: PrizeController.Coin3, scene),
                b => new AutoscrollScenePart(b, ScenePartType.Coin, position: 8, delay: 8, variation: PrizeController.Coin5Diag2, scene),
                b => new AutoscrollScenePart(b, ScenePartType.Coin, position: 8, delay: 8, variation: PrizeController.Coin5Diag, scene),
                b => new AutoscrollScenePart(b, ScenePartType.EnemyType1, position: 8, delay: 6, variation: 1, scene),
                b => new AutoscrollScenePart(b, ScenePartType.EnemyType1, position: 8, delay: 6, variation: 1, scene),
                b => new AutoscrollScenePart(b, ScenePartType.EnemyType2, position: 9, delay: 8, variation: 1, scene),
                b => new AutoscrollScenePart(b, ScenePartType.EnemyType2, position: 9, delay: 8, variation: 1, scene),
                b => new AutoscrollScenePart(b, ScenePartType.EnemyType2, position: 9, delay: 8, variation: 1, scene)
            );
            CheckDestroyBits(Level.Level2_2_Fly, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level2_2_Fly2, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new AutoscrollScenePart(b, ScenePartType.EnemyType1, position: 9, delay: 15, variation: 1, scene)
            );
            CheckDestroyBits(Level.Level2_2_Fly2, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level2_3_Beach, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                 b => new SpriteScenePart(b, ScenePartType.EnemyType1, 12, 4, scene),
                 b => new SpriteScenePart(b, ScenePartType.EnemyType2, 20, 8, scene),
                 b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 10, y: 8, definition: scene),
                 b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 12, y: 8, definition: scene),
                 b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 14, y: 8, definition: scene),
                 b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 42, y: 8, definition: scene),
                 b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: true, bottomRight: true, x: 44, y: 8, definition: scene),
                 b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: false, x: 46, y: 10, definition: scene),
                 b => new SpriteScenePart(b, ScenePartType.EnemyType2, 40, 8, scene),
                 b => new SpriteScenePart(b, ScenePartType.Bomb, 10, 8, scene),
                 b => new SpriteScenePart(b, ScenePartType.EnemyType1, 50, 4, scene),
                 b => new ExitScenePart(b, ExitType.Right, 1, scene)
            );
            CheckDestroyBits(Level.Level2_3_Beach, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level2_4_Beach2, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                 b => new SpriteScenePart(b, ScenePartType.EnemyType1, 10, 12, scene),
                 b => new SpriteScenePart(b, ScenePartType.EnemyType2, 20, 8, scene),
                 b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 40, y: 8, definition: scene),
                 b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 42, y: 8, definition: scene),
                 b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: false, bottomLeft: true, bottomRight: false, x: 40, y: 10, definition: scene),
                 b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: true, bottomLeft: false, bottomRight: true, x: 42, y: 10, definition: scene),
                 b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: true, bottomLeft: false, bottomRight: true, x: 38, y: 10, definition: scene),
                 b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: false, bottomLeft: true, bottomRight: false, x: 44, y: 10, definition: scene),
                 b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: true, bottomLeft: true, bottomRight: true, x: 10, y: 10, definition: scene),
                 b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: false, bottomLeft: true, bottomRight: true, x: 20, y: 10, definition: scene),

                 b => new PrefabScenePart(b, scene, 12, 10, PrefabSize.Eight, PrefabSize.Four, PrefabStyle.Block),
                 b => PitScenePart(b, 40, PrefabSize.Four, scene),
                 b => new SpriteScenePart(b, ScenePartType.EnemyType1, 50, 12, scene),
                 b => PitScenePart(b, 52, PrefabSize.Two, scene),
                 b => new ExitScenePart(b, ExitType.Left, -1, scene),
                 b => new ExitScenePart(b, ExitType.Right, 1, scene)
            );
            CheckDestroyBits(Level.Level2_4_Beach2, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level2_5_Hub, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new PrefabScenePart(b, scene, 0, 20, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Block),
                b => new PrefabScenePart(b, scene, 8, 16, PrefabSize.Six, PrefabSize.Eight, PrefabStyle.Block),
                b => new PrefabScenePart(b, scene, 8, 20, PrefabSize.Six, PrefabSize.Eight, PrefabStyle.Block),
                b => new PrefabScenePart(b, scene, 28, 16, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Block),
                b => new PrefabScenePart(b, scene, 28, 20, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Block),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, true, true, true, true, 22, 18, scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, true, true, true, true, 24, 18, scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, true, true, true, true, 26, 18, scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, true, true, true, true, 14, 18, scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, true, true, true, true, 16, 18, scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, true, true, true, true, 18, 18, scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, true, true, true, true, 20, 18, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType1, 3, 40, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType2, 13, 20, scene),
                b => new ExitScenePart(b, ExitType.Left, -1, scene),
                b => new ExitScenePart(b, ExitType.Right, 2, scene),
                b => new SpriteScenePart(b, ScenePartType.DoorFowardExit, 3, 42, scene)
            );
            CheckDestroyBits(Level.Level2_5_Hub, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level2_6_After_Hub, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new SpriteScenePart(b, ScenePartType.DoorBackExit, 3, 42, scene),
                b => new ExitScenePart(b, ExitType.Right, 4, scene)
            );
            CheckDestroyBits(Level.Level2_6_After_Hub, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level2_7_Door, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new ExitScenePart(b, ExitType.Left, -2, scene),
                b => new SpriteScenePart(b, ScenePartType.DoorFowardExit, 8, 10, scene)
            );
            CheckDestroyBits(Level.Level2_7_Door, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level2_8_Platforms, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new SpriteScenePart(b, ScenePartType.DoorBackExit, 3, 10, scene),
                b => PitScenePart(b, 8, PrefabSize.Eight, scene),
                b => PitScenePart(b, 16, PrefabSize.Eight, scene),

                b => PitScenePart(b, 32, PrefabSize.Eight, scene),
                b => PitScenePart(b, 40, PrefabSize.Eight, scene),
                b => PitScenePart(b, 48, PrefabSize.Six, scene),

                b => new SpriteScenePart(b, ScenePartType.EnemyType1, 3, 40, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType1, 3, 20, scene),
                b => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len16, 12, 8, scene),
                b => new PlatformScenePart(b, ScenePartType.Platform_LeftRight, PlatformDistance.Len32, 16, 8, scene),
                b => new PlatformScenePart(b, ScenePartType.Platform_LeftRight, PlatformDistance.Len16, 36, 8, scene),
                b => new PlatformScenePart(b, ScenePartType.Platform_LeftRight, PlatformDistance.Len32, 40, 12, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 6, y: 8, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 26, y: 8, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 28, y: 8, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 30, y: 8, definition: scene),

                b => new ExitScenePart(b, ExitType.Right, 1, scene)
            );
            CheckDestroyBits(Level.Level2_8_Platforms, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level2_9_Pit, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                   b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, true, true, true, false, 6, 12, scene),
                   b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, true, true, false, false, 8, 12, scene),
                   b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, true, true, false, true, 10, 12, scene),
                   b => new PrefabScenePart(b, scene, 12, 4, PrefabSize.Four, PrefabSize.Four, PrefabStyle.Space),
                   b => new ExitScenePart(b, ExitType.Left, -1, scene),
                   b => new SpriteScenePart(b, ScenePartType.Button, 14, 9, scene),
                   b => new ExitScenePart(b, ExitType.Bottom, -2, scene)
            );
            CheckDestroyBits(Level.Level2_9_Pit, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level2_10_Beach3, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new ExitScenePart(b, ExitType.Left, -1, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType1, 16, 12, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType2, 20, 12, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType2, 30, 12, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType1, 44, 12, scene),
                b => new ExitScenePart(b, ExitType.Right, 1, scene)
            );
            CheckDestroyBits(Level.Level2_10_Beach3, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level2_11_Beach4, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new ExitScenePart(b, ExitType.Left, -1, scene),

                b => new SpriteScenePart(b, ScenePartType.EnemyType2, 12, 12, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType2, 30, 4, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType1, 40, 4, scene),
                 b => new SpriteScenePart(b, ScenePartType.EnemyType1, 50, 4, scene),

                b => new ExitScenePart(b, ExitType.Right, 1, scene)
            );
            CheckDestroyBits(Level.Level2_11_Beach4, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);


            scene = new SceneDefinition(Level.Level2_12_Boss, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new SpriteScenePart(b, ScenePartType.Bomb, x: 12, y: 16, definition: scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType1, x: 18, y: 16, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 0, y: 0, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 2, y: 0, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 4, y: 0, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 6, y: 0, definition: scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 8, y: 0, definition: scene)
            );
            CheckDestroyBits(Level.Level2_12_Boss, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level3_1_City, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
             //   b => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 10, 10, scene),
              //  b => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 10, 8, scene),
              //  b => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 28, 10, scene),
              //  b => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, true, true, 52, 10, scene),
              //  b => new DynamicScenePart(b, DynamicBlockType.Coin, false, true, true, true, 60, 10, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType2, 8, 4, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType2, 24, 4, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType2, 32, 4, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType2, 48, 4, scene),
                b => new SpriteScenePart(b, ScenePartType.Bomb, 12, 0, scene),
                b => new SpriteScenePart(b, ScenePartType.Bomb, 30, 0, scene),
                b => new ExitScenePart(b, ExitType.Right, 1, scene)
            );
            CheckDestroyBits(Level.Level3_1_City, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level3_2_Building1, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new ExitScenePart(b, ExitType.Left, -1, scene),
                b => new TurretScenePart(b, scene, Direction.Right, 5, 52),
                b => new TurretScenePart(b, scene, Direction.Left, 11, 46),
                b => new TurretScenePart(b, scene, Direction.Down, 11, 52),
                b => new TurretScenePart(b, scene, Direction.Left, 11, 32),
                b => new TurretScenePart(b, scene, Direction.Down, 10, 12),
                b => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len32, 8, 50, scene),
                b => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len24, 6, 44, scene),
                b => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len16, 8, 34, scene),
                b => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len48, 6, 22, scene),
                b => new SpriteScenePart(b, ScenePartType.DoorFowardExit, 10, 20, scene),
                b => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 12, 56, scene),
                b => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 12, 58, scene),
                b => new ExitScenePart(b, ExitType.Right, 3, scene)
            );
            CheckDestroyBits(Level.Level3_2_Building1, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level3_3_Building1_Room1, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new ExitScenePart(b, ExitType.Right, 1, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, false, false, true, true, 6, 10, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, false, false, true, true, 8, 10, scene),
                b => new SpriteScenePart(b, ScenePartType.Bomb, 5, 6, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType1, 14, 8, scene),
                b => new SpriteScenePart(b, ScenePartType.DoorBackExit, 2, 6, scene)
            );
            CheckDestroyBits(Level.Level3_3_Building1_Room1, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level3_4_Building1_Room2, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new PrefabScenePart(builder, scene, 20, 12, PrefabSize.Four, PrefabSize.Four, PrefabStyle.Space),
                b => new PrefabScenePart(builder, scene, 24, 12, PrefabSize.Four, PrefabSize.Six, PrefabStyle.Space),
                b => new PrefabScenePart(builder, scene, 20, 20, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.StairUp),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, false, true, true, true, 8, 20, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, true, true, 10, 20, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, false, true, 8, 22, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, false, 10, 22, scene),

                b => new DynamicScenePart(b, DynamicBlockType.Coin, false, true, true, true, 16, 20, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, true, true, 18, 20, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, false, true, 16, 22, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, false, 18, 22, scene),


                b => new SpriteScenePart(b, ScenePartType.EnemyType1, 10, 13, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType1, 18, 24, scene),
                b => new SpriteScenePart(b, ScenePartType.DoorFowardExit, 5, 13, scene),
                b => new ExitScenePart(b, ExitType.Left, -1, scene)
            );
            CheckDestroyBits(Level.Level3_4_Building1_Room2, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level3_5_City2, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new SpriteScenePart(b, ScenePartType.DoorBackExit, 5, 10, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType2, 16, 0, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType2, 32, 0, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType2, 44, 0, scene),
                b => new SpriteScenePart(b, ScenePartType.Bomb, 16, 4, scene),
                b => new ExitScenePart(b, ExitType.Right, 1, scene),
                b => new ExitScenePart(b, ExitType.Left, -3, scene)
            );
            CheckDestroyBits(Level.Level3_5_City2, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level3_6_City3, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                b => new ExitScenePart(b, ExitType.Left, -1, scene),
                b => new PrefabScenePart(b, scene, 10, 6, PrefabSize.Four, PrefabSize.Four, PrefabStyle.Space),
                b => new PrefabScenePart(b, scene, 30, 6, PrefabSize.Four, PrefabSize.Eight, PrefabStyle.Space),
                b => new PrefabScenePart(b, scene, 40, 2, PrefabSize.Four, PrefabSize.Four, PrefabStyle.StairUp),
                b => new PrefabScenePart(b, scene, 44, 6, PrefabSize.Eight, PrefabSize.Four, PrefabStyle.Space),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, false, 8, 6, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, false, true, 8, 8, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, true, true, 10, 6, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, false, true, true, true, 10, 8, scene),

                b => new DynamicScenePart(b, DynamicBlockType.Coin, false, true, false, false, 28, 6, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, true, false, 28, 8, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, false, true, false, true, 30, 6, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, true, false, 30, 8, scene),


                b => new SpriteScenePart(b, ScenePartType.EnemyType1, 16, 8, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType1, 24, 8, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType2, 20, 0, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType1, 45, 8, scene),
                b => new SpriteScenePart(b, ScenePartType.EnemyType2, 60, 0, scene),


                b => new ExitScenePart(b, ExitType.Right, 1, scene)
            );
            CheckDestroyBits(Level.Level3_6_City3, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            scene = new SceneDefinition(Level.Level3_7_Building2, builder.Memory, specs);
            header = new ScenePartsHeader(builder,
                //  b => new ExitScenePart(b, ExitType.Left, -1, scene),
                b => new ExitScenePart(b, ExitType.Right, 2, scene),
                b => new ExitScenePart(b, ExitType.Top, 3, scene),
                b => new PlatformScenePart(builder, ScenePartType.Platform_Vanishing, PlatformDistance.Len16, 8, 58, scene),
                b => new PlatformScenePart(builder, ScenePartType.Platform_Vanishing, PlatformDistance.Len48, 6, 54, scene),
                b => new PlatformScenePart(builder, ScenePartType.Platform_Vanishing, PlatformDistance.Len24, 4, 50, scene),
                b => new PlatformScenePart(builder, ScenePartType.Platform_Vanishing, PlatformDistance.Len16, 8, 46, scene),

                b => new PlatformScenePart(builder, ScenePartType.Platform_Vanishing, PlatformDistance.Len32, 6, 38, scene),
                b => new PlatformScenePart(builder, ScenePartType.Platform_Vanishing, PlatformDistance.Len24, 6, 34, scene),


                b => new PlatformScenePart(builder, ScenePartType.Platform_Vanishing, PlatformDistance.Len16, 4, 30, scene),
                b => new PlatformScenePart(builder, ScenePartType.Platform_Vanishing, PlatformDistance.Len24, 8, 26, scene),

                b => new PlatformScenePart(builder, ScenePartType.Platform_UpDown, PlatformDistance.Len32, 8, 10, scene),

                b => new DynamicScenePart(b, DynamicBlockType.Coin, false, true, true, true, 10, 48, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, false, true, true, true, 10, 46, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 4, 22, scene),
                b => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 10, 22, scene),

                b => new TurretScenePart(b, scene, Direction.Right, 4, 42),
                b => new TurretScenePart(b, scene, Direction.Left, 11, 15),


                b => new PrefabScenePart(b, scene, 12, 24, PrefabSize.Eight, PrefabSize.Four, PrefabStyle.Space),
                b => new PrefabScenePart(b, scene, 0, 6, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Block),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, true, true, false, false, 8, 4, scene),
                b => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, true, true, false, false, 10, 4, scene),

                b => new SpriteScenePart(b, ScenePartType.DoorFowardExit, 11, 10, scene)
            );
            CheckDestroyBits(Level.Level3_7_Building2, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);

            AddLevel(
                Level.Level3_8_Building2_Room1,
                builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
                (b, scene) => new SpriteScenePart(b, ScenePartType.DoorBackExit, 5, 12, scene),
                (b, scene) => new SpriteScenePart(b, ScenePartType.Button, x: 10, y: 13, definition: scene),
                (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 12, 10, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, false, true, true, true, 12, 10, scene)
            );

            AddLevel(
                Level.Level3_9_Building2_Room2,
                builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
                (b, scene) => new ExitScenePart(b, ExitType.Left, -2, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 4, 4, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 6, 4, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 8, 4, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 10, 4, scene),
                (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 10, 8, scene)
            );

            AddLevel(
                Level.Level3_10_Building3,
                builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, false, true, false, 4, 58, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 10, 52, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, false, false, false, true, 8, 52, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, false, false, false, true, 10, 50, scene),

                (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, false, true, false, true, 4, 42, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, false, true, false, 4, 40, scene),

                (b, scene) => new TurretScenePart(b, scene, Direction.Right, 4, 48),
                (b, scene) => new TurretScenePart(b, scene, Direction.Left, 11, 38),

                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 6, 34, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 8, 34, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 10, 34, scene),

                (b, scene) => new PrefabScenePart(b, scene, 8, 44, PrefabSize.Two, PrefabSize.Four, PrefabStyle.Space),
                (b, scene) => new PrefabScenePart(b, scene, 4, 26, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Space),
                (b, scene) => new PrefabScenePart(b, scene, 12, 14, PrefabSize.Four, PrefabSize.Four, PrefabStyle.Space),
                (b, scene) => new PrefabScenePart(b, scene, 0, 0, PrefabSize.Eight, PrefabSize.Six, PrefabStyle.Space),


                (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len32, 8, 42, scene),

                (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Vanishing, PlatformDistance.Len48, 6, 22, scene),
                (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Vanishing, PlatformDistance.Len16, 4, 26, scene),
                (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Vanishing, PlatformDistance.Len32, 6, 30, scene),
                (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Vanishing, PlatformDistance.Len16, 8, 34, scene),
                (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene),
                (b, scene) => new ExitScenePart(b, ExitType.Left, 2, scene),

                (b, scene) => new SpriteScenePart(b, ScenePartType.DoorFowardExit, 4, 9, scene),
                (b, scene) => new ExitScenePart(b, ExitType.Bottom, -3, scene)
            );

            AddLevel(
               Level.Level3_11_Building3_Room1,
               builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
               (b, scene) => new ExitScenePart(b, ExitType.Left, -1, scene),
               (b, scene) => new PrefabScenePart(builder, scene, 12, 16, PrefabSize.Six, PrefabSize.Six, PrefabStyle.Space),
               (b, scene) => new PrefabScenePart(builder, scene, 24, 12, PrefabSize.Four, PrefabSize.Eight, PrefabStyle.Space),
               (b, scene) => new PrefabScenePart(builder, scene, 12, 8, PrefabSize.Six, PrefabSize.Six, PrefabStyle.Space),
               (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len16, 26, 16, scene),
               (b, scene) => new SpriteScenePart(b, ScenePartType.DoorBackExit, 4, 13, scene),
               (b, scene) => new SpriteScenePart(b, ScenePartType.Button, 16, 27, scene),

               (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 12, 24, scene),
               (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 12, 8, scene),

               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 14, 20, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 14, 12, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, false, true, true, true, 12, 12, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, true, true, 16, 12, scene),

               (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, true, true, true, true, 24, 24, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, true, true, true, true, 24, 22, scene)
            );

            AddLevel(
              Level.Level3_12_Building3_Room2,
              builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
              (b, scene) => new ExitScenePart(b, ExitType.Bottom, 1, scene),
              (b, scene) => new ExitScenePart(b, ExitType.Right, -2, scene)
           );

            AddLevel(
             Level.Level3_13_Building3_Room3,
             builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
             //   (b, scene) => new ExitScenePart(b, ExitType.Top, -1, scene),
             (b, scene) => new ExitScenePart(b, ExitType.Left, 1, scene)
          );

            AddLevel(
            Level.Level3_14_Building3_Room4,
            builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
            (b, scene) => new TurretScenePart(b, scene, Direction.Down, 5, 2),
            (b, scene) => new TurretScenePart(b, scene, Direction.Down, 10, 2),
            (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 8, 6, scene),
            (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 6, 6, scene),

            (b, scene) => new ExitScenePart(b, ExitType.Left, 1, scene),
            (b, scene) => new ExitScenePart(b, ExitType.Bottom, 5, scene),
            (b, scene) => new ExitScenePart(b, ExitType.Right, -1, scene)
         );

            AddLevel(
             Level.Level3_15_Building3_Room5,
             builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
             (b, scene) => new ExitScenePart(b, ExitType.Left, 1, scene),
             (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 7, 8, scene),
             (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, false, false, true, true, 2, 2, scene),
             (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, false, 2, 4, scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, false, false, true, true, 12, 2, scene),
             (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, false, true, 12, 4, scene),

             (b, scene) => new ExitScenePart(b, ExitType.Right, -1, scene)
          );

            AddLevel(
                Level.Level3_16_Building3_Room6,
                builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
                (b, scene) => new ExitScenePart(b, ExitType.Bottom, 1, scene),
                (b, scene) => new ExitScenePart(b, ExitType.Right, -1, scene),
                (b, scene) => new SpriteScenePart(b, ScenePartType.DoorFowardExit, 4, 4, scene),
                (b, scene) => new SpriteScenePart(b, ScenePartType.Button, 2, 13, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, true, true, true, true, 2, 2, scene),

                (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, false, true, false, true, 4, 10, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, false, true, 4, 8, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, false, false, 2, 8, scene)
            );

            AddLevel(
                Level.Level3_17_Building3_Room7,
                builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
                (b, scene) => new ExitScenePart(b, ExitType.Top, -1, scene),
                (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene),
                (b, scene) => new SpriteScenePart(b, ScenePartType.DoorBackExit, 2, 12, scene)
            );

            AddLevel(
               Level.Level3_18_Building3_Room8,
               builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, true, true, true, true, 8, 12, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, true, true, true, true, 10, 12, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, true, true, true, true, 8, 10, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, true, false, true, true, 8, 8, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, true, false, true, true, 10, 10, scene),
               (b, scene) => new TurretScenePart(b, scene, Direction.Down, 11, 2),
               (b, scene) => new ExitScenePart(b, ExitType.Left, -1, scene),
               (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene),
               (b, scene) => new ExitScenePart(b, ExitType.Bottom, 2, scene)
           );

            AddLevel(
               Level.Level3_19_Building3_Room9,
               builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
               (b, scene) => new ExitScenePart(b, ExitType.Top, -5, scene),
               (b, scene) => new ExitScenePart(b, ExitType.Left, -1, scene),
               (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len16, 8, 4, scene),
               (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Vanishing, PlatformDistance.Len32, 4, 10, scene),
               (b, scene) => new SpriteScenePart(b, ScenePartType.Bomb, 8, 0, scene)
           );

            AddLevel(
             Level.Level3_20_Midboss,
             builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
             (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 4, 4, scene),
             (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene),
             (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len24, 4, 5, scene),
             (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len16, 10, 6, scene),
             (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_LeftRight, PlatformDistance.Len16, 6, 5, scene),
             (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 4, 10, scene),
             (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 6, 10, scene),
             (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 8, 10, scene),
             (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 10, 10, scene),
             (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 2, 10, scene),
             (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 12, 10, scene)
             );

            AddLevel(
                Level.Level3_21_CityAfterMidboss,
                builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
                (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 12, 6, scene),
                (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 20, 0, scene),
                (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 50, 0, scene),
                (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 50, 4, scene),
                (b, scene) => new SpriteScenePart(b, ScenePartType.Bomb, 35, 0, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, false, false, true, true, 28, 8, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, false, false, true, false, 30, 8, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 30, 10, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 40, 10, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 55, 10, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, false, false, true, true, 57, 10, scene),

                (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene)

                );

            AddLevel(
               Level.Level3_22_CityTrain,
               builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
               (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene),
               (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 12, 0, scene),
               (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 20, 4, scene),
               (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 40, 3, scene),
               (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 60, 8, scene),
               (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 33, 0, scene),
               (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 55, 0, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, false, true, 12, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, false, true, true, false, 10, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, false, true, 8, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, false, true, true, false, 6, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, false, true, 24, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, false, true, true, false, 22, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, false, true, 20, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, false, true, true, false, 18, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 40, 8, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 44, 8, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 44, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, false, true, 56, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, false, true, true, false, 54, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, false, true, 52, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, false, true, true, false, 50, 6, scene)

               );

            AddLevel(
               Level.Level3_23_CityTrain2,
               builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
               (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene),

               (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 24, 0, scene),
               (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 44, 0, scene),

               (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 10, 2, scene),
               (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 40, 7, scene),


               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, false, true, 4, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, false, true, 6, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, false, true, 8, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, false, true, 18, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, false, true, 20, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, false, true, 22, 6, scene),

               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, false, false, true, true, 28, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, false, false, true, true, 30, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, false, false, true, true, 32, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, false, false, true, true, 34, 6, scene),

               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, false, true, 42, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, false, true, 44, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, false, true, 46, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, false, true, 48, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 54, 8, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 54, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 56, 8, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 56, 6, scene)


               );

            AddLevel(
               Level.Level3_24_CityTrain3,
               builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
               (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene),
               (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 20, 0, scene),
               (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 40, 0, scene),

               (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 20, 4, scene),
               (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 20, 5, scene),

               (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 50, 4, scene),
               (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 50, 5, scene),

               (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 16, 8, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 20, 8, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 14, 8, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 26, 8, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, false, true, 30, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, false, true, true, false, 32, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, false, true, 34, 6, scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, false, true, true, false, 36, 6, scene)
               );

            AddLevel(
                Level.Level3_25_CityBeforeBoss,
                builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
                (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 8, 4, scene),
                (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 10, 6, scene),
                (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 16, 0, scene),
                (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 24, 0, scene),
                (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 34, 6, scene),
                (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 44, 0, scene),
                (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 12, 8, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 14, 10, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, false, false, true, false, 14, 8, scene),

                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, false, 52, 8, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, false, true, true, true, 52, 10, scene),

               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 30, 10, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, false, false, true, false, 30, 8, scene)


                );

            AddLevel(
                Level.Level3_26_Boss,
                builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
                (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, x: 0, y: 16, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 0, y: 0, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 2, y: 0, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 4, y: 0, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 6, y: 0, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 8, y: 0, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 10, y: 0, definition: scene)
            );

            AddLevel(
                Level.Level4_1_Desert,
                builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
                (b, scene) => new SpriteScenePart(b, ScenePartType.Bomb, x: 8, y: 0, definition: scene),
                (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene)
            );

            AddLevel(
                Level.Level4_2_Desert2,
                builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
                (b, scene) => new ExitScenePart(b, ExitType.Left, -1, scene),
                (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene),
                (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, x: 12, y: 0, definition: scene),
                (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, x: 21, y: 0, definition: scene),
                (b, scene) => new SpriteScenePart(b, ScenePartType.Bomb, x: 24, y: 0, definition: scene),
                (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, x: 32, y: 0, definition: scene),
                (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, x: 46, y: 0, definition: scene),
                (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, x: 54, y: 0, definition: scene),
                (b, scene) => new PrefabScenePart(b, scene, 8, 4, PrefabSize.Eight, PrefabSize.Four, PrefabStyle.Block),
                (b, scene) => new PrefabScenePart(b, scene, 32, 4, PrefabSize.Eight, PrefabSize.Four, PrefabStyle.Block),
                (b, scene) => new PrefabScenePart(b, scene, 40, 4, PrefabSize.Four, PrefabSize.Four, PrefabStyle.Block)
             //   (b, scene) => new PrefabScenePart(b, scene, 48, 4, PrefabSize.Eight, PrefabSize.Four, PrefabStyle.Block)
            );

            AddLevel(
              Level.Level4_3_MageIntro,
              builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
              (b, scene) => new ExitScenePart(b, ExitType.Left, -1, scene),
              (b, scene) => new ExitScenePart(b, ExitType.Right, 2, scene),
              (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, x: 16, y: 24, definition: scene),
              (b, scene) => new SpriteScenePart(b, ScenePartType.Bomb, x: 8, y: 0, definition: scene),

              (b, scene) => new ExitScenePart(b, ExitType.Bottom, 1, scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, topLeft: false, topRight: true, bottomLeft: true, bottomRight: true, x: 12, y: 24, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 14, y: 24, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 16, y: 24, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, topLeft: true, topRight: false, bottomLeft: true, bottomRight: true, x: 18, y: 24, definition: scene),

              (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, topLeft: false, topRight: true, bottomLeft: true, bottomRight: true, x: 14, y: 22, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, topLeft: true, topRight: false, bottomLeft: true, bottomRight: true, x: 16, y: 22, definition: scene)

            );

            AddLevel(
              Level.Level4_4_Bonus,
              builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
              (b, scene) => new ExitScenePart(b, ExitType.Top, -1, scene),
              (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len16, 8, 4, scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 2, y: 8, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 4, y: 8, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 6, y: 8, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 8, y: 8, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 10, y: 8, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 12, y: 8, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 2, y: 6, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 4, y: 6, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 6, y: 6, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 8, y: 6, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 10, y: 6, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 12, y: 6, definition: scene)

            );

            AddLevel(
              Level.Level4_5_Desert3,
              builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
              (b, scene) => new PrefabScenePart(b, scene, 0, 4, PrefabSize.Eight, PrefabSize.Four, PrefabStyle.Block),
              (b, scene) => new PrefabScenePart(b, scene, 8, 4, PrefabSize.Eight, PrefabSize.Four, PrefabStyle.Block),
              (b, scene) => new PrefabScenePart(b, scene, 40, 4, PrefabSize.Eight, PrefabSize.Four, PrefabStyle.Block),
              (b, scene) => new PrefabScenePart(b, scene, 48, 4, PrefabSize.Eight, PrefabSize.Four, PrefabStyle.Block),
               (b, scene) => new PrefabScenePart(b, scene, 56, 4, PrefabSize.Eight, PrefabSize.Four, PrefabStyle.Block),


              (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, x: 16, y: 0, definition: scene),
              (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, x: 44, y: 0, definition: scene),
              (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, x: 40, y: 0, definition: scene),

              (b, scene) => new ExitScenePart(b, ExitType.Left, -2, scene),
              (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene)

            );

            AddLevel(
              Level.Level4_6_PyramidEntrance,
              builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
              (b, scene) => new ExitScenePart(b, ExitType.Left, -1, scene),
              (b, scene) => new ExitScenePart(b, ExitType.Right, 7, scene),
              (b, scene) => new SpriteScenePart(b, ScenePartType.Bomb, x: 4, y: 0, definition: scene),
              (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, x: 10, y: 0, definition: scene),
              (b, scene) => new PrefabScenePart(b, scene, 4, 24, PrefabSize.Four, PrefabSize.Four, PrefabStyle.Block),
              (b, scene) => new PrefabScenePart(b, scene, 8, 20, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Block),
              (b, scene) => new PrefabScenePart(b, scene, 16, 24, PrefabSize.Four, PrefabSize.Four, PrefabStyle.Block),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 28, y: 24, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 28, y: 22, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 28, y: 20, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 28, y: 18, definition: scene),

              (b, scene) => new SpriteScenePart(b, ScenePartType.DoorFowardExit, 12, 20, scene)
            );

            AddLevel(
              Level.Level4_7_Pit,
              builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
              (b, scene) => new ExitScenePart(b, ExitType.Top, -1, scene),
              (b, scene) => new PrefabScenePart(b, scene, 0, 4, PrefabSize.Four, PrefabSize.Four, PrefabStyle.Space),
              (b, scene) => new SpriteScenePart(b, ScenePartType.DoorBackExit, 2, 8, scene),
              (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 4, 16, scene),
              (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 6, 16, scene),
              (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 8, 16, scene),
              (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 10, 16, scene),

              (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 4, 40, scene),
              (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 6, 40, scene),
              (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 8, 40, scene),
              (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 10, 40, scene),


              (b, scene) => new ExitScenePart(b, ExitType.Bottom, 1, scene)
            );

            AddLevel(
              Level.Level4_8_Chamber,
              builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
              (b, scene) => new ExitScenePart(b, ExitType.Left, 1, scene),
              (b, scene) => new ExitScenePart(b, ExitType.Right, 4, scene),
              (b, scene) => new ExitScenePart(b, ExitType.Top, -1, scene)
            );

            AddLevel(
              Level.Level4_9_Chamber2,
              builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
              (b, scene) => new ExitScenePart(b, ExitType.Left, 2, scene),
              (b, scene) => new ExitScenePart(b, ExitType.Right, -1, scene),
              (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 32, 10, scene),
              (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 32, 10, scene),
              (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 16, 10, scene),
              (b, scene) => new SpriteScenePart(b, ScenePartType.DoorFowardExit, 12, 12, scene),
              (b, scene) => new PrefabScenePart(b, scene, 48, 0, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Space),
              (b, scene) => new PrefabScenePart(b, scene, 40, 0, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Space),
              (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 44, 12, scene),
              (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 52, 12, scene)

            );

            AddLevel(
             Level.Level4_10_SwitchRoom,
             builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
             (b, scene) => new SpriteScenePart(b, ScenePartType.DoorBackExit, 2, 12, scene),
             (b, scene) => new SpriteScenePart(b, ScenePartType.Button, 8, 7, scene),
             (b, scene) => new ExitScenePart(b, ExitType.Right, -3, scene),
             (b, scene) => new PrefabScenePart(b, scene, 12, 0, PrefabSize.Four, PrefabSize.Four, PrefabStyle.Space),
             (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: false, bottomRight: true, x: 12, y: 0, definition: scene),
             (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: false, x: 14, y: 0, definition: scene),
             (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: false, topRight: true, bottomLeft: false, bottomRight: true, x: 12, y: 2, definition: scene),
             (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: false, bottomLeft: true, bottomRight: false, x: 14, y: 2, definition: scene)

             );

            AddLevel(
              Level.Level4_11_DeadEnd,
              builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
              (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 8, 12, scene),
              (b, scene) => new ExitScenePart(b, ExitType.Right, -2, scene)
            );

            AddLevel(
              Level.Level4_12_DeadEnd2,
              builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
              (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 12, 12, scene),
              (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 30, 12, scene),
              (b, scene) => new SpriteScenePart(b, ScenePartType.Bomb, 34, 12, scene),

              (b, scene) => new ExitScenePart(b, ExitType.Left, -4, scene)
            );

            AddLevel(
              Level.Level4_13_PyramidExit,
              builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
              (b, scene) => new ExitScenePart(b, ExitType.Right, 2, scene),
              (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 30, 12, scene),
              (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 28, 0, scene),
              (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 38, 0, scene),

              (b, scene) => new PrefabScenePart(b, scene, 56, 4, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Block),

              (b, scene) => new PrefabScenePart(b, scene, 4, 4, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Block),
              (b, scene) => new PrefabScenePart(b, scene, 12, 4, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Block),
              (b, scene) => new PrefabScenePart(b, scene, 32, 4, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Block),
              (b, scene) => new PrefabScenePart(b, scene, 40, 4, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Block)

            );

            AddLevel(
              Level.Level4_14_DeadEnd3,
              builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
              (b, scene) => new SpriteScenePart(b, ScenePartType.DoorFowardExit, 6, 10, scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: true, x: 4, y: 8, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: false, x: 4, y: 6, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: false, x: 6, y: 6, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: false, x: 8, y: 8, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: true, x: 8, y: 6, definition: scene)
            );

            AddLevel(
              Level.Level4_15_Steps,
              builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
              (b, scene) => new ExitScenePart(b, ExitType.Left, -2, scene),
              (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene),
              (b, scene) => new SpriteScenePart(b, ScenePartType.DoorBackExit, 2, 10, scene)
            );

            AddLevel(
              Level.Level4_16_PyramidEntrance2,
              builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
              (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene),

              (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: false, bottomLeft: true, bottomRight: false, x: 6, y: 24, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: false, bottomLeft: true, bottomRight: false, x: 6, y: 22, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: false, bottomLeft: true, bottomRight: false, x: 6, y: 20, definition: scene),

              (b, scene) => new SpriteScenePart(b, ScenePartType.Button, 4, 27, scene),

              (b, scene) => new ExitScenePart(b, ExitType.Bottom, 2, scene),
              (b, scene) => new PrefabScenePart(b, scene, 16, 26, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Space),
              (b, scene) => new PrefabScenePart(b, scene, 24, 26, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Space),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 22, y: 24, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 24, y: 24, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 26, y: 24, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 28, y: 24, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 30, y: 24, definition: scene)


            );

            AddLevel(
             Level.Level4_17_Before_Midboss,
             builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
             (b, scene) => new ExitScenePart(b, ExitType.Right, 7, scene),
             (b, scene) => new PrefabScenePart(b, scene, 0, 8, PrefabSize.Eight, PrefabSize.Four, PrefabStyle.Space)
            );

            AddLevel(
              Level.Level4_18_Pit2,
              builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
              (b, scene) => new ExitScenePart(b, ExitType.Right, 2, scene),
              (b, scene) => new ExitScenePart(b, ExitType.Top, -2, scene),
              (b, scene) => new ExitScenePart(b, ExitType.Left, 1, scene),
              (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 8, 20, scene),

              (b, scene) => new SpriteScenePart(b, ScenePartType.Bomb, 12, 58, scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 8, y: 56, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 8, y: 58, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 10, y: 56, definition: scene),
              (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 10, y: 58, definition: scene),
              (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len16, 2, 40, scene),
              (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len24, 12, 28, scene),
              (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len24, 2, 20, scene),
              (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len24, 12, 12, scene),

              (b, scene) => new PrefabScenePart(b, scene, 14, 32, PrefabSize.Eight, PrefabSize.Four, PrefabStyle.Space),
              (b, scene) => new PrefabScenePart(b, scene, 0, 42, PrefabSize.Four, PrefabSize.Four, PrefabStyle.Space),
              (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len16, 8,4, scene),
              (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len16, 12, 50, scene),
              (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len16, 2, 56, scene)
            );

            AddLevel(
             Level.Level4_19_Chamber3,
             builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
             (b, scene) => new ExitScenePart(b, ExitType.Right, -1, scene),
             (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 3, 6, scene),
             (b, scene) => new ExitScenePart(b, ExitType.Bottom, 3, scene),
             (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 2, y: 2, definition: scene),
             (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: false, x: 2, y: 4, definition: scene),
             (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: false, x: 4, y: 2, definition: scene)



           );

            AddLevel(
            Level.Level4_20_DeadEnd4,
            builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
            (b, scene) => new ExitScenePart(b, ExitType.Left, -2, scene)
          );

            AddLevel(
              Level.Level4_21_Chamber4,
              builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
              (b, scene) => new ExitScenePart(b, ExitType.Left, -3, scene),
              (b, scene) => new ExitScenePart(b, ExitType.Top, -6, scene),
              (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 15, 12, scene),
              (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 40, 12, scene)
            );

            AddLevel(
           Level.Level4_22_Chamber5,
           builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
           (b, scene) => new PrefabScenePart(b, scene, 14, 0, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Space),
           (b, scene) => new PrefabScenePart(b, scene, 28, 14, PrefabSize.Four, PrefabSize.Four, PrefabStyle.Space),
           (b, scene) => new PrefabScenePart(b, scene, 14, 22, PrefabSize.Six, PrefabSize.Four, PrefabStyle.Space),
           (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: true, bottomRight: true, x: 12, y: 24, definition: scene),
           (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: true, bottomRight: true, x: 10, y: 24, definition: scene),
           (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: true, bottomRight: true, x: 08, y: 24, definition: scene),
           (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: true, bottomRight: true, x: 18, y: 24, definition: scene),
           (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: true, bottomRight: true, x: 20, y: 24, definition: scene),
           (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: true, bottomRight: true, x: 22, y: 24, definition: scene),

           (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 15, 18, scene),
           (b, scene) => new SpriteScenePart(b, ScenePartType.Bomb, 15, 22, scene),

           (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 4, 20, scene),
           (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 8, 24, scene),
           (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 4, 16, scene),
           (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 8, 12, scene),

           (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 22, 24, scene),
           (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 18, 20, scene),
           (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 24, 16, scene),

           (b, scene) => new ExitScenePart(b, ExitType.Left, 3, scene),
           (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene),
           (b, scene) => new ExitScenePart(b, ExitType.Top, -3, scene)
         );

        AddLevel(
            Level.Level4_23_Chamber6,
            builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
            (b, scene) => new ExitScenePart(b, ExitType.Left, -1, scene),
            (b, scene) => new ExitScenePart(b, ExitType.Right, 7, scene),
            (b, scene) => new PrefabScenePart(b, scene, 8, 0, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Space),
            (b, scene) => new PrefabScenePart(b, scene, 16, 0, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Space),
            (b, scene) => new PrefabScenePart(b, scene, 24, 0, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Space),
            (b, scene) => new PrefabScenePart(b, scene, 32, 0, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Space),
            (b, scene) => new PrefabScenePart(b, scene, 40, 0, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Space),
            (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 58, 10, scene),

            (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 12, 8, scene),
            (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Vanishing, PlatformDistance.Len16, 8, 12, scene),
            (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 20, 14, scene),
            (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len16, 28, 10, scene),
            (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Vanishing, PlatformDistance.Len24, 36, 12, scene),
            (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 44, 12, scene),
            (b, scene) => new ExitScenePart(b, ExitType.Top, -2, scene)
        );

            AddLevel(
              Level.Level4_24_BeforeMidboss2,
              builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
              (b, scene) => new ExitScenePart(b, ExitType.Left, -7, scene),         
              (b, scene) => new ExitScenePart(b, ExitType.Right, 7, scene)
            );

           AddLevel(
            Level.Level4_25_DeadEnd5,
                builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: true, bottomLeft: true, bottomRight: true, x: 2, y: 10, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 4, y: 10, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 6, y: 10, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 8, y: 10, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: false, bottomLeft: true, bottomRight: true, x: 10, y: 10, definition: scene),

                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: true, bottomLeft: true, bottomRight: true, x: 4, y: 8, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 6, y: 8, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: false, bottomLeft: true, bottomRight: true, x: 8, y: 8, definition: scene),

                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: true, bottomRight: true, x: 6, y: 6, definition: scene),

                (b, scene) => new ExitScenePart(b, ExitType.Right, -3, scene)
      );

            AddLevel(
           Level.Level4_26_Pit3,
           builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
           (b, scene) => new ExitScenePart(b, ExitType.Bottom, 4, scene),
           (b, scene) => new ExitScenePart(b, ExitType.Left, 1, scene),
           (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 11, 53, scene),
           (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 11, 35, scene),
           (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 11, 16, scene),

           (b, scene) => new PrefabScenePart(b, scene, 0, 20, PrefabSize.Eight, PrefabSize.Six, PrefabStyle.Space),
           (b, scene) => new PrefabScenePart(b, scene, 12, 20, PrefabSize.Eight, PrefabSize.Six, PrefabStyle.Space),
           (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 2, 58, scene),
           (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 12, 52, scene),
           (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Vanishing, PlatformDistance.Len16, 12, 48, scene),
           (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 2, 40, scene),
           (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Vanishing, PlatformDistance.Len24, 4, 44, scene),
           (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 10, 36, scene),
           (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 12, 32, scene),
           (b, scene) => new ExitScenePart(b, ExitType.Right, 2, scene)
         );

            AddLevel(
         Level.Level4_27_Chamber8,
         builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
         (b, scene) => new ExitScenePart(b, ExitType.Bottom, -6, scene),
         (b, scene) => new ExitScenePart(b, ExitType.Left, 2, scene),
         (b, scene) => new ExitScenePart(b, ExitType.Right, -1, scene)
       );

        AddLevel(
            Level.Level4_28_SwitchRoom2,
            builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
            (b, scene) => new SpriteScenePart(b, ScenePartType.Button, 4, 9, scene),
            (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 16, 18, scene),

            (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: false, bottomLeft: false, bottomRight: true, x: 8, y: 8, definition: scene),
            (b, scene) => new DynamicScenePart(b, DynamicBlockType.SwitchBlock, topLeft: true, topRight: false, bottomLeft: false, bottomRight: true, x: 10, y: 10, definition: scene),

            (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: false, x: 6, y: 20, definition: scene),
            (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 8, y: 20, definition: scene),
            (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: false, bottomRight: false, x: 10, y: 20, definition: scene),


            (b, scene) => new PrefabScenePart(b, scene, 16, 16, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.StairUp),
            (b, scene) => new PrefabScenePart(b, scene, 24, 16, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Block),
            (b, scene) => new PrefabScenePart(b, scene, 18, 24, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Block),
            (b, scene) => new PrefabScenePart(b, scene, 4, 8, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.StairDown),
            (b, scene) => new PrefabScenePart(b, scene, 16, 8, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Space),
            (b, scene) => new ExitScenePart(b, ExitType.Left, -2, scene)
      );

          AddLevel(
            Level.Level4_29_DeadEnd6,
            builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
            (b, scene) => new ExitScenePart(b, ExitType.Right, -2, scene)
          );

            AddLevel(
           Level.Level4_30_Chamber7,
           builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
           (b, scene) => new ExitScenePart(b, ExitType.Left, -7, scene),
           (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: true, bottomLeft: true, bottomRight: true, x: 12, y: 10, definition: scene),
           (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: true, bottomRight: true, x: 10, y: 10, definition: scene),
           (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: true, bottomLeft: false, bottomRight: true, x: 12, y: 8, definition: scene),
           (b, scene) => new PrefabScenePart(b, scene, 4, 4, PrefabSize.Eight, PrefabSize.Six, PrefabStyle.StairDown),
           (b, scene) => new ExitScenePart(b, ExitType.Top, -4, scene)
         );

            AddLevel(
                Level.Level4_31_Midboss,
                builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
                (b, scene) => new ExitScenePart(b, ExitType.Left, -7, scene),
                (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 16, 16, scene),
                                
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 2, y: 10, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 2, y: 8, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 2, y: 6, definition: scene),


                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: true, bottomRight: true, x: 4, y: 10, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: true, bottomRight: true, x: 6, y: 10, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: true, bottomRight: true, x: 8, y: 10, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: true, bottomRight: true, x: 10, y: 10, definition: scene),
             
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 12, y: 10, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 12, y: 8, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 12, y: 6, definition: scene),

                (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene)
              );

            AddLevel(
             Level.Level4_32_Desert4,
             builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
             (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene),

             (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 28, 12, scene),
             (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_UpDown, PlatformDistance.Len16, 36, 8, scene),

             (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 40, 8, scene),           
             (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 48, 12, scene),

             (b, scene) => new TurretScenePart(b, scene, Direction.Up, 40, 12),
             (b, scene) => new TurretScenePart(b, scene, Direction.Left, 54, 8),

             (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 18, 0, scene),

             (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, false, true, true, true, 14, 8, scene),
             (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, true, true, 22, 8, scene),
       
             (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, false, true, true, true, 56, 8, scene),
             (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, false, true, true, 58, 8, scene),

             (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true,true,true,true, 16, 8, scene),
             (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 18, 8, scene),
             (b, scene) => new DynamicScenePart(b, DynamicBlockType.DestructibleBlock, true, true, true, true, 20, 8, scene),
             (b, scene) => new PrefabScenePart(b, scene, 24, 0, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Space),
             (b, scene) => new PrefabScenePart(b, scene, 32, 0, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Space),
             (b, scene) => new PrefabScenePart(b, scene, 40, 0, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Space),
             (b, scene) => new PrefabScenePart(b, scene, 48, 0, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Space)
     
           );

            AddLevel(
            Level.Level4_33_Desert5,
            builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
            (b, scene) => new TurretScenePart(b, scene, Direction.Right, 10, 8),
            (b, scene) => new TurretScenePart(b, scene, Direction.Up, 18, 10),
            (b, scene) => new TurretScenePart(b, scene, Direction.Right, 28, 8),
            (b, scene) => new TurretScenePart(b, scene, Direction.Up, 24, 10),

            (b, scene) => new SpriteScenePart(b, ScenePartType.Bomb, 20, 0, scene),
            (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 16, 0, scene),
            (b, scene) => new ExitScenePart(b, ExitType.Left, -1, scene),
            (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene)
          );


            AddLevel(
                Level.Level4_34_DesertRain1,
                builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
                (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 8, 10, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 6, 10, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 4, 10, scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 6, 8, scene),

                (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 12, 0, scene)
              );


            AddLevel(
                Level.Level4_35_DesertRain2,
                builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
                (b, scene) => new ExitScenePart(b, ExitType.Left, -1, scene),
                (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene),
                (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 12, 0, scene)
              );

            AddLevel(
              Level.Level4_36_DesertRain3,
              builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
              (b, scene) => new ExitScenePart(b, ExitType.Left, -1, scene),
              (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene),
              (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Vanishing, PlatformDistance.Len16, 4, 8, scene),
              (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 8, 0, scene)
            );

            AddLevel(
            Level.Level4_37_DesertRain4,
            builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
            (b, scene) => new PrefabScenePart(b, scene, 8, 52, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.StairUp),
            (b, scene) => new PrefabScenePart(b, scene, 16, 52, PrefabSize.Four, PrefabSize.Eight, PrefabStyle.Block),
            (b, scene) => new PrefabScenePart(b, scene, 20, 52, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.StairDown),

            (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 8, 24, scene),
            (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 10, 22, scene),
            (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 12, 20, scene),
            (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, true, true, true, true, 14, 18, scene),
          
            (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 12, 20, scene),
               (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 18, 4, scene),

            (b, scene) => new PrefabScenePart(b, scene, 8, 40, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.StairDown),
            (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 16, 18, scene),
            (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 16, 16, scene),

            (b, scene) => new PrefabScenePart(b, scene, 18, 4, PrefabSize.Eight, PrefabSize.Two, PrefabStyle.Block),
            (b, scene) => new PrefabScenePart(b, scene, 26, 8, PrefabSize.Eight, PrefabSize.Two, PrefabStyle.Block),

            (b, scene) => new ExitScenePart(b, ExitType.Left, -1, scene),
            (b, scene) => new SpriteScenePart(b, ScenePartType.DoorFowardExit, 60, 8, scene)
          );

            AddLevel(
               Level.Level4_38_DesertRain5,
               builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,

                (b, scene) => new SpriteScenePart(b, ScenePartType.DoorBackExit, 4, 8, scene),
                (b, scene) => new PrefabScenePart(b, scene, 0, 8, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Block),
                (b, scene) => new PrefabScenePart(b, scene, 0, 16, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Block),
                (b, scene) => new PrefabScenePart(b, scene, 0, 24, PrefabSize.Eight, PrefabSize.Eight, PrefabStyle.Block),

                (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 12, 8, scene),
                (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 16, 10, scene),
                (b, scene) => new PlatformScenePart(b, ScenePartType.Platform_Falling, PlatformDistance.Len16, 20, 12, scene),
                            
                (b, scene) => new PrefabScenePart(b, scene, 30, 8, PrefabSize.Four, PrefabSize.Two, PrefabStyle.Block),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 30, y: 0, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 30, y: 2, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 30, y: 4, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 30, y: 6, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 28, y: 0, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 28, y: 2, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 28, y: 4, definition: scene),
                (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: true, topRight: true, bottomLeft: true, bottomRight: true, x: 28, y: 6, definition: scene),


                (b, scene) => new SpriteScenePart(b, ScenePartType.DoorFowardExit, 26, 26, scene)
             );

            AddLevel(
             Level.Level4_39_BeforeBoss,
             builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
             (b, scene) => new SpriteScenePart(b, ScenePartType.DoorBackExit, 0, 12, scene),
             (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene)
           );

            AddLevel(
               Level.Level4_40_Boss,
               builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
               (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, x: 0, y: 16, definition: scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 0, y: 0, definition: scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 2, y: 0, definition: scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 4, y: 0, definition: scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 6, y: 0, definition: scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 8, y: 0, definition: scene),
               (b, scene) => new DynamicScenePart(b, DynamicBlockType.Coin, topLeft: false, topRight: false, bottomLeft: false, bottomRight: false, x: 10, y: 0, definition: scene)
           );

            AddLevel(
              Level.Level5_1_Mist,
              builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
              (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene)              
            );

            AddLevel(
                Level.Level5_2_Bridges,
                builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,

                (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType1, 16, 4, scene),
                 (b, scene) => new SpriteScenePart(b, ScenePartType.EnemyType2, 24, 8, scene),

                (b, scene) => PitScenePart(b, 8, PrefabSize.Eight, scene),                
                (b, scene) => PitScenePart(b, 32, PrefabSize.Eight, scene),
                (b, scene) => PitScenePart(b, 40, PrefabSize.Eight, scene),

                (b, scene) => new ExitScenePart(b, ExitType.Left, -1, scene),
                (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene)
            );

            AddLevel(
                Level.Level5_3_Bridges2,
                builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
                (b, scene) => PitScenePart(b, 12, PrefabSize.Six, scene),
                (b, scene) => PitScenePart(b, 24, PrefabSize.Eight, scene),
                (b, scene) => PitScenePart(b, 48, PrefabSize.Six, scene),

                (b, scene) => new ExitScenePart(b, ExitType.Left, -1, scene),
                (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene)
            );

            AddLevel(
              Level.Level5_4_Mist2,
              builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
              (b, scene) => new ExitScenePart(b, ExitType.Left, -1, scene),
              (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene)
            );

            AddLevel(
              Level.Level5_5_Forest,
              builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
              (b, scene) => new ExitScenePart(b, ExitType.Left, -1, scene),
              (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene)
            );

            AddLevel(
             Level.Level5_6_Forest_Hub,
             builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
             (b, scene) => new ExitScenePart(b, ExitType.Left, -1, scene),
             (b, scene) => new ExitScenePart(b, ExitType.Bottom, 2, scene),
             (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene)
           );

            AddLevel(
              Level.Level5_7_Forest2,
              builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
              (b, scene) => new ExitScenePart(b, ExitType.Left, -1, scene),
              (b, scene) => new ExitScenePart(b, ExitType.Right, 2, scene)
            );

            AddLevel(
              Level.Level5_8_Forest2,
              builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
              (b, scene) => new ExitScenePart(b, ExitType.Top, -2, scene),
              (b, scene) => new ExitScenePart(b, ExitType.Right, 4, scene)
            );

            AddLevel(
                Level.Level5_9_Forest_Corner,
                builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
                (b, scene) => new ExitScenePart(b, ExitType.Left, -2, scene),
                (b, scene) => new ExitScenePart(b, ExitType.Bottom, 1, scene)
            );

            AddLevel(
                Level.Level5_10_Forest3,
                builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
                (b, scene) => new ExitScenePart(b, ExitType.Top, -1, scene),
                (b, scene) => new ExitScenePart(b, ExitType.Left, 1, scene)
            );

            AddLevel(
               Level.Level5_11_Forest_Path,
               builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
               (b, scene) => new ExitScenePart(b, ExitType.Right, -1, scene),
               (b, scene) => new ExitScenePart(b, ExitType.Left, 1, scene)
           );

            AddLevel(
             Level.Level5_12_Forest_Hub2,
             builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
             (b, scene) => new ExitScenePart(b, ExitType.Left, -4, scene),
             (b, scene) => new ExitScenePart(b, ExitType.Bottom, 1, scene),
             (b, scene) => new ExitScenePart(b, ExitType.Right, -1, scene)
            );

            AddLevel(
               Level.Level5_13_Forest_Corner2,
               builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
               (b, scene) => new ExitScenePart(b, ExitType.Top, -1, scene),
               (b, scene) => new ExitScenePart(b, ExitType.Left, 1, scene)
            );

            AddLevel(
             Level.Level5_14_Forest4,
             builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
             (b, scene) => new ExitScenePart(b, ExitType.Right, -1, scene),
             (b, scene) => new ExitScenePart(b, ExitType.Left, 1, scene)
          );

            AddLevel(
            Level.Level5_15_Forest_Corner3,
            builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
            (b, scene) => new ExitScenePart(b, ExitType.Right, -1, scene),
            (b, scene) => new ExitScenePart(b, ExitType.Bottom, 3, scene)
         );

            AddLevel(
            Level.Level5_16_Forest_Corner4,
            builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
            (b, scene) => new SpriteScenePart(b, ScenePartType.DoorFowardExit, 4, 12, scene),
            (b, scene) => new ExitScenePart(b, ExitType.Right, 4, scene)
         );

            AddLevel(
            Level.Level5_17_Forest5,
            builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
            (b, scene) => new SpriteScenePart(b, ScenePartType.DoorBackExit, 4, 10, scene),
            (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene)
         );

            AddLevel(
             Level.Level5_18_Forest_Hub3,
             builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
             (b, scene) => new ExitScenePart(b, ExitType.Left, -1, scene),
             (b, scene) => new ExitScenePart(b, ExitType.Top, -3, scene),
             (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene)
            );

            AddLevel(
            Level.Level5_19_Forest6,
            builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
            (b, scene) => new ExitScenePart(b, ExitType.Left, -1, scene)
         );


            AddLevel(
            Level.Level5_20_Mist3,
            builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
            (b, scene) => PitScenePart(b, 8, PrefabSize.Four, scene),
            (b, scene) => PitScenePart(b, 32, PrefabSize.Four, scene),
            (b, scene) => PitScenePart(b, 36, PrefabSize.Six, scene),
            (b, scene) => new ExitScenePart(b, ExitType.Left, -4, scene),
            (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene)
         );


            AddLevel(
            Level.Level5_21_Mist4,
            builder, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded,
            (b, scene) => PitScenePart(b, 16, PrefabSize.Eight, scene),
            (b, scene) => PitScenePart(b, 24, PrefabSize.Eight, scene),
            (b, scene) => new ExitScenePart(b, ExitType.Left, -1, scene),
            (b, scene) => new ExitScenePart(b, ExitType.Right, 1, scene)
         );


        }

        private static PrefabScenePart PitScenePart(SystemMemoryBuilder b, byte x, PrefabSize width, SceneDefinition scene)
        {
            return new PrefabScenePart(b, scene, x, 10, width, PrefabSize.Eight, PrefabStyle.Space);
        }

        private static void AddLevel(Level level, 
            SystemMemoryBuilder memoryBuilder,             
            Specs specs,
            ref int destroyBitsNeeded,
            ref int maxDestroyBitsNeeded,
            params Func<SystemMemoryBuilder, SceneDefinition, IScenePart>[] parts)
        {
            var scene = new SceneDefinition(level, memoryBuilder.Memory, specs);
            var header = new ScenePartsHeader(memoryBuilder, scene, parts);
            CheckDestroyBits(level, header, scene, specs, ref destroyBitsNeeded, ref maxDestroyBitsNeeded);
        }

        private static void CheckDestroyBits(Level level, ScenePartsHeader header, SceneDefinition scene, Specs specs, 
            ref int destroyBitsRequired,
            ref int maxDestroyBitsRequired)
        {
            var extraBitsNeeded = header.DestroyBitsNeeded(scene, specs);

            if (TransitionLevels.Contains(level))
            {
                maxDestroyBitsRequired = Math.Max(maxDestroyBitsRequired, destroyBitsRequired);
                if (maxDestroyBitsRequired > 127)
                    throw new Exception("Too many destructible objects");
                destroyBitsRequired = extraBitsNeeded;
            }
            else
            {
                destroyBitsRequired += extraBitsNeeded;
                if (destroyBitsRequired > 127)
                    throw new Exception("Too many destructible objects");
            }
        }
    }
}
