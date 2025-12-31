using Microsoft.Extensions.Logging;
using Ptr.Shared.Extensions;
using Ptr.Shared.Hosting;
using Sharp.Shared.Definition;
using Sharp.Shared.Enums;
using Sharp.Shared.Listeners;
using Sharp.Shared.Objects;

namespace Ptr.Modules.MapManager.Services;

internal interface IRtvService : IModule;

internal class RtvService : IRtvService, IGameListener, IClientListener
{
    private readonly InterfaceBridge _bridge;

    private readonly IConVar? _enableRtv;
    private readonly ILogger<RtvService> _logger;
    private readonly bool[] _rtvPlayers = new bool[64];

    public RtvService(InterfaceBridge bridge, ILogger<RtvService> logger)
    {
        _bridge = bridge;
        _logger = logger;


        _enableRtv = _bridge.ConVarManager.CreateConVar("mapmanager_enable_rtv", true, "Enable RTV");
    }

    private void AttemptRtv(IGameClient client)
    {
        var remaining = (int)(_bridge.AllowVoteTime - DateTime.Now).TotalSeconds;
        if (remaining > 0)
        {
            client.PrintToChat(
                _bridge.ChatFormatter.Format($"{ChatColor.Green} {remaining} {ChatColor.White}秒后才能发起换图投票。"));
            return;
        }

        if (_rtvPlayers[client.Slot])
        {
            client.PrintToChat(_bridge.ChatFormatter.Format("你已经投票过换图了！"));
            return;
        }

        _rtvPlayers[client.Slot] = true;
        var current = _rtvPlayers.Count(rtvPlayer => rtvPlayer);
        var requested = _bridge.MapManager.GetVoteSuccessNumberRequested();
        var clientGaps = requested - current;
        _bridge.ModSharp.PrintToChatAll(_bridge.ChatFormatter.Format(
            $"已有 {ChatColor.Green}{current}{ChatColor.White} 人投票换图，还需 {ChatColor.Green}{clientGaps}{ChatColor.White} 票。"));

        if (clientGaps > 0)
        {
            return;
        }

        _bridge.ModSharp.PrintToChatAll(_bridge.ChatFormatter.Format("投票换图通过！将在回合结束后开始投票。"));
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
        if (_enableRtv?.GetBool() is not true)
        {
            _logger.LogInformation("RTV is disabled, skip initialization.");
            return;
        }

        _bridge.ClientManager.InstallClientListener(this);
        _bridge.ModSharp.InstallGameListener(this);
    }

    public void OnShutdown()
    {
        if (_enableRtv?.GetBool() is not true)
        {
            _logger.LogInformation("RTV is disabled, skip shutdown.");
            return;
        }

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