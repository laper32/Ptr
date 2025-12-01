namespace Ptr.Shared.Hosting;

/// <summary>
///     For module inner usage.
/// </summary>
public interface IBaseInterface
{
    void OnInit()
    {
    }

    void OnPostInit()
    {
    }

    void OnAllModulesLoaded()
    {
    }

    void OnLibraryConnected(string libName)
    {
    }

    void OnLibraryDisconnect(string libName)
    {
    }

    void OnShutdown()
    {
    }
}