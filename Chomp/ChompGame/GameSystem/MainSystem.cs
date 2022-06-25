using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.Graphics;
using System;
using System.Linq;

namespace ChompGame.GameSystem
{
    public class MainSystem
    {
        private IModule[] _modules;
        private IHBlankHandler[] _hBlankHandlers;
        private IVBlankHandler[] _vBlankHandlers;
        private ILogicUpdateHandler[] _logicUpdateHandlers;

        public Specs Specs { get; }
        public SystemMemory Memory { get; }

        public CoreGraphicsModule CoreGraphicsModule { get; }
      
        public MainSystem(Specs specs, 
            Func<MainSystem,CoreGraphicsModule> createCoreGraphicsModule,
            params Func<MainSystem, IModule>[] createModules)
        {
            Specs = specs;
            CoreGraphicsModule = createCoreGraphicsModule(this);

            _modules = new IModule[createModules.Length];
            for (int i = 0; i < _modules.Length; i++)
                _modules[i] = createModules[i].Invoke(this);

            _hBlankHandlers = _modules.OfType<IHBlankHandler>().ToArray();
            _vBlankHandlers = _modules.OfType<IVBlankHandler>().ToArray();
            _logicUpdateHandlers = _modules.OfType<ILogicUpdateHandler>().ToArray();

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
            foreach (var handler in _logicUpdateHandlers)
                handler.OnLogicUpdate();
        }

        public void OnHBlank()
        {
            foreach (var handler in _hBlankHandlers)
                handler.OnHBlank();           
        }

        public void OnVBlank()
        {
            foreach (var handler in _vBlankHandlers)
                handler.OnVBlank();
        }

       
    }
}
