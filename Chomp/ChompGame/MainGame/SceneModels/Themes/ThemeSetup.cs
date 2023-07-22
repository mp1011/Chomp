using ChompGame.Data;
using ChompGame.GameSystem;
using System;

namespace ChompGame.MainGame.SceneModels.Themes
{
    abstract class ThemeSetup
    {
        protected Specs _specs;
        protected SceneDefinition _sceneDefinition;

        public abstract void SetupVRAMPatternTable(
          NBitPlane masterPatternTable,
          NBitPlane vramPatternTable,
          SystemMemory memory);

        public abstract void BuildBackgroundNameTable(NBitPlane nameTable);

        public static ThemeSetup Create(ThemeType themeType, Specs specs, SceneDefinition sceneDefinition)
        {
            ThemeSetup setup = themeType switch {
                ThemeType.Plains => new PlainsThemeSetup(),
                ThemeType.PlainsEvening => new PlainsThemeSetup(),
                ThemeType.PlainsBoss => new PlainsBossThemeSetup(),
                ThemeType.Ocean => new OceanThemeSetup(),
                _ => throw new NotImplementedException()
            };

            setup._specs = specs;
            setup._sceneDefinition = sceneDefinition;
            return setup;
        }
    }
}
