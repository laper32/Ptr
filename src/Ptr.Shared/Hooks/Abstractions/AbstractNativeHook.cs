using Ptr.Shared.Hooks.Managers;
using Sharp.Shared;

namespace Ptr.Shared.Hooks.Abstractions;

public interface IAbstractNativeHook
{
    public string Name { get; }

    public void Init();
    public void Load();
    public void Unload();
}

public abstract class AbstractNativeHook<T> : IAbstractNativeHook
    where T : AbstractNativeHook<T>
{
    private readonly IModSharpModule _module;

    protected AbstractNativeHook(IModSharpModule module, string name)
    {
        _module = module;
        Name = name;
    }

    public void Init()
    {
        INativeHookManager.Instance.RegisterNativeHook(_module, this);
    }

    public string Name { get; init; }

    public abstract void Load();

    public abstract void Unload();
}