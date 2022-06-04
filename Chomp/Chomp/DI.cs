using Chomp.Models;
using Chomp.Services;
using Chomp.Services.Interfaces;
using Chomp.SystemModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Chomp
{
    static class DI
    {
        public static IServiceProvider Init(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IRenderService, RenderService>();
                    services.AddSingleton( _ => SystemSpecs.Default);
                    services.AddSingleton<MemoryValueFactory>();
                    services.AddSingleton<Memory>();
                    services.AddSingleton<GameSystem>();
                })
                .Build();
            return host.Services;
        }
    }
}
