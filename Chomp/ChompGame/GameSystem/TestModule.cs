using ChompGame.Data;
using System;

namespace ChompGame.GameSystem
{
    class TestModule : Module, ILogicUpdateHandler
    {
        private TileModule _tileModule;

        public TestModule(MainSystem mainSystem) : base(mainSystem) { }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
        }

        public void OnLogicUpdate()
        {
            // GameSystem.Memory[_tileModule.NameTable.Address]++;
            GameSystem.Memory[GameSystem.CoreGraphicsModule.PatternTable.Address]++;
        }

        public override void OnStartup()
        {
            _tileModule = GameSystem.GetModule<TileModule>();
        }
    }
}
