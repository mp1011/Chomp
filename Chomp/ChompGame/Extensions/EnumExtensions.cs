using ChompGame.MainGame.SceneModels;

namespace ChompGame.Extensions
{
    static class EnumExtensions
    {
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
