using ChompGame.Data;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace ChompGame.GameSystem
{
    public class MainSystem
    {
        private IModule[] _modules;
        private IMasterModule _masterModule;

        public Specs Specs { get; }
        public SystemMemory Memory { get; }

        public GraphicsDevice GraphicsDevice { get;  }

        public CoreGraphicsModule CoreGraphicsModule { get; }
      
        public MainSystem(Specs specs, GraphicsDevice graphicsDevice,
            Func<MainSystem,CoreGraphicsModule> createCoreGraphicsModule,
            params Func<MainSystem, IModule>[] createModules)
        {
            GraphicsDevice = graphicsDevice;
            Specs = specs;
            CoreGraphicsModule = createCoreGraphicsModule(this);

            _modules = new IModule[createModules.Length];
            for (int i = 0; i < _modules.Length; i++)
                _modules[i] = createModules[i].Invoke(this);

            _masterModule = _modules.Last() as IMasterModule;
            if (_masterModule == null)
                throw new Exception("Last module must implement IMasterModule");

            Memory = new SystemMemory(builder =>
            {
                CoreGraphicsModule.BuildMemory(builder);
                foreach (var module in _modules)
                    module.BuildMemory(builder);
            }, specs);

            CoreGraphicsModule.OnStartup();
            foreach (var module in _modules)
                module.OnStartup();

            Memory.Ready();
        }

        public T GetModule<T>() where T:IModule
        {
            return _modules.OfType<T>().Single();
        }

        public void OnLogicUpdate()
        {
            _masterModule.OnLogicUpdate();
        }

        public void OnHBlank()
        {
            _masterModule.OnHBlank();
        }

        public void OnVBlank()
        {
            _masterModule.OnVBlank();
        }

       
    }
}
