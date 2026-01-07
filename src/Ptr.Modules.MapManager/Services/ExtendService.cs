using Microsoft.Extensions.Logging;
using Ptr.Shared.Extensions;
using Ptr.Shared.Hosting;
using Sharp.Modules.CommandManager.Shared;
using Sharp.Modules.LocalizerManager.Shared;
using Sharp.Shared.Definition;
using Sharp.Shared.Enums;
using Sharp.Shared.Listeners;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;

namespace Ptr.Modules.MapManager.Services;

internal interface IExtendService : IModule;

internal class ExtendService : IExtendService, IClientListener, IGameListener
{
    private readonly InterfaceBridge _bridge;
    private readonly IConVar _enableExtend;
    private readonly IConVar _extendTime;
    private readonly IConVar _maxExtCount;
    private readonly bool[] _extClients = new bool[64];
    private readonly ILogger<ExtendService> _logger;
    private int _extCount;
    private ICommandRegistry _commandRegistry = null!;
    private ILocalizerManager _localizerManager = null!;

    public ExtendService(InterfaceBridge bridge, ILogger<ExtendService> logger)
    {
        _bridge = bridge;
        _logger = logger;
        _enableExtend = _bridge.ConVarManager.CreateConVar("mapmanager_enable_extend", true, "Enable map extensions command", ConVarFlags.Release)!;
        _maxExtCount = _bridge.ConVarManager.CreateConVar("mapmanager_max_extend_count", 3,
            "Maximum allowed extend map time limit count.", ConVarFlags.Release)!;
        _extendTime = _bridge.ConVarManager.CreateConVar("mapmanager_ext_time", 15, "The extend applies for time limit.", ConVarFlags.Release)!;
    }

    private void OnCommandExt(IGameClient client, StringCommand command)
    {
        if (_enableExtend.GetBool() is not true)
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
            _logger.LogWarning("Localizer not found for client {ClientSlot} when executing ext command.",
                client.Slot);
            return;
        }
        var remaining = (int)(_bridge.AllowVoteTime - DateTime.Now).TotalSeconds;
        if (remaining > 0)
        {
            client.PrintToChat(
                _bridge.ChatFormatter.Format(_localizer.Format("ptr.mapmanager.extend_map_after_x", remaining)));
            return;
        }

        var maxAllowed = _maxExtCount.GetInt32();
        if (_extCount >= maxAllowed)
        {
            client.PrintToChat(_bridge.ChatFormatter.Format(_localizer.Format("ptr.mapmanager.max_extends_reached")));
            return;
        }

        if (_extClients[client.Slot])
        {
            client.PrintToChat(_bridge.ChatFormatter.Format(_localizer.Format("ptr.mapmanager.you_already_voted_to_extend")));
            return;
        }

        _extClients[client.Slot] = true;
        var current = _extClients.Count(t => t);
        var request = _bridge.MapManager.GetVoteSuccessNumberRequested();
        var clientGaps = request - current;
        client.PrintToChat(
            _bridge.ChatFormatter.Format(_localizer.Format("ptr.mapmanager.extend_vote_progress", current, clientGaps)));
        if (clientGaps > 0)
        {
            return;
        }
        var allClients = _bridge.ClientManager.GetGameClients(true);
        foreach (var c in allClients)
        {
            if (c.IsFakeClient)
            {
                continue;
            }
            _localizerManager.TryGetLocalizer(client, out var _tempLocalizer);
            if (_tempLocalizer is null)
            {
                _logger.LogWarning("Localizer not found for client {ClientSlot} when executing ext command.",
                    c.Slot);
                continue;
            }
            c.PrintToChat(
                _bridge.ChatFormatter.Format(_tempLocalizer.Format("ptr.mapmanager.extend_vote_passed")));
        }
        var timeLimit = _bridge.ConVarManager.FindConVar("mp_timelimit")!;
        var currentTimeLeft = timeLimit.GetFloat();
        var pendingExtendTime = _extendTime.GetFloat();
        var nextTimeLeft = currentTimeLeft + pendingExtendTime;
        timeLimit.Set($"{nextTimeLeft}");
        _extCount++;
    }


    private void ResetClientExtState(IGameClient client)
    {
        _extClients[client.Slot] = false;
    }

    private void ResetClientsExt()
    {
        Array.Fill(_extClients, false);
    }

    private void ResetExtCount()
    {
        _extCount = 0;
    }

    #region IModule
    public void OnInit()
    {
        _bridge.ModSharp.InstallGameListener(this);
        _bridge.ClientManager.InstallClientListener(this);

        _logger.LogInformation("Ext is disabled.");
    }

    public void OnAllModulesLoaded()
    {
        _commandRegistry = _bridge.SharpModuleManager
            .GetRequiredSharpModuleInterface<ICommandManager>(ICommandManager.Identity).Instance!
            .GetRegistry(_bridge.ModuleIdentity);

        _commandRegistry.RegisterClientCommand("ext", OnCommandExt);

        _localizerManager = _bridge.SharpModuleManager
            .GetRequiredSharpModuleInterface<ILocalizerManager>(ILocalizerManager.Identity)
            .Instance!;
    }

    public void OnShutdown()
    {
        ResetExtCount();
        ResetClientsExt();
    }

    #endregion

    #region IClientListener

    public void OnClientPutInServer(IGameClient client)
    {
        ResetClientExtState(client);
    }

    public void OnClientDisconnected(IGameClient client, NetworkDisconnectionReason reason)
    {
        ResetClientExtState(client);
    }

    int IClientListener.ListenerPriority => 0;
    int IClientListener.ListenerVersion => IClientListener.ApiVersion;

    #endregion

    #region IGameListener

    public void OnGameDeactivate()
    {
        ResetClientsExt();
    }

    int IGameListener.ListenerPriority => 0;

    int IGameListener.ListenerVersion => IGameListener.ApiVersion;

    #endregion
}
