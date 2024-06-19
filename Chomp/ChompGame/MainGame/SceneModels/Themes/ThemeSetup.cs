using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels.SmartBackground;
using System;
using System.Collections.Generic;

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
        public virtual IEnumerable<SmartBackgroundBlock> SmartBackgroundBlocks => Array.Empty<SmartBackgroundBlock>();
        public virtual NBitPlane BuildAttributeTable(NBitPlane attributeTable, NBitPlane nameTable)
        {
            int foreGroundAttributePosition = _sceneDefinition.GetBackgroundLayerTile(BackgroundLayer.ForegroundStart, false) / _specs.AttributeTableBlockSize;
            if (_sceneDefinition.IsAutoScroll)
            {
                foreGroundAttributePosition += 2;
            }

            attributeTable.ForEach((x, y, b) =>
            {
                bool isSolid = nameTable[x * 2, y * 2] != 0
                    || nameTable[(x * 2) + 1, (y * 2) + 1] != 0;

                if (isSolid || y >= foreGroundAttributePosition)
                    attributeTable[x, y] = 1;
                else
                    attributeTable[x, y] = 0;

            });

            return attributeTable;
        }

        public static ThemeSetup Create(ThemeType themeType, Specs specs, SceneDefinition sceneDefinition)
        {
            ThemeSetup setup = themeType switch {
                ThemeType.Plains => new PlainsThemeSetup(),
                ThemeType.PlainsEvening => new PlainsThemeSetup(),
                ThemeType.PlainsBoss => new PlainsBossThemeSetup(),
                ThemeType.Ocean => new OceanThemeSetup(),
                ThemeType.OceanAutoscroll => new OceanAutoscrollThemeSetup(),
                ThemeType.OceanBoss => new OceanBossThemeSetup(),
                ThemeType.City => new CityThemeSetup(),
                ThemeType.CityEvening => new CityThemeSetup(),
                ThemeType.CityTrain => new CityTrainThemeSetup(),
                ThemeType.CityInterior => new CityInteriorThemeSetup(),
                ThemeType.CityBoss => new CityInteriorThemeSetup(),
                _ => throw new NotImplementedException()
            };

            setup._specs = specs;
            setup._sceneDefinition = sceneDefinition;
            return setup;
        }
    }
}
