namespace Ptr.Shared.Proxies;

internal interface IBaseEntityProxy
{
    void DelayCall(float delay, Action action);
}

internal static class BaseEntityProxyProvider
{
    private static IBaseEntityProxy? _instance;

    public static IBaseEntityProxy Instance =>
        _instance ?? throw new InvalidOperationException("BaseEntity proxy has not been initialized.");

    public static void Initialize(IBaseEntityProxy proxy)
    {
        _instance = proxy ?? throw new ArgumentNullException(nameof(proxy));
    }
}