using ChompGame.Data;
using ChompGame.Data.Memory;
using System;

namespace ChompGame.GameSystem
{
    class RandoModule : Module, IMasterModule
    {
        public RandoModule(MainSystem mainSystem) : base(mainSystem)
        {
        }

        public override void OnStartup()
        {
            
        }

        public void OnHBlank()
        {
        }

        public void OnLogicUpdate()
        {
        }

        public void OnVBlank()
        {
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
        }

        public byte GetPalette(int pixel)
        {
            return 0;
        }
    }
}
