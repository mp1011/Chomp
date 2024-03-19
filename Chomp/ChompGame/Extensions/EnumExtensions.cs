using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteModels;
using System;

namespace ChompGame.Extensions
{
    static class EnumExtensions
    {

        public static bool Between<T>(this T value, T min, T max)
            where T:Enum
        {
            var byteValue = (byte)(object)value;
            var byteMin = (byte)(object)min;
            var byteMax = (byte)(object)max;
            return byteValue >= byteMin && byteValue < byteMax;
        }

        public static byte DestroyBitsRequired(this ScenePartType type) =>
            type switch
            {
                ScenePartType.Bomb => 1,
                ScenePartType.EnemyType1 => 1,
                ScenePartType.EnemyType2 => 1,
                ScenePartType.DestructibleBlock => 1,
                ScenePartType.Coin => 4,
                _ => 0
            };

        public static SpriteType ToSpriteType(this EnemyIndex e) =>
            e switch
            {
                EnemyIndex.Bird => SpriteType.Bird,
                EnemyIndex.Boulder => SpriteType.Boulder,
                EnemyIndex.Crocodile => SpriteType.Crocodile,
                EnemyIndex.Lizard => SpriteType.Lizard,
                EnemyIndex.Ogre => SpriteType.Ogre,
                EnemyIndex.Rocket => SpriteType.Rocket,
                EnemyIndex.Ufo => SpriteType.Ufo,
                _ => SpriteType.Mage,
            };

        public static SpriteType ToBossSpriteType(this EnemyIndex e) =>
           e switch
           {
               EnemyIndex.Midboss => SpriteType.Chomp,
               _ => SpriteType.LevelBoss,
           };
    }
}
