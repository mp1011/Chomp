using ChompGame.MainGame.SceneModels;
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
    }
}
