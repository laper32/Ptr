using Microsoft.Extensions.Logging;
using Ptr.Modules.MapManager.Shared;
using Ptr.Shared.Extensions;
using Ptr.Shared.Hosting;
using Sharp.Shared.Enums;
using Sharp.Shared.HookParams;
using Sharp.Shared.Listeners;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;

namespace Ptr.Modules.MapManager.Services;

internal interface IMapVoteService : IModule;

internal class MapVoteService : IMapVoteService, IGameListener
{
    private const int MapVoteSize = 10;
    private readonly InterfaceBridge _bridge;
    private readonly int[] _clientVoteOptions = new int[64];
    private readonly ILogger<MapVoteService> _logger;


    public MapVoteService(InterfaceBridge bridge, ILogger<MapVoteService> logger)
    {
        _bridge = bridge;
        _logger = logger;
    }

    public void OnInit()
    {
        _bridge.ModSharp.InstallGameListener(this);
        _bridge.HookManager.MapVoteCreated.InstallForward(OnMapVoteCreated);
        _bridge.ClientManager.InstallCommandListener("endmatch_votenextmap", OnEndMatchVoteNextMap);
    }

    public void OnShutdown()
    {
        _bridge.ModSharp.RemoveGameListener(this);
        _bridge.HookManager.MapVoteCreated.RemoveForward(OnMapVoteCreated);
        _bridge.ClientManager.RemoveCommandListener("endmatch_votenextmap", OnEndMatchVoteNextMap);
    }

    private ECommandAction OnEndMatchVoteNextMap(IGameClient client, StringCommand command)
    {
        var option = command.Get<int>(1);
        UpdateVoteOption(client, option);
        return ECommandAction.Skipped;
    }


    private void OnMapVoteCreated(IMapVoteCreatedForwardParams @params)
    {
        // GetMapGroupMapList的索引是可以直接和EndMatchMapGroupVoteOptions对应的
        if (_bridge.ModSharp.GetMapGroupMapList(_bridge.CurrentMapGroup) is not { } mapGroupElements)
        {
            _logger.LogInformation("Current map group {CurrentMapGroup} cannot retrive map list, check your map group configuration!", _bridge.CurrentMapGroup);
            return;
        }

        var voteMaps = SelectMapsForVote();
        _logger.LogInformation("Selected maps:\n{Maps}",
            string.Join("\n", voteMaps.Select(map => $"  - {map.MapName}")));

        for (var i = 0; i < MapVoteSize; i++)
        {
            @params.GameRules.GetEndMatchMapGroupVoteTypes()[i] = 0; // casual
            @params.GameRules.GetEndMatchMapGroupVoteOptions()[i] = 0; // casual
        }

        var voteOptions = @params.GameRules.GetEndMatchMapGroupVoteOptions();

        // Find indices in MapGroupElements for each selected map
        for (var i = 0; i < Math.Min(voteMaps.Count, MapVoteSize); i++)
        {
            // List<IGameMap>'s index
            var selectedMap = voteMaps[i];

            // Find the index of this map in GetMapGroupMapList()
            var mapIndex = -1;
            for (var j = 0; j < mapGroupElements.Count; j++)
            {
                if (!mapGroupElements[j].Equals(selectedMap.MapName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                mapIndex = j;
                break;
            }

            // Set the vote option to the GetMapGroupMapList() index
            if (mapIndex >= 0)
            {
                voteOptions[i] = mapIndex;
            }
            else
            {
                voteOptions[i] = i; // Fallback to sequential index
            }
        }

        // delay call to get summary.
        var endMatchVoteNextLevelTime = _bridge.ConVarManager.FindConVar("mp_endmatch_votenextleveltime")!.GetFloat();
        _bridge.ModSharp.DelayCall(endMatchVoteNextLevelTime + 2, SummaryVote);
    }

    private List<IGameMap> GetAvailableMaps()
    {
        // Get maps that are not in countdown
        var mapsNotInCountdown = _bridge.Maps.Where(map =>
            {
                var mapInCountdown = _bridge.PreviousGameMaps.Any(prevMap =>
                    prevMap.MapName.Equals(map.MapName, StringComparison.OrdinalIgnoreCase));
                return !mapInCountdown;
            })
            .ToList();


        return mapsNotInCountdown;
    }

    private List<IGameMap> GetNominatedGameMaps()
    {
        // Convert nominated map names to GameMap objects
        return _bridge.NominatedMaps
            .Select(nominatedMapName => _bridge.Maps.FirstOrDefault(map =>
                map.MapName.Equals(nominatedMapName, StringComparison.OrdinalIgnoreCase)))
            .OfType<IGameMap>()
            .ToList();
    }

    private List<IGameMap> SelectMapsForVote(int maxMaps = 10)
    {
        var selectedMaps = new List<IGameMap>();
        var availableMaps = GetAvailableMaps();
        var nominatedMaps = GetNominatedGameMaps();
        var currentMapName = _bridge.GlobalVars.MapName;

        // Filter out the current map from nominated maps
        var validNominatedMaps = nominatedMaps
            .Where(map => !map.MapName.Equals(currentMapName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // First, add all valid nominated maps (they must be included, but not current map)
        selectedMaps.AddRange(validNominatedMaps);

        // Remove nominated maps and current map from available maps to avoid duplicates
        var remainingAvailableMaps = availableMaps
            .Where(map => !validNominatedMaps.Any(nominated =>
                nominated.MapName.Equals(map.MapName, StringComparison.OrdinalIgnoreCase)))
            .Where(map => !map.MapName.Equals(currentMapName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Calculate how many more maps we need
        var remainingSlots = maxMaps - selectedMaps.Count;

        if (remainingSlots > 0 && remainingAvailableMaps.Count > 0)
        {
            // Randomly select from remaining available maps
            var additionalMaps = remainingAvailableMaps
                .Shuffle()
                .Take(remainingSlots)
                .ToList();

            selectedMaps.AddRange(additionalMaps);
        }

        // Shuffle the final list to randomize order
        var shuffledMaps = selectedMaps.Shuffle();

        return shuffledMaps.ToList();
    }


    private void SummaryVote()
    {
        // GetMapGroupMapList的索引是可以直接和EndMatchMapGroupVoteOptions对应的
        if (_bridge.ModSharp.GetMapGroupMapList(_bridge.CurrentMapGroup) is not { } mapGroupElements)
        {
            return;
        }

        var voteOptions = _bridge.GameRules.GetEndMatchMapGroupVoteOptions();
        var mapVotes = new int[10];
        var clients = _bridge.Server.GetGameClients(true, true);
        foreach (var client in clients)
        {
            var voteSelectionIndex = _clientVoteOptions[client.Slot];
            if (voteSelectionIndex < 0)
            {
                continue;
            }

            mapVotes[voteSelectionIndex]++;
        }

        var hasAnyVotes = false;
        var maxVotes = 0;
        var winningIndex = -1;

        for (var i = 0; i < MapVoteSize; i++)
        {
            var opt = voteOptions[i];
            if (opt < 0)
            {
                continue;
            }

            var votes = mapVotes[i];

            if (votes > 0)
            {
                hasAnyVotes = true;
            }

            Console.WriteLine($" - {mapGroupElements[opt]} got {votes} votes.");
            if (votes <= maxVotes)
            {
                continue;
            }

            maxVotes = votes;
            winningIndex = i;
        }

        if (!hasAnyVotes)
        {
            _bridge.ModSharp.DelayCall(2.0, ForceChangeMap);
            return;
        }

        var winningMapIndex = voteOptions[winningIndex];
        var winningMapName = mapGroupElements[winningMapIndex];
        var winningMap = _bridge.Maps.First(map =>
            map.MapName.Equals(winningMapName, StringComparison.OrdinalIgnoreCase));

        _logger.LogInformation("Map {MapName} won with {Votes} votes!", winningMapName, maxVotes);

        _bridge.ModSharp.DelayCall(2.0, () => { _bridge.MapManager.ChangeLevel(winningMap); });
    }

    private void ForceChangeMap()
    {
        var maps = SelectMapsForVote();
        var randomOne = maps.Shuffle().First();
        Console.WriteLine($"No any selection. Randomly choose a map, that is: {randomOne.MapName}");
        _bridge.MapManager.ChangeLevel(randomOne);
    }

    private void UpdateVoteOption(IGameClient client, int voteOption)
    {
        Console.WriteLine($"player {client.Name} vote opt: {voteOption}");
        _clientVoteOptions[client.Slot] = voteOption;
    }

    private void ResetAllClientVoteOptions()
    {
        Array.Fill(_clientVoteOptions, -1);
    }

    #region IGameListener

    public void OnGameDeactivate()
    {
        ResetAllClientVoteOptions();
    }

    int IGameListener.ListenerVersion => IGameListener.ApiVersion;
    int IGameListener.ListenerPriority => 0;

    #endregion
}