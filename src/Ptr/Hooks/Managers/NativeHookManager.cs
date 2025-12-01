#pragma warning disable CA1822
#pragma warning disable IDE0290

using Microsoft.Extensions.Logging;
using Ptr.Hooks.Services;
using Ptr.Shared.Hooks.Abstractions;
using Ptr.Shared.Hooks.Managers;
using Ptr.Shared.Hooks.Params;
using Ptr.Shared.Hosting;
using Sharp.Shared;
using Sharp.Shared.HookParams;
using Sharp.Shared.Managers;

namespace Ptr.Hooks.Managers;

internal interface IInternalNativeHookManager : INativeHookManager, IModule;

internal class NativeHookManager : IInternalNativeHookManager
{
    private readonly List<NativeHookInfo> _hooks = [];
    private readonly ILogger<NativeHookManager> _logger;
    private readonly IModSharpModule _self;

    public NativeHookManager(IModSharpModule self, ILogger<NativeHookManager> logger)
    {
        _self = self;
        _logger = logger;
    }


    private record NativeHookInfo(IModSharpModule Module, IAbstractNativeHook Hook);

    #region IModule

    public void OnInit()
    {
        foreach (var hook in _hooks.Where(x => x.Module.Equals(_self)))
        {
            hook.Hook.Load();
        }
    }

    public void OnShutdown()
    {
        foreach (var hook in _hooks.Where(x => x.Module.Equals(_self)))
        {
            hook.Hook.Unload();
        }
    }

    #endregion

    #region INativeHookManager

    public IHookType<IHandleCommandBuyHookParams, EmptyHookReturn> HandleCommandBuy =>
        HandleCommandBuyHookService.Instance;

    public IHookType<IHandleDropWeaponHookParams, EmptyHookReturn> HandleDropWeapon =>
        HandleDropWeaponHookService.Instance;

    public void RegisterNativeHook(IModSharpModule module, IAbstractNativeHook hook)
    {
        if (_hooks.FirstOrDefault(x => x.Hook.Equals(hook)) is not null)
        {
            _logger.LogError("Hook type {Type} has been added multiple times.", hook.GetType());
            return;
        }

        _hooks.Add(new NativeHookInfo(module, hook));
    }

    public void LoadModuleHooks(IModSharpModule module)
    {
        foreach (var abstractNativeHook in _hooks.Where(x => x.Module.Equals(module)).Select(x => x.Hook))
        {
            abstractNativeHook.Load();
        }
    }

    public void UnloadModuleHooks(IModSharpModule module)
    {
        foreach (var abstractNativeHook in _hooks.Where(x => x.Module.Equals(module)).Select(x => x.Hook))
        {
            abstractNativeHook.Unload();
        }
    }

    #endregion
}