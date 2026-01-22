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
    protected AbstractNativeHook(IModSharpModule module, string name)
    {
        Name = name;
        Module = module;
    }

    protected IModSharpModule Module { get; }
    public string Name { get; init; }

    public virtual void Init()
    {
        if (INativeHookManager.Instance is null)
        {
            throw new InvalidOperationException(
                $"INativeHookManager.Instance is not initialized. Cannot initialize hook '{Name}' from module '{Module.DisplayName}'");
        }
        
        INativeHookManager.Instance.RegisterNativeHook(Module, this);
    }

    public virtual void Load()
    {
    }

    public virtual void Unload()
    {
    }
}