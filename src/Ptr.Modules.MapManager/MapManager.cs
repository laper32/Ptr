using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ptr.Modules.MapManager.Hooks;
using Ptr.Modules.MapManager.Services;
using Ptr.Modules.MapManager.Shared;
using Ptr.Shared.Hooks.Hosting;
using Ptr.Shared.Hooks.Managers;
using Ptr.Shared.Hosting;
using Ptr.Shared.Misc;
using Sharp.Modules.LocalizerManager.Shared;
using Sharp.Shared;
using Sharp.Shared.Abstractions;
using Sharp.Shared.Listeners;
using Sharp.Shared.Objects;

namespace Ptr.Modules.MapManager;

internal class MapManager : IModSharpModule, IMapManager, IGameListener
{
    private readonly InterfaceBridge _bridge;
    private readonly ILogger<MapManager> _logger;
    private readonly IServiceProvider _provider;
    private IConVar? _chatFormatPrefix;
    private IConVar? _voteSuccessRatio;
    private IConVar? _countdownAfterChangeLevel;

    public MapManager(
        ISharedSystem sharedSystem,
        string dllPath,
        string sharpPath,
        Version version,
        IConfiguration configuration,
        bool hotReload)
    {
        var formatter = new ChatMessageFormatter();
        var bridge = new InterfaceBridge(sharedSystem, dllPath, sharpPath, version, configuration, hotReload, formatter,
            this);
        var services = new ServiceCollection();
        services.AddSingleton(sharedSystem);
        services.AddSingleton(bridge);
        services.AddSingleton<IRtvService, RtvService>();
        services.AddSingleton<INominateService, NominateService>();
        services.AddSingleton<IExtendService, ExtendService>();
        services.AddSingleton<IMapVoteService, MapVoteService>();
        services.AddHook<ApplyGameSettingsHook>("server@CSource2Server::ApplyGameSettings");
        services.AddSingleton<IModSharpModule>(this);
        services.AddSingleton(sharedSystem.GetLoggerFactory());
        services.AddLogging(x => x.ClearProviders());

        var provider = services.BuildServiceProvider();
        _bridge = provider.GetRequiredService<InterfaceBridge>();
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<MapManager>();

        _provider = provider;
    }

    internal int GetVoteSuccessNumberRequested()
    {
        var ratio = _voteSuccessRatio!.GetFloat();
        var clients = _bridge.Server.GetGameClients(true, true).Count(x => !x.IsFakeClient);
        var result = (int)MathF.Floor(ratio * clients);

        return result <= 0 ? 1 : result;
    }

    private void InitConfig()
    {
        _bridge.Maps.Clear();

        var cfgPath = Path.Combine(_bridge.SharpPath, "configs", "mapmanager", "maplist.jsonc");
        if (!Path.Exists(cfgPath))
        {
            _logger.LogWarning("{CfgPath} is missing, failed to load game maps, map cycle may not work!", cfgPath);
            return;
        }

        var cfgContent = File.ReadAllText(cfgPath);
        var results = JsonSerializer.Deserialize<List<GameMapManifest>>(cfgContent, new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        }) ?? [];

        foreach (var manifest in results)
        {
            var mapName = manifest.MapName;
            var countdown = manifest.Countdown ?? 3;
            var minPlayers = manifest.MinPlayers ?? 0;
            var maxPlayers = manifest.MaxPlayers ?? 0;
            var workshopId = manifest.WorkshopId;
            var obj = new GameMap(mapName, countdown, minPlayers, maxPlayers)
            {
                WorkshopId = workshopId
            };
            _bridge.Maps.Add(obj);
        }

        //CallMapConfigLoaded();
    }

    private void CallMapConfigLoaded()
    {
        if (MapConfigLoaded is null)
        {
            return;
        }

        foreach (var @delegate in MapConfigLoaded.GetInvocationList())
        {
            try
            {
                ((DelegateOnMapConfigLoaded)@delegate).Invoke();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred when calling delegate {Delegate}", nameof(MapConfigLoaded));
            }
        }
    }

    #region IModSharpModule

    public bool Init()
    {
        InitConfig();
        _provider.CallInit<IModule>(e => { _logger.LogError(e, "An error occurred when initializing modules"); });

        return true;
    }

    public void PostInit()
    {
        // Safest place to init convars is PostInit 
        _chatFormatPrefix = _bridge.ConVarManager.CreateConVar("mapmanager_format_prefix", "{whitespace}{green}[RED]{whitespace}{{white}",
            "Chat prefix format for map manager module.");
        
        // Real time prefix changes
        if (_chatFormatPrefix is not null)
        {
            _bridge.ChatFormatter.SetPrefix(_chatFormatPrefix.GetString());
            _bridge.ConVarManager.InstallChangeHook(_chatFormatPrefix, OnChatFormatPrefixChange);
        }

        _countdownAfterChangeLevel = _bridge.ConVarManager.CreateConVar("mapmanager_countdown_after_changelevel", 180,
            "Countdown after map change (unit is sec.)");

        _voteSuccessRatio = _bridge.ConVarManager.CreateConVar("mapmanager_vote_success_ratio", 0.6f,
            "Ratio request for a success vote.");
        _provider.CallPostInit<IModule>(e => { _logger.LogError(e, "An error occurred when initializing modules"); });
        _bridge.SharpModuleManager.RegisterSharpModuleInterface<IMapManager>(this, IMapManager.Identity, this);
    }

    private void OnChatFormatPrefixChange(IConVar conVar)
    {
        _bridge.ChatFormatter.SetPrefix(conVar.GetString());
    }

    public void OnLibraryConnected(string name)
    {
        _provider.CallLibraryConnected<IModule>(name, e =>
        {
            _logger.LogError(e, "An error occurred when calling OnLibraryConnected.");
        });
    }

    public void OnAllModulesLoaded()
    {
        _provider.LoadAllSharpExtensions();
        _provider.InitNativeHooks();
        _provider.CallAllModulesLoaded<IModule>(e => { _logger.LogError(e, "An error occurred when initializing modules"); });
        _provider.UseHook<ApplyGameSettingsHook>();

        CallMapConfigLoaded();

        var _localizerManager = _bridge.SharpModuleManager
            .GetRequiredSharpModuleInterface<ILocalizerManager>(ILocalizerManager.Identity).Instance!;
        _localizerManager.LoadLocaleFile("Ptr.Modules.MapManager");
        
    }

    public void Shutdown()
    {
        _provider.CallShutdown<IModule>(e => { _logger.LogError(e, "An error occurred when shutting down modules"); });
        _provider.ShutdownNativeHooks();
        _provider.ShutdownAllSharpExtensions();
        _bridge.Maps.Clear();
        MapConfigLoaded = null;
        if (_chatFormatPrefix is not null)
        {
            _bridge.ConVarManager.RemoveChangeHook(_chatFormatPrefix, OnChatFormatPrefixChange);
        }
    }

    string IModSharpModule.DisplayName => "Ptr.Modules.MapManager";
    string IModSharpModule.DisplayAuthor => "laper32";

    #endregion

    #region IMapManager

    public event DelegateOnMapConfigLoaded? MapConfigLoaded;

    public IEnumerable<IGameMap> GetMaps()
    {
        return _bridge.Maps;
    }

    public void SetMapVoteStyle(EMapVoteStyle style)
    {
        _bridge.MapVoteStyle = style;
    }

    public bool IsWorkshopMap(string mapName)
    {
        return _bridge.Maps.FirstOrDefault(x => x.MapName.Equals(mapName, StringComparison.OrdinalIgnoreCase)) is
        {
            IsWorkshopMap: true
        };
    }

    public void ChangeLevel(string mapName)
    {
        if (_bridge.Maps.FirstOrDefault(x => x.MapName.Equals(mapName, StringComparison.OrdinalIgnoreCase)) is not
            { } map)
        {
            _logger.LogError("Map {MapName} does not found.", mapName);
            return;
        }

        ChangeLevel(map);
    }

    public void ChangeLevel(IGameMap map)
    {
        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (map.IsWorkshopMap)
        {
            _bridge.ModSharp.ServerCommand($"ds_workshop_changelevel {map.MapName}");
        }
        else
        {
            _bridge.ModSharp.ServerCommand($"changelevel {map.MapName}");
        }
    }

    #endregion

    #region IGameListener

    public void OnServerActivate()
    {
        _bridge.AllowVoteTime = DateTime.Now.AddSeconds(_countdownAfterChangeLevel?.GetInt32() ?? 180);
        // On this step we already executed .cfg files
        if (_chatFormatPrefix is not null) 
        {
            _bridge.ChatFormatter.SetPrefix(_chatFormatPrefix.GetString());
        }
    }

    int IGameListener.ListenerPriority => 0;

    int IGameListener.ListenerVersion => IGameListener.ApiVersion;

    #endregion
}