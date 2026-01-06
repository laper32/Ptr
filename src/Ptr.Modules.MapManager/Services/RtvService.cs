using Microsoft.Extensions.Logging;
using Ptr.Shared.Extensions;
using Ptr.Shared.Hosting;
using Sharp.Modules.LocalizerManager.Shared;
using Sharp.Shared.Definition;
using Sharp.Shared.Enums;
using Sharp.Shared.Listeners;
using Sharp.Shared.Objects;

namespace Ptr.Modules.MapManager.Services;

internal interface IRtvService : IModule;

internal class RtvService : IRtvService, IGameListener, IClientListener
{
    private readonly InterfaceBridge _bridge;
    private readonly IConVar _enableRtv;
    private readonly ILogger<RtvService> _logger;
    private readonly bool[] _rtvPlayers = new bool[64];
    private ILocalizerManager _localizerManager = null!;

    public RtvService(InterfaceBridge bridge, ILogger<RtvService> logger)
    {
        _bridge = bridge;
        _logger = logger;
        _enableRtv = _bridge.ConVarManager.CreateConVar("mapmanager_enable_rtv", true, "Enable RTV", ConVarFlags.Release)!;
    }

    private void AttemptRtv(IGameClient client)
    {
        if (_enableRtv.GetBool() is not true)
        {
            _logger.LogInformation("RTV is disabled, skip RTV attempt.");
            return;
        }
        if (_localizerManager is null)
        {
            throw new InvalidOperationException("LocalizerManager is not initialized.");
        }
        _localizerManager.TryGetLocalizer(client, out var _localizer);
        if (_localizer is null)
        {
            _logger.LogWarning("Localizer not found for client {ClientSlot} when executing rtv command.",
                client.Slot);
            return;
        }

        var remaining = (int)(_bridge.AllowVoteTime - DateTime.Now).TotalSeconds;
        if (remaining > 0)
        {
            client.PrintToChat(
                _bridge.ChatFormatter.Format(_localizer.Format("ptr.mapmanager.rtv_after_x", remaining)));
            return;
        }

        if (_rtvPlayers[client.Slot])
        {
            client.PrintToChat(_bridge.ChatFormatter.Format(_localizer.Format("ptr.mapmanager.already_voted_rtv")));
            return;
        }

        _rtvPlayers[client.Slot] = true;
        var current = _rtvPlayers.Count(rtvPlayer => rtvPlayer);
        var requested = _bridge.MapManager.GetVoteSuccessNumberRequested();
        var clientGaps = requested - current;

        var allClients = _bridge.ClientManager.GetGameClients(true);
        foreach (var c in allClients)
        {
            _localizerManager.TryGetLocalizer(c, out var _tempLocalizer);
            if (_tempLocalizer is null)
            {
                _logger.LogWarning("Localizer not found for client {ClientSlot} when broadcasting rtv progress.",
                    c.Slot);
                continue;
            }
            c.PrintToChat(
                _bridge.ChatFormatter.Format(_tempLocalizer.Format("ptr.mapmanager.rtv_vote_progress", current, clientGaps)));
        }

        if (clientGaps > 0)
        {
            return;
        }

        foreach (var c in allClients)
        {
            _localizerManager.TryGetLocalizer(c, out var _tempLocalizer);
            if (_tempLocalizer is null)
            {
                _logger.LogWarning("Localizer not found for client {ClientSlot} when broadcasting rtv passed.",
                    c.Slot);
                continue;
            }
            c.PrintToChat(
                _bridge.ChatFormatter.Format(_tempLocalizer.Format("ptr.mapmanager.rtv_vote_passed")));
        }
        _bridge.ModSharp.ServerCommand("mp_timelimit 0.00000001");
    }

    private void ResetRtvState()
    {
        Array.Fill(_rtvPlayers, false);
    }

    private void ResetClientRtvState(IGameClient client)
    {
        _rtvPlayers[client.Slot] = false;
    }

    #region IModule
    public void OnInit()
    {
        _bridge.ModSharp.InstallGameListener(this);
        _bridge.ClientManager.InstallClientListener(this);
    }
    public void OnAllModulesLoaded()
    {
        _localizerManager = _bridge.SharpModuleManager
            .GetRequiredSharpModuleInterface<ILocalizerManager>(ILocalizerManager.Identity)
            .Instance!;
    }

    public void OnShutdown()
    {
        _bridge.ModSharp.RemoveGameListener(this);
        _bridge.ClientManager.RemoveClientListener(this);
        ResetRtvState();
    }

    #endregion

    #region IGameListener

    public void OnGameDeactivate()
    {
        ResetRtvState();
    }

    int IGameListener.ListenerVersion => IGameListener.ApiVersion;
    int IGameListener.ListenerPriority => 0;

    #endregion

    #region IClientListener

    public void OnClientPutInServer(IGameClient client)
    {
        ResetClientRtvState(client);
    }

    public void OnClientDisconnected(IGameClient client, NetworkDisconnectionReason reason)
    {
        ResetClientRtvState(client);
    }

    public ECommandAction OnClientSayCommand(IGameClient client, bool teamOnly, bool isCommand, string commandName,
        string message)
    {
        if (message.Split().ElementAtOrDefault(0)?.Equals("rtv", StringComparison.OrdinalIgnoreCase) is true)
        {
            AttemptRtv(client);
        }

        return ECommandAction.Skipped;
    }

    int IClientListener.ListenerPriority => 0;

    int IClientListener.ListenerVersion => IClientListener.ApiVersion;

    #endregion
}