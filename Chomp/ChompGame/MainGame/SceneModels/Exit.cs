using ChompGame.Data.Memory;
using ChompGame.MainGame.SceneModels.SceneParts;
using ChompGame.MainGame.SpriteControllers;

namespace ChompGame.MainGame.SceneModels
{
    enum ExitType : byte
    {
        None,
        Left,
        Right,
        Top,
        Bottom,
        DoorForward,
        DoorBack
    }

    class ExitsModule
    {
        private readonly ChompGameModule _gameModule;

        public ExitScenePart ActiveExit { get; private set; }

        public ExitsModule(ChompGameModule module)
        {
            _gameModule = module;
        }

        private void SetActiveExit(ExitScenePart exit)
        {
            _gameModule.GameSystem.Memory.BlockCopy(exit.Address, ActiveExit.Address, ScenePartsHeader.ScenePartBytes);
        }

        public void BuildMemory(SystemMemoryBuilder memoryBuilder, SceneDefinition sceneDefinition)
        {
            ActiveExit = new ExitScenePart(memoryBuilder, ExitType.None, 1, sceneDefinition);                               
        }

        public void OnDoorEntered(ExitType exitType)
        {
            ActiveExit.ExitType = exitType;
        }

        public void CheckExits(PlayerController player, SceneDefinition sceneDefinition)
        {
            if (ActiveExit.ExitType != ExitType.None)
                return;

            if (_gameModule.CurrentScene.IsAutoScroll)
            {
                CheckAutoscrollLevelEnd(player);
                return;
            }
                

            DynamicScenePartHeader header = _gameModule.CurrentScenePartHeader;

            int rightEdge = (sceneDefinition.LevelTileWidth - 1) * _gameModule.Specs.TileWidth;
            int bottomEdge = (sceneDefinition.LevelTileHeight - 2) * _gameModule.Specs.TileWidth;

            if (player.WorldSprite.X != 0
                && player.WorldSprite.X != rightEdge
                && player.WorldSprite.Y != bottomEdge)
            {
                return;
            }

            for (int i = 0; i < header.PartsCount; i++)
            {
                if (header.IsPartActivated(i))
                    continue;

                var sp = header.GetExitScenePart(i, sceneDefinition, _gameModule.Specs);

                if (sp.Type != ScenePartType.SideExit)
                    continue;

                if (player.WorldSprite.X == rightEdge
                    && sp.ExitType == ExitType.Right)
                {
                    SetActiveExit(sp);
                    return;
                }
                else if (player.WorldSprite.X == 0
                    && sp.ExitType == ExitType.Left)
                {
                    SetActiveExit(sp);
                    return;
                }
                else if (player.WorldSprite.Y == bottomEdge
                    && sp.ExitType == ExitType.Bottom)
                {
                    SetActiveExit(sp);
                    return;
                }
            }
        }

        private void CheckAutoscrollLevelEnd(PlayerController player)
        {
            if (player.WorldSprite.X < _gameModule.Specs.ScreenWidth)
                return;

            ActiveExit.SetForAutoscrollLevelEnd();
        }
    }
}
