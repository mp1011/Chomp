using ChompGame.MainGame.SpriteControllers;

namespace ChompGame.MainGame.SceneModels
{
    enum ExitType : byte
    {
        Left,
        Right,
        Top,
        Bottom,
        Door1,
        Door2
    }

    class ExitsModule
    {
        private readonly ChompGameModule _gameModule;

        public ExitsModule(ChompGameModule module)
        {
            _gameModule = module;
        }

        public int CheckExits(PlayerController player, SceneDefinition sceneDefinition)
        {
            DynamicScenePartHeader header = _gameModule.CurrentScenePartHeader;

            int rightEdge = (sceneDefinition.LevelTileWidth - 1) * _gameModule.Specs.TileWidth;

            if (player.WorldSprite.X != 0
                && player.WorldSprite.X != rightEdge)
            {
                return 0;
            }

            for (int i = 0; i < header.PartsCount; i++)
            {
                if (header.IsPartActivated(i))
                    continue;

                ScenePart sp = header.GetScenePart(i, sceneDefinition);

                if (sp.Type != ScenePartType.Exit)
                    continue;

                if(player.WorldSprite.X == rightEdge
                    && sp.ExitType == ExitType.Right)
                {
                    return sp.ExitLevelOffset;
                }
                else if(player.WorldSprite.X == 0
                    && sp.ExitType == ExitType.Left)
                {
                    return sp.ExitLevelOffset;
                }
            }

            return 0;
        }
    }
}
