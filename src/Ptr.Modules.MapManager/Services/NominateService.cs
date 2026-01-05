using Microsoft.Extensions.Logging;
using Ptr.Shared.Extensions;
using Ptr.Shared.Hosting;
using Sharp.Modules.CommandManager.Shared;
using Sharp.Modules.LocalizerManager.Shared;
using Sharp.Shared.Definition;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;

namespace Ptr.Modules.MapManager.Services;

internal interface INominateService : IModule;

internal class NominateService : INominateService
{
    private readonly IConVar? _activateNominateMinPlayers;
    private readonly InterfaceBridge _bridge;

    private readonly IConVar? _enableNominate;
    private readonly ILogger<NominateService> _logger;
    private ILocalizerManager _localizerManager = null!;

    public NominateService(InterfaceBridge bridge, ILogger<NominateService> logger)
    {
        _bridge = bridge;
        _logger = logger;

        _enableNominate = _bridge.ConVarManager.CreateConVar("mapmanager_enable_nominate", true, "Enable nominate");
        _activateNominateMinPlayers = _bridge.ConVarManager.CreateConVar("mapmanager_activate_nominate_min_players", 5,
            "minimal players count to activate nominate.");
    }

    public void OnInit()
    {
        _logger.LogInformation("Nomination is enabled.");
    }

    public void OnAllModulesLoaded()
    {
        _bridge
            .SharpModuleManager
            .GetRequiredSharpModuleInterface<ICommandManager>(ICommandManager.Identity)
            .Instance!
            .GetRegistry(_bridge.ModuleIdentity)
            .RegisterClientCommand("nominate", OnCommandNominate);

    }

    public void OnShutdown()
    {
        _bridge.NominatedMaps.Clear();
    }

    private void OnCommandNominate(IGameClient client, StringCommand command)
    {
        if (_enableNominate?.GetBool() is not true)
        {
            return;
        }
        if (_localizerManager is null)
        {
            throw new InvalidOperationException("LocalizerManager is not initialized.");
        }
        _localizerManager.TryGetLocalizer(client, out var _localizer);
        if (_localizer is null)
        {
            _logger.LogWarning("Localizer not found for client {ClientSlot} when executing nominate command.",
                client.Slot);
            return;
        }

        var clientsCount = _bridge.Server.GetGameClients(true, true).Count;
        var leastActivateNominateCount = _activateNominateMinPlayers?.GetInt32() ?? 5;

        if (clientsCount < leastActivateNominateCount)
        {
            client.PrintToChat(_bridge.ChatFormatter.Format(
                _localizer.Format("ptr.mapmanager.nominate_not_enough_players", leastActivateNominateCount)));
            return;
        }

        if (command.ArgCount < 1)
        {
            client.PrintToChat(_bridge.ChatFormatter.Format(_localizer.Format("ptr.mapmanager.nominate_usage")));
            return;
        }

        var map = command[1];
        if (_bridge.NominatedMaps.Contains(map, StringComparer.OrdinalIgnoreCase))
        {
            client.PrintToChat(_bridge.ChatFormatter.Format(_localizer.Format("ptr.mapmanager.map_already_nominated")));
            return;
        }

        if (_bridge.PreviousGameMaps.Exists(x => x.MapName.Equals(map, StringComparison.OrdinalIgnoreCase)))
        {
            client.PrintToChat(_bridge.ChatFormatter.Format(_localizer.Format("ptr.mapmanager.map_recently_played")));
            return;
        }

        if (_bridge.Maps.Exists(x => x.MapName.Equals(map, StringComparison.OrdinalIgnoreCase)))
        {
            client.PrintToChat(_bridge.ChatFormatter.Format(_localizer.Format("ptr.mapmanager.map_not_found")));
            return;
        }

        var currentMap = _bridge.GlobalVars.MapName;
        if (currentMap.Equals(map, StringComparison.OrdinalIgnoreCase))
        {
            client.PrintToChat(_bridge.ChatFormatter.Format(_localizer.Format("ptr.mapmanager.cannot_nominate_current_map")));
            return;
        }

        _bridge.NominatedMaps.Add(map);
        var allClients = _bridge.ClientManager.GetGameClients(true);
        foreach (var c in allClients)
        {
            _localizerManager.TryGetLocalizer(c, out var _tempLocalizer);
            if (_tempLocalizer is null)
            {
                _logger.LogWarning("Localizer not found for client {ClientSlot} when broadcasting nomination.",
                    c.Slot);
                continue;
            }
            c.PrintToChat(
                _bridge.ChatFormatter.Format(_tempLocalizer.Format("ptr.mapmanager.player_nominated_map", client.Name, map)));
        }

        _logger.LogInformation("{ClientName} nominated {Map}", client.Name, map);
    }
}