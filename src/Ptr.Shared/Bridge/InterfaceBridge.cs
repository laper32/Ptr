using Microsoft.Extensions.Configuration;
using Sharp.Shared;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;

namespace Ptr.Shared.Bridge;

internal class InterfaceBridge
{
    private readonly ISharedSystem _sharedSystem;

    public InterfaceBridge(ISharedSystem sharedSystem,
        string dllPath,
        string sharpPath,
        Version? version,
        IConfiguration? configuration,
        bool hotReload)
    {
        _sharedSystem = sharedSystem;
        DllPath = dllPath;
        SharpPath = sharpPath;
        Version = version;
        Configuration = configuration;
        IsHotReload = hotReload;
        Instance = this;
    }

    /// <summary>
    ///     Mostly please use constructor passed ref. <br />
    ///     This is only used for some corner case.
    /// </summary>
    public static InterfaceBridge Instance { get; private set; } = null!;

    public ISteamApi SteamApi => ModSharp.GetSteamGameServer();

    public IEntityManager EntityManager => _sharedSystem.GetEntityManager();
    public IClientManager ClientManager => _sharedSystem.GetClientManager();
    public IConVarManager ConVarManager => _sharedSystem.GetConVarManager();
    public ITransmitManager TransmitManager => _sharedSystem.GetTransmitManager();
    public IHookManager HookManager => _sharedSystem.GetHookManager();
    public IEventManager EventManager => _sharedSystem.GetEventManager();
    public IFileManager FileManager => _sharedSystem.GetFileManager();
    public ISchemaManager SchemaManager => _sharedSystem.GetSchemaManager();
    public IEconItemManager EconItemManager => _sharedSystem.GetEconItemManager();
    public ILibraryModuleManager LibraryModuleManager => _sharedSystem.GetLibraryModuleManager();
    public ISoundManager SoundManager => _sharedSystem.GetSoundManager();
    public IPhysicsQueryManager PhysicsQueryManager => _sharedSystem.GetPhysicsQueryManager();

    public IModSharp ModSharp => _sharedSystem.GetModSharp();

    /// <summary>
    ///     CGlobalVars* gpGlobals.<br />
    ///     Note: Must be called after the map is loaded! Otherwise it won't be available on the server's first load!
    /// </summary>
    public IGlobalVars GlobalVars => ModSharp.GetGlobals();

    /// <summary>
    ///     CGameRules* g_pGameRules <br />
    ///     Note: Must be called after the map is loaded! Otherwise it won't be available on the server's first load!
    /// </summary>
    public IGameRules GameRules => ModSharp.GetGameRules();

    public INetworkServer Server => ModSharp.GetIServer();
    public IGameData GameData => ModSharp.GetGameData();
    public ISharpModuleManager SharpModuleManager => _sharedSystem.GetSharpModuleManager();

    public string DllPath { get; init; }

    public string SharpPath { get; init; }

    public Version? Version { get; init; }

    public IConfiguration? Configuration { get; init; }

    public bool IsHotReload { get; init; }

    public Random RandomEngine { get; init; } = new();
}