// ReSharper disable UnusedParameter.Local

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ptr.Hooks;
using Ptr.Shared.Bridge;
using Ptr.Shared.Hosting;
using Sharp.Shared;
using Sharp.Shared.Abstractions;

[assembly: DisableRuntimeMarshalling]
[assembly: InternalsVisibleTo("Ptr.Shared")]

namespace Ptr;

internal class ModuleIdentity
{
    public ModuleIdentity(string dllPath)
    {
        Identity = Path.GetFileName(dllPath);
    }

    public string Identity { get; init; }
}

internal class Ptr : IModSharpModule
{
    private readonly InterfaceBridge _bridge;
    private readonly ILogger<Ptr> _logger;
    private readonly IServiceProvider _provider;

    public Ptr(
        ISharedSystem sharedSystem,
        string dllPath,
        string sharpPath,
        Version version,
        IConfiguration configuration,
        bool hotReload)
    {
        var factory = sharedSystem.GetLoggerFactory();
        var bridge = new InterfaceBridge(sharedSystem, dllPath, sharpPath, version, configuration, hotReload);
        var identity = new ModuleIdentity(dllPath);
        var services = new ServiceCollection();
        bridge.GameData.Register("ptr.games");

        // NativeHookManager requires it
        services.AddSingleton<IModSharpModule>(this);
        services.AddSingleton(identity);
        services.AddSingleton(sharedSystem);
        services.AddSingleton(bridge.GameData);
        services.AddSingleton(bridge);
        services.AddSingleton(configuration);
        services.AddSingleton(factory);
        services.AddLogging(p => { p.ClearProviders(); });


        services.AddHooks();

        var provider = services.BuildServiceProvider();
        provider.UseHooks();

        _provider = provider;
        _logger = factory.CreateLogger<Ptr>();
        _bridge = bridge;
    }

    public bool Init()
    {
        _provider.LoadAllSharpExtensions();
        _provider.CallInit<IModule>(e => { _logger.LogError(e, "An error occurred when calling Init."); });

        return true;
    }

    public void PostInit()
    {
        _provider.CallPostInit<IModule>(e => { _logger.LogError(e, "An error occurred when calling PostInit."); });
    }

    public void OnLibraryConnected(string name)
    {
        _provider.CallLibraryConnected<IModule>(name,
            e => { _logger.LogError(e, "An error occurred when calling OnLibraryConnected."); });
    }

    public void OnAllModulesLoaded()
    {
        _provider.CallAllModulesLoaded<IModule>(e =>
        {
            _logger.LogError(e, "An error occurred when calling OnAllModulesLoaded.");
        });
    }

    public void OnLibraryDisconnect(string name)
    {
        _provider.CallLibraryDisconnect<IModule>(name,
            e => { _logger.LogError(e, "An error occurred when calling OnLibraryDisconnect.."); });
    }

    public void Shutdown()
    {
        _bridge.GameData.Unregister("ptr.games");
        _provider.CallShutdown<IModule>(e =>
        {
            _logger.LogError(e, "An error occurred when calling OnLibraryDisconnect..");
        });
        _provider.ShutdownAllSharpExtensions();
    }

    public string DisplayName => "Ptr";
    public string DisplayAuthor => "laper32";
}