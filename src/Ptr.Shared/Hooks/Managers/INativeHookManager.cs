using Microsoft.Extensions.DependencyInjection;
using Ptr.Shared.Extensions;
using Ptr.Shared.Hooks.Abstractions;
using Ptr.Shared.Hooks.Params;
using Sharp.Shared;
using Sharp.Shared.HookParams;
using Sharp.Shared.Managers;

namespace Ptr.Shared.Hooks.Managers;

public interface INativeHookManager
{
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
    internal static INativeHookManager Instance { get; private set; } = null!;

    IHookType<IHandleDropWeaponHookParams, EmptyHookReturn> HandleDropWeapon { get; }

    IHookType<IHandleCommandBuyHookParams, EmptyHookReturn> HandleCommandBuy { get; }

    void RegisterNativeHook(IModSharpModule module, IAbstractNativeHook hook);

    internal void LoadModuleHooks(IModSharpModule module);
    internal void UnloadModuleHooks(IModSharpModule module);
}

public static class ServiceProviderExtensions
{
    extension(IServiceProvider self)
    {
        public void InitNativeHooks()
        {
            var hooks = self.GetAllServices<IAbstractNativeHook>(t => !t.ContainsGenericParameters).ToArray();
            foreach (var hook in hooks)
            {
                hook.Init();
            }
        }

        public void LoadNativeHooks()
        {
            var module = self.GetRequiredService<IModSharpModule>();
            INativeHookManager.Instance.LoadModuleHooks(module);
        }

        public void ShutdownNativeHooks()
        {
            var module = self.GetRequiredService<IModSharpModule>();
            INativeHookManager.Instance.UnloadModuleHooks(module);
        }
    }
}