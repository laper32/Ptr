using Ptr.Shared.Hooks.Managers;
using Sharp.Shared;

namespace Ptr.Shared.Hooks.Abstractions;

public interface IAbstractNativeHook
{
    public string Name { get; }
    public void Load();
    public void Unload();
}

public abstract class AbstractNativeHook<T> : IAbstractNativeHook
    where T : AbstractNativeHook<T>
{
    protected AbstractNativeHook(IModSharpModule module, string name)
    {
        Name = name;
        INativeHookManager.Instance.RegisterNativeHook(module, this);
    }

    public string Name { get; init; }

    public virtual void Load()
    {
    }

    public virtual void Unload()
    {
    }
}