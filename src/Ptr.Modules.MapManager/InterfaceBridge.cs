// ReSharper disable UnusedParameter.Local

#pragma warning disable IDE0290

using Microsoft.Extensions.Configuration;
using Ptr.Modules.MapManager.Shared;
using Ptr.Shared.Misc;
using Sharp.Modules.CommandManager.Shared;
using Sharp.Shared;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;

namespace Ptr.Modules.MapManager;

internal record GameMapManifest(string MapName, int? Countdown, int? MinPlayers, int? MaxPlayers, ulong? WorkshopId);

internal class GameMap : IGameMap
{
    public GameMap(string name, int countdown, int minPlayers, int maxPlayers)
    {
        MapName = name;
        Countdown = countdown;
        MinPlayers = minPlayers;
        MaxPlayers = maxPlayers;
    }

    public string MapName { get; init; }
    public int Countdown { get; init; }
    public int MinPlayers { get; init; }
    public int MaxPlayers { get; init; }

    public bool IsWorkshopMap => WorkshopId is not null;

    public ulong? WorkshopId { get; set; }
}

internal class PreviousGameMap
{
    public PreviousGameMap(string mapName)
    {
        MapName = mapName;
    }

    public string MapName { get; }
    public int RemainingTimes { get; set; }
}

/// <summary>
///     Global context.
/// </summary>
internal class InterfaceBridge
{
    private readonly ISharedSystem _sharedSystem;
    

    public InterfaceBridge(ISharedSystem sharedSystem,
        string dllPath,
        string sharpPath,
        Version version,
        IConfiguration coreConfiguration,
        bool hotReload,
        ChatMessageFormatter formatter, MapManager mapManager)
    {
        _sharedSystem = sharedSystem;
        MapManager = mapManager;
        DllPath = dllPath;
        SharpPath = sharpPath;
        Version = version;
        CoreConfiguration = coreConfiguration;
        IsHotReload = hotReload;
        ChatFormatter = formatter;
        ModuleIdentity = Path.GetFileName(dllPath);
        Instance = this;
    }

    public MapManager MapManager { get; init; }
    public string ModuleIdentity { get; init; }

    /// <summary>
    ///     开洞，一般情况下别用！
    /// </summary>
    internal static InterfaceBridge Instance { get; private set; } = null!;

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
    ///     CGlobalVars* gpGlobals，没什么好说的。<br />
    ///     注意，一定要在地图加载之后调用！不然服务器第一次加载的时候是拿不到的！
    /// </summary>
    public IGlobalVars GlobalVars => ModSharp.GetGlobals();

    /// <summary>
    ///     CGameRules* g_pGameRules <br />
    ///     注意，一定要在地图加载之后调用！不然服务器第一次加载的时候是拿不到的！
    /// </summary>
    public IGameRules GameRules => ModSharp.GetGameRules();

    public INetworkServer Server => ModSharp.GetIServer();
    public IGameData GameData => ModSharp.GetGameData();
    public ISharpModuleManager SharpModuleManager => _sharedSystem.GetSharpModuleManager();

    public ChatMessageFormatter ChatFormatter { get; init; }

    public string DllPath { get; init; }

    public string SharpPath { get; init; }

    public Version? Version { get; init; }

    public IConfiguration? CoreConfiguration { get; init; }

    public bool IsHotReload { get; init; }

    public List<PreviousGameMap> PreviousGameMaps { get; init; } = [];

    public List<IGameMap> Maps { get; init; } = [];

    public List<string> NominatedMaps { get; init; } = [];

    public EMapVoteStyle MapVoteStyle { get; set; } = EMapVoteStyle.Native;

    public DateTime AllowVoteTime { get; set; }

    /// <summary>
    ///     开洞，一般情况下别用！
    /// </summary>
    public string CurrentMapGroup { get; set; } = string.Empty;

    //public ICommandRegistry CommandRegistry { get; set; } = null!;
}