using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Ptr.Shared.Hooks.Abstractions;
using Sharp.Shared;
using Sharp.Shared.Hooks;
using Sharp.Shared.Objects;

namespace Ptr.Modules.MapManager.Hooks;

public unsafe class ApplyGameSettingsHook : AbstractVirtualHook<ApplyGameSettingsHook>
{
    private static ApplyGameSettingsHook _sInstance = null!;
    private static nint _sTrampoline = nint.Zero;
    private readonly ISharedSystem _sharedSystem;

    public ApplyGameSettingsHook(IModSharpModule module, string name, ISharedSystem sharedSystem,
        ILogger<ApplyGameSettingsHook> logger) : base(
        module, name, sharedSystem, logger)
    {
        _sharedSystem = sharedSystem;
        _sInstance = this;
    }

    protected override void Prepare(IVirtualHook hook)
    {
        hook.Prepare(Dll, Class, Function, (nint)(delegate* unmanaged<nint, nint, void>)&Hook);
    }

    protected override void InternalShutdown()
    {
        _sTrampoline = nint.Zero;
    }

    protected override void InternalPostInstall(IntPtr trampoline)
    {
        _sTrampoline = trampoline;
    }

    private void ReadConfig(IKeyValues kv)
    {
        var mapGroup = kv.FindKey("launchoptions")?.GetString("mapgroup") ?? string.Empty;
        InterfaceBridge.Instance.CurrentMapGroup = mapGroup;
    }

    [UnmanagedCallersOnly]
    public static void Hook(nint pService, nint pKeyValues)
    {
        var trampoline = (delegate* unmanaged<nint, nint, void>)_sTrampoline;

        if (_sInstance.CreateKeyValues(pKeyValues) is { } kv)
        {
            _sInstance.ReadConfig(kv);
        }

        trampoline(pService, pKeyValues);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IKeyValues? CreateKeyValues(nint ptr)
    {
        return ptr == nint.Zero ? null : _sharedSystem.GetModSharp().CreateNativeObject<IKeyValues>(ptr);
    }
}