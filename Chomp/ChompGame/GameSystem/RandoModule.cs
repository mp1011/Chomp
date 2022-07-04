using ChompGame.Data;
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
            var rng = new Random();
            for (int i = 0; i < GameSystem.Memory.RAMSize-1; i++)
            {
                GameSystem.Memory[i] = (byte)rng.Next(0, 256);
            }
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
            memoryBuilder.BeginROM();
        }
    }
}
