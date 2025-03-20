using ChompGame.Data;
using ChompGame.Data.Memory;
using System.Linq;

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
            _partsDestroyed = new BitArray(memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            _switchBlocksOff = new GameBit(memoryBuilder.CurrentAddress + 15, Bit.Bit7, memoryBuilder.Memory);
            memoryBuilder.AddBytes(16);            
        }

        public void Reset(SystemMemory memory)
        {
            for (int i = 0; i < 16; i++)
                memory[_partsDestroyed.Address + i] = 0;
        }

        public void SetCurrentLevel(Level level, SystemMemory memory)
        {
            Level transitionLevel = level;
            while (!SceneBuilder.TransitionLevels.Contains(transitionLevel))
                transitionLevel--;

            byte sceneOffset=1;
            for (Level l = transitionLevel; l < level; l++)
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


        public bool IsDestroyed2(int offset) => _partsDestroyed[offset];

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
