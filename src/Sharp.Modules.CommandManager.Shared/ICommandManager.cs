using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using DelegateClientCommand = Sharp.Shared.Managers.IClientManager.DelegateClientCommand;

namespace Sharp.Modules.CommandManager.Shared;

public interface ICommandManager
{
    const string Identity = nameof(ICommandManager);

    /// <summary>
    /// Add Command registry. <br/>
    /// </summary>
    /// <param name="moduleIdentity"></param>
    public ICommandRegistry GetRegistry(string moduleIdentity);

}

public interface ICommandRegistry
{
    /// <summary>
    ///     Register a client command.<br />
    ///     This command can be invoked by:<br />
    ///     1. Typing .{command} in chat, e.g., .ztele.<br />
    ///     (Note: You can use ! or / instead of ., they are equivalent.)<br />
    ///     2. Client console, you must add the `ms_` prefix, e.g., ms_ztele.<br />
    ///     Commands registered with this function cannot be called from the server console.
    /// </summary>
    /// <param name="command">The command, you don't need to add the ms_ prefix, it will be added automatically during registration. You can also add it yourself, this doesn't matter. But if you use ms_ms_, we can't help you.</param>
    /// <param name="call"></param>
    void RegisterClientCommand(string command, Action<IGameClient, StringCommand> call);

    /// <summary>
    ///     Create a server console command.<br />
    ///     This command only works in the server console.
    /// </summary>
    /// <param name="description"></param>
    /// <param name="addPrefix">Whether to add ms_ prefix? Default is true.</param>
    /// <param name="command"></param>
    /// <param name="call"></param>
    void RegisterServerCommand(string command, Action<StringCommand> call, string description = "", bool addPrefix = true);

    /// <summary>
    ///     Create a server console command.<br />
    ///     This command only works in the server console.
    /// </summary>
    /// <param name="description"></param>
    /// <param name="addPrefix">Whether to add ms_ prefix? Default is true.</param>
    /// <param name="command"></param>
    /// <param name="call"></param>
    void RegisterServerCommand(string command, Action call, string description = "", bool addPrefix = true);

    /// <summary>
    /// Register a "generic" command: can be used from client chat, client console, and server console.<br />
    /// Will always add the ms_ prefix, please note.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="call"></param>
    /// <param name="description"></param>
    void RegisterGenericCommand(string command, Action<IGameClient?, StringCommand> call, string description = "");

    /// <summary>
    ///     Create a console command. <br />
    ///     This command only works in client console and server console.<br />
    /// </summary>
    /// <param name="command"></param>
    /// <param name="callback"></param>
    /// <param name="addPrefix">Whether to add ms_ prefix? Default is true.</param>
    void RegisterConsoleCommand(string command, Action<IGameClient?, StringCommand> callback, bool addPrefix = true);

    /// <summary>
    ///     Listen for a command.<br />
    ///     Generally, this function is only used to listen for commands entered in the client console, such as player_ping.<br />
    ///     Refer to function calls for details.
    /// </summary>
    /// <param name="commandName"></param>
    /// <param name="callback"></param>
    void AddCommandListener(string commandName, DelegateClientCommand callback);

}