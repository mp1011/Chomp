using ChompGame.Data;

namespace ChompGame.MainGame.SceneModels
{
    class TileMasterNameTable : NBitPlane
    {
        public TileMasterNameTable(int address, SystemMemory memory) 
            : base(address, memory, 4, 8, 8)
        {
        }
    }
}
