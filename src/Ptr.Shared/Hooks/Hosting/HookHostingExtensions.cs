using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sharp.Shared;

namespace Ptr.Shared.Hooks.Hosting;

public static class HookHostingExtensions
{
    public static void AddHook<THook>(this IServiceCollection self, string name) where THook : class
    {
        self.AddSingleton(p =>
        {
            var module = p.GetRequiredService<IModSharpModule>();
            var sharedSystem = p.GetRequiredService<ISharedSystem>();
            var logger = p.GetRequiredService<ILogger<THook>>();

            return (THook)Activator.CreateInstance(typeof(THook), module, name, sharedSystem, logger)!;
        });
    }

    public static void UseHook<THook>(this IServiceProvider self) where THook : class
    {
        self.GetRequiredService<THook>();
    }
}