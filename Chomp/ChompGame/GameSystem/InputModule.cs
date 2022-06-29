using ChompGame.Data;
using Microsoft.Xna.Framework.Input;

namespace ChompGame.GameSystem
{
    class InputModule : Module, ILogicUpdateModule
    {
        public GameInput Player1 { get; private set; }
        public GameInput Player2 { get; private set; }

        public InputModule(MainSystem mainSystem) : base(mainSystem)
        {
        }

        public override void OnStartup()
        {
        }

        

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            Player1 = new GameInput(memoryBuilder,1);
            Player2 = new GameInput(memoryBuilder,2);           
        }

        public void OnLogicUpdate()
        {
            Player1.Update();
            Player2.Update();
        }
    }
}
