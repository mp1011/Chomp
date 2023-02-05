using ChompGame.Data.Memory;
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

        public ScenePart ActiveExit { get; private set; }

        public ExitsModule(ChompGameModule module)
        {
            _gameModule = module;
        }

        private void SetActiveExit(ScenePart exit)
        {
            _gameModule.GameSystem.Memory.BlockCopy(exit.Address, ActiveExit.Address, ScenePart.Bytes);
        }

        public void BuildMemory(SystemMemoryBuilder memoryBuilder, SceneDefinition sceneDefinition)
        {
            ActiveExit = new ScenePart(memoryBuilder, ExitType.None, 1, sceneDefinition);                               
        }

        public void OnDoorEntered(ExitType exitType)
        {
            ActiveExit.ExitType = exitType;
        }

        public void CheckExits(PlayerController player, SceneDefinition sceneDefinition)
        {
            if (ActiveExit.ExitType != ExitType.None)
                return;

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

                ScenePart sp = header.GetScenePart(i, sceneDefinition);

                if (sp.Type != ScenePartType.SideExit)
                    continue;

                if(player.WorldSprite.X == rightEdge
                    && sp.ExitType == ExitType.Right)
                {
                    SetActiveExit(sp);
                    return;
                }
                else if(player.WorldSprite.X == 0
                    && sp.ExitType == ExitType.Left)
                {
                    SetActiveExit(sp);
                    return;
                }
                else if(player.WorldSprite.Y == bottomEdge
                    && sp.ExitType == ExitType.Bottom)
                {
                    SetActiveExit(sp);
                    return;
                }
            }
        }
    }
}
