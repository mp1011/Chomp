using ChompGame.Data;
using ChompGame.Data.Memory;
using Microsoft.Xna.Framework.Input;

namespace ChompGame.GameSystem
{
    class InputModule : Module, ILogicUpdateModule
    {
        public GameInput Player1 { get; private set; }

        public InputModule(MainSystem mainSystem) : base(mainSystem)
        {
        }

        public override void OnStartup()
        {
        }

        

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            Player1 = new GameInput(memoryBuilder,1);   
        }

        public void OnLogicUpdate()
        {
            Player1.Update();
        }
    }
}
