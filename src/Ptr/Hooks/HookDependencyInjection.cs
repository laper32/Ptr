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
        self.AddSingleton<IInternalNativeHookManager, NativeHookManager>();

        self.AddHook<HandleDropWeaponHookService>("CCSPlayer_WeaponServices::HandleDropWeapon");
        self.AddHook<HandleCommandBuyHookService>("CCSPlayer_BuyServices::HandleCommand_Buy_Internal");
    }

    public static void UseHooks(this IServiceProvider self)
    {
        var hookManager = self.GetRequiredService<IInternalNativeHookManager>();
        typeof(INativeHookManager).SetStaticReadonlyProperty("Instance", hookManager);
        self.UseHook<HandleDropWeaponHookService>();
        self.UseHook<HandleCommandBuyHookService>();
    }
}