using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.MainGame.SceneModels.SceneParts;

namespace ChompGame.MainGame.SceneModels
{
    class ScenePartsDestroyed
    {

        private BitArray _partsDestroyed;
        private GameByte _sceneOffset;

        private GameBit _switchBlocksOff;

        public bool SwitchBlocksOff
        {
            get => _switchBlocksOff.Value;
            set => _switchBlocksOff.Value = value;
        }

        public ScenePartsDestroyed(SystemMemoryBuilder memoryBuilder)
        {
            _sceneOffset = memoryBuilder.AddByte();

            _switchBlocksOff = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit0, memoryBuilder.Memory);
            _partsDestroyed = new BitArray(memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            memoryBuilder.AddBytes(32); //todo, figure out how much we need
        }

        public void SetCurrentLevel(Level level, SystemMemory memory)
        {
            byte sceneOffset=1;
            for (Level l = (Level)0; l < level; l++)
            {
                ScenePartsHeader header = new ScenePartsHeader(l, memory);
                for(int i = 0; i < header.PartsCount;i++)
                {
                    sceneOffset += header.GetScenePartDestroyBitsRequired(i);
                }
            }

            _sceneOffset.Value = sceneOffset;
        }

        public void OnSceneRestart(ChompGameModule gameModule)
        {
            byte nextDestructionBitOffset = 0;

            for (int i = 0; i < gameModule.CurrentScenePartHeader.PartsCount; i++)
            {
                var sp = gameModule.CurrentScenePartHeader.GetScenePart(i, gameModule.CurrentScene, gameModule.Specs);
                
                switch(sp.Type)
                {
                    case ScenePartType.EnemyType1:
                    case ScenePartType.EnemyType2:
                    case ScenePartType.Bomb:
                    case ScenePartType.DestructibleBlock:
                        _partsDestroyed[_sceneOffset.Value + nextDestructionBitOffset] = false;
                        break;
                }

                nextDestructionBitOffset += sp.DestroyBitsRequired;
            }
        }

        public bool IsDestroyed(byte offsetWithinScene) => offsetWithinScene == 255 ? false 
            : _partsDestroyed[_sceneOffset.Value + offsetWithinScene];
        public bool IsDestroyed(int offsetWithinScene) => IsDestroyed((byte)offsetWithinScene);
        public void SetDestroyed(byte offsetWithinScene)
        {
            if(offsetWithinScene!=255)
                _partsDestroyed[_sceneOffset.Value + offsetWithinScene] = true;
        }
        public void SetDestroyed(int offsetWithinScene) => SetDestroyed((byte)offsetWithinScene);
    }
}
