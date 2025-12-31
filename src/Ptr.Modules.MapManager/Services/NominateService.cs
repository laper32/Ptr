using Microsoft.Extensions.Logging;
using Ptr.Shared.Extensions;
using Ptr.Shared.Hosting;
using Sharp.Modules.CommandManager.Shared;
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
        if (_enableNominate?.GetBool() is true)
        {
            return;
        }

        _logger.LogInformation("Nomination is disabled.");
    }

    public void OnAllModulesLoaded()
    {
        if (_enableNominate?.GetBool() is not true)
        {
            return;
        }

        _bridge
            .SharpModuleManager
            .GetRequiredSharpModuleInterface<ICommandManager>(ICommandManager.Identity)
            .Instance!
            .GetRegistry(_bridge.ModuleIdentity)
            .RegisterClientCommand("nominate", OnCommandNominate);
    }

    public void OnShutdown()
    {
        if (_enableNominate?.GetBool() is not true)
        {
            return;
        }

        _bridge.NominatedMaps.Clear();
    }

    private void OnCommandNominate(IGameClient client, StringCommand command)
    {
        var clientsCount = _bridge.Server.GetGameClients(true, true).Count;
        var leastActivateNominateCount = _activateNominateMinPlayers?.GetInt32() ?? 5;

        if (clientsCount < leastActivateNominateCount)
        {
            client.PrintToChat(_bridge.ChatFormatter.Format(
                $"当前在线人数不足 {ChatColor.Green}{leastActivateNominateCount}{ChatColor.White} 人，无法提名地图。"));
            return;
        }

        if (command.ArgCount < 1)
        {
            client.PrintToChat(_bridge.ChatFormatter.Format("用法：.nominate <地图名>"));
            return;
        }

        var map = command[1];
        if (_bridge.NominatedMaps.Contains(map, StringComparer.OrdinalIgnoreCase))
        {
            client.PrintToChat("该地图已经被提名过了。");
            return;
        }

        if (_bridge.PreviousGameMaps.Exists(x => x.MapName.Equals(map, StringComparison.OrdinalIgnoreCase)))
        {
            client.PrintToChat("该地图最近已经玩过了，目前无法提名。");
            return;
        }

        if (_bridge.Maps.Exists(x => x.MapName.Equals(map, StringComparison.OrdinalIgnoreCase)))
        {
            client.PrintToChat("无法从图池中找到该地图。");
            return;
        }

        var currentMap = _bridge.GlobalVars.MapName;
        if (currentMap.Equals(map, StringComparison.OrdinalIgnoreCase))
        {
            client.PrintToChat("不能投票正在游玩的地图。");
            return;
        }

        _bridge.NominatedMaps.Add(map);
        _bridge.ModSharp.PrintToChatAll(_bridge.ChatFormatter.Format(
            $"{ChatColor.Green}{client.Name}{ChatColor.White} 提名了地图 {ChatColor.Green}{map}{ChatColor.White}。"));

        _logger.LogInformation("{ClientName} nominated {Map}", client.Name, map);
    }
}