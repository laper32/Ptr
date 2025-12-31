using Microsoft.Extensions.Logging;
using Ptr.Shared.Extensions;
using Ptr.Shared.Hosting;
using Sharp.Modules.CommandManager.Shared;
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
    private readonly IConVar? _enableExtend;

    private readonly bool[] _extClients = new bool[64];
    private readonly IConVar? _extendTime;
    private readonly ILogger<ExtendService> _logger;
    private readonly IConVar? _maxExtCount;
    private int _extCount;
    private ICommandRegistry _commandRegistry = null!;

    public ExtendService(InterfaceBridge bridge, ILogger<ExtendService> logger)
    {
        _bridge = bridge;
        _logger = logger;
        _enableExtend = _bridge.ConVarManager.CreateConVar("mapmanager_enable_extend", true, "Enable ext");
        _maxExtCount = _bridge.ConVarManager.CreateConVar("mapmanager_max_extend_count", 3,
            "Maximum allowed extend map time limit count.");
        _extendTime =
            _bridge.ConVarManager.CreateConVar("mapmanager_ext_time", 15, "The extend applies for time limit.");
    }

    private void OnCommandExt(IGameClient client, StringCommand command)
    {
        var remaining = (int)(_bridge.AllowVoteTime - DateTime.Now).TotalSeconds;
        if (remaining > 0)
        {
            client.PrintToChat(
                _bridge.ChatFormatter.Format($"{ChatColor.Green}{remaining}{ChatColor.White} 秒后才能发起延长地图时间投票。"));
            return;
        }

        var maxAllowed = _maxExtCount!.GetInt32();
        if (_extCount >= maxAllowed)
        {
            client.PrintToChat(_bridge.ChatFormatter.Format("已到达最大可延长时间次数。"));
            return;
        }

        if (_extClients[client.Slot])
        {
            client.PrintToChat("你已经投票过延长地图时间了。");
            return;
        }

        _extClients[client.Slot] = true;
        var current = _extClients.Count(t => t);
        var request = _bridge.MapManager.GetVoteSuccessNumberRequested();
        var clientGaps = request - current;
        client.PrintToChat(
            $"已有 {ChatColor.Green}{current}{ChatColor.White} 人投票延长地图时间，还需 {ChatColor.Green}{clientGaps}{ChatColor.White} 票。");
        if (clientGaps > 0)
        {
            return;
        }

        _bridge.ModSharp.PrintToChatAll("投票通过，即将延长地图持续时间。");
        var timeLimit = _bridge.ConVarManager.FindConVar("mp_timelimit")!;
        var currentTimeLeft = timeLimit.GetFloat();
        var pendingExtendTime = _extendTime?.GetFloat() ?? 15.0f;
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
        if (_enableExtend?.GetBool() is true)
        {
            return;
        }

        _logger.LogInformation("Ext is disabled.");
    }

    public void OnAllModulesLoaded()
    {
        if (_enableExtend?.GetBool() is not true)
        {
            return;
        }

        _commandRegistry = _bridge.SharpModuleManager
            .GetRequiredSharpModuleInterface<ICommandManager>(ICommandManager.Identity).Instance!
            .GetRegistry(_bridge.ModuleIdentity);

        _commandRegistry.RegisterClientCommand("ext", OnCommandExt);
    }

    public void OnShutdown()
    {
        if (_enableExtend?.GetBool() is not true)
        {
            return;
        }

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