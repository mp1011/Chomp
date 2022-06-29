using ChompGame.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChompGame.GameSystem
{
    class CorruptionTestModule : Module, ILogicUpdateModule
    {
        public CorruptionTestModule(MainSystem mainSystem) : base(mainSystem)
        {
        }

        public override void OnStartup()
        {
        
        }

        int i = 0;

        public void OnLogicUpdate()
        {
            //i++;
            //if(i == 16)
            //{
            //    i = 0;
            //    GameSystem.Memory.CorruptBit(new Random().Next(GameSystem.Memory.RAMSize * 8));
            //}
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
        }
    }
}
