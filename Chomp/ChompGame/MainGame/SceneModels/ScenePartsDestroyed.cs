using ChompGame.Data;
using ChompGame.Data.Memory;

namespace ChompGame.MainGame.SceneModels
{
    class ScenePartsDestroyed
    {

        private BitArray _partsDestroyed;
        private GameByte _sceneOffset;

        public ScenePartsDestroyed(SystemMemoryBuilder memoryBuilder)
        {
            _sceneOffset = memoryBuilder.AddByte();

            _partsDestroyed = new BitArray(memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            memoryBuilder.AddBytes(32); //todo, figure out how much we need
        }

        public void SetCurrentLevel(Level level, SystemMemory memory)
        {
            byte sceneOffset=0;
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

        public bool IsDestroyed(byte offsetWithinScene) => _partsDestroyed[_sceneOffset.Value + offsetWithinScene];
        public bool IsDestroyed(int offsetWithinScene) => IsDestroyed((byte)offsetWithinScene);
        public void SetDestroyed(byte offsetWithinScene)
        {
            _partsDestroyed[_sceneOffset.Value + offsetWithinScene] = true;
        }
        public void SetDestroyed(int offsetWithinScene) => SetDestroyed((byte)offsetWithinScene);
    }
}
