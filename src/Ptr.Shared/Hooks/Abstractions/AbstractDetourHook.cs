using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Hooks;

namespace Ptr.Shared.Hooks.Abstractions;

public abstract class AbstractDetourHook<T> : AbstractNativeHook<T> where T : AbstractNativeHook<T>
{
    private readonly IDetourHook? _hook;
    private bool _disposed;
    private bool _installed;

    protected AbstractDetourHook(IModSharpModule module, string name, ISharedSystem sharedSystem, ILogger<T> logger) :
        base(module, name)
    {
        if (sharedSystem.GetHookManager().CreateDetourHook() is not { } hook)
        {
            logger.LogError("Failed to create detour hook.");
            return;
        }

        _hook = hook;
    }

    public override void Load()
    {
        if (_hook is null)
        {
            return;
        }

        if (_installed)
        {
            return;
        }

        Prepare(_hook);

        _installed = _hook.Install();

        if (_installed)
        {
            InternalPostInstall(_hook.Trampoline);
        }
    }

    public override void Unload()
    {
        if (_hook is null)
        {
            return;
        }

        ObjectDisposedException.ThrowIf(_disposed, _hook);

        if (_installed)
        {
            _hook.Uninstall();
            _installed = false;
        }

        InternalShutdown();

        _disposed = true;
    }

    protected abstract void Prepare(IDetourHook hook);

    protected abstract void InternalShutdown();

    protected abstract void InternalPostInstall(nint trampoline);
}