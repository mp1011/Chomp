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

        public Specs Specs { get; }
        public SystemMemory Memory { get; }

        public CoreGraphicsModule CoreGraphicsModule { get; }
      
        public MainSystem(Specs specs, 
            Func<MainSystem,CoreGraphicsModule> createCoreGraphicsModule,
            params Func<MainSystem, IModule>[] createModules)
        {
            Specs = specs;
            CoreGraphicsModule = createCoreGraphicsModule(this);
            _modules = createModules.Select(p => p(this)).ToArray();
            _hBlankHandlers = _modules.OfType<IHBlankHandler>().ToArray();
            _vBlankHandlers = _modules.OfType<IVBlankHandler>().ToArray();

            Memory = new SystemMemory(builder =>
            {
                CoreGraphicsModule.BuildMemory(builder);
                foreach (var module in _modules)
                    module.BuildMemory(builder);
            });

            CoreGraphicsModule.OnStartup();
            foreach (var module in _modules)
                module.OnStartup();
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
