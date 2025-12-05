using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Option;
using Microsoft.Xna.Framework.Input;

namespace ChompGame.GameSystem
{
    class InputModule : Module, ILogicUpdateModule
    {
        public GameInput Player1 { get; private set; }

        private GameOptions _options;

        public InputModule(MainSystem mainSystem) : base(mainSystem)
        {
            _options = mainSystem.Options;
        }

        public override void OnStartup()
        {
        }

        

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            Player1 = new GameInput(memoryBuilder, _options);   
        }

        public void OnLogicUpdate()
        {
            Player1.Update();
        }
    }
}
