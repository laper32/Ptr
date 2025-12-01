using Microsoft.Extensions.DependencyInjection;
using Ptr.Hooks.Managers;
using Ptr.Hooks.Services;
using Ptr.Shared.Extensions;
using Ptr.Shared.Hooks.Hosting;
using Ptr.Shared.Hooks.Managers;

namespace Ptr.Hooks;

public static class HookDependencyInjection
{
    public static void AddHooks(this IServiceCollection self)
    {
        self.AddSingleton<NativeHookManager>();
        self.AddSingleton<IInternalNativeHookManager>(s => s.GetRequiredService<NativeHookManager>());
        self.AddSingleton<INativeHookManager>(s => s.GetRequiredService<NativeHookManager>());
        self.AddHook<HandleDropWeaponHookService>("CCSPlayer_WeaponServices::HandleDropWeapon");
    }

    public static void UseHooks(this IServiceProvider self)
    {
        var hookManager = self.GetRequiredService<NativeHookManager>();
        typeof(INativeHookManager).SetStaticReadonlyProperty("Instance", hookManager);
        self.UseHook<HandleDropWeaponHookService>();
    }
}