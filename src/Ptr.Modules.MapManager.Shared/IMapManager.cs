namespace Ptr.Modules.MapManager.Shared;

/// <summary>
///     Map voting style
/// </summary>
public enum EMapVoteStyle
{
    /// <summary>
    ///     Native CS2 Endgame panorama vote
    /// </summary>
    Native,

    /// <summary>
    ///     Menu vote
    /// </summary>
    Menu,

    /// <summary>
    ///     Custom voting style
    /// </summary>
    Custom
}

/// <summary>
///     Game Map
/// </summary>
public interface IGameMap
{
    /// <summary>
    ///     Map name, for example: de_dust2. <br />
    ///     For workshop map, you have to inspect the actual map name by yourself.
    /// </summary>
    string MapName { get; }

    /// <summary>
    ///     Countdown.
    /// </summary>
    int Countdown { get; }

    /// <summary>
    ///     Minimal players that you can vote.
    /// </summary>
    int MinPlayers { get; }

    /// <summary>
    ///     Maximum players that you can vote.
    /// </summary>
    int MaxPlayers { get; }

    /// <summary>
    ///     Is this map workshop map?
    /// </summary>
    bool IsWorkshopMap { get; }

    /// <summary>
    ///     Map workshop id.
    /// </summary>
    ulong? WorkshopId { get; }
}

public delegate void DelegateOnMapConfigLoaded();

public interface IMapManager
{
    const string Identity = nameof(IMapManager);

    event DelegateOnMapConfigLoaded? MapConfigLoaded;

    IEnumerable<IGameMap> GetMaps();

    void SetMapVoteStyle(EMapVoteStyle style);

    bool IsWorkshopMap(string mapName);

    void ChangeLevel(string mapName);

    void ChangeLevel(IGameMap map);
}