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
        protected ChompGameModule _gameModule;

        public abstract void SetupVRAMPatternTable(
          NBitPlane masterPatternTable,
          NBitPlane vramPatternTable,
          SystemMemory memory);

        protected ThemeSetup(ChompGameModule gameModule)
        {
            _gameModule = gameModule;
        }

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

        public static ThemeSetup Create(ThemeType themeType, Specs specs, SceneDefinition sceneDefinition, 
            ChompGameModule chompGameModule)
        {
            ThemeSetup setup = themeType switch {
                ThemeType.Plains => new PlainsThemeSetup(chompGameModule),
                ThemeType.PlainsEvening => new PlainsThemeSetup(chompGameModule),
                ThemeType.PlainsBoss => new PlainsBossThemeSetup(chompGameModule),
                ThemeType.Ocean => new OceanThemeSetup(chompGameModule),
                ThemeType.OceanAutoscroll => new OceanAutoscrollThemeSetup(chompGameModule),
                ThemeType.OceanBoss => new OceanBossThemeSetup(chompGameModule),
                ThemeType.City => new CityThemeSetup(chompGameModule),
                ThemeType.CityEvening => new CityThemeSetup(chompGameModule),
                ThemeType.CityTrain => new CityTrainThemeSetup(chompGameModule),
                ThemeType.CityInterior => new CityInteriorThemeSetup(chompGameModule),
                ThemeType.CityBoss => new CityBossThemeSetup(chompGameModule),
                ThemeType.Desert => new DesertThemeSetup(chompGameModule),
                ThemeType.DesertInterior => new DesertInteriorThemeSetup(chompGameModule),
                _ => throw new NotImplementedException()
            };

            setup._specs = specs;
            setup._sceneDefinition = sceneDefinition;
            return setup;
        }
    }
}
