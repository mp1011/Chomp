using ChompGame.Data;

namespace ChompGame.GameSystem
{

    public interface IModule
    {
        void BuildMemory(SystemMemoryBuilder memoryBuilder);

        void OnStartup();
    }

    public abstract class Module : IModule
    {
        public MainSystem GameSystem { get; }
        public Specs Specs => GameSystem.Specs;

        protected Module(MainSystem mainSystem)
        {
            GameSystem = mainSystem;
        }

        public abstract void BuildMemory(SystemMemoryBuilder memoryBuilder);
        public abstract void OnStartup();
    }

    public interface IHBlankHandler : IModule
    {
        void OnHBlank();
    }

    public interface IVBlankHandler : IModule
    {
        void OnVBlank();
    }

    public interface ILogicUpdateHandler : IModule
    {
        void OnLogicUpdate();
    }
}
