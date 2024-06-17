using ChompGame.MainGame.SceneModels;

namespace ChompGame.Extensions
{
    static class ThemeTypeExtensions
    {
        public static bool IsCityTheme(this ThemeType t) => t == ThemeType.City || t == ThemeType.CityEvening;
    }
}
