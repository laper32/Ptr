using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Hooks;

namespace Ptr.Shared.Hooks.Abstractions;

/// <summary>
///     Virtual hook. <br />
///     Virtual hook always in the specific dll, and always inside the class. <br />
///     It follows the naming rule: {module}@{class}::{function}, eg: server@CBasePlayerWeapon::Deploy
/// </summary>
public abstract class AbstractVirtualHook<T> : AbstractNativeHook<T> where T : AbstractNativeHook<T>
{
    private readonly IVirtualHook? _hook;
    private bool _disposed;
    private bool _installed;

    protected AbstractVirtualHook(IModSharpModule module, string name, ISharedSystem sharedSystem, ILogger<T> logger) :
        base(module, name)
    {
        // Initialize to non-null defaults; values will be set after parsing below.
        Dll = string.Empty;
        Class = string.Empty;
        Function = string.Empty;
        if (sharedSystem.GetHookManager().CreateVirtualHook() is not { } hook)
        {
            logger.LogError("Failed to create virtual hook.");
            return;
        }

        var dll = name.Split("@");
        if (dll.Length != 2)
        {
            logger.LogError(
                $"{name} has invalid dll declaration. The naming rule is: {{module}}@{{class}}::{{function}}");
            return;
        }

        Dll = dll[0];

        var vFn = dll[1];
        var separatedClassAndFunction = vFn.Split("::");
        if (separatedClassAndFunction.Length != 2)
        {
            logger.LogError($"{vFn} naming convention is incorrect. The naming rule is: {{class}}::{{function}}");
            return;
        }

        Class = separatedClassAndFunction[0];
        Function = separatedClassAndFunction[1];

        _hook = hook;
    }

    public string Dll { get; init; }
    public string Class { get; init; }
    public string Function { get; init; }

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

    protected abstract void Prepare(IVirtualHook hook);

    protected abstract void InternalShutdown();

    protected abstract void InternalPostInstall(nint trampoline);
}