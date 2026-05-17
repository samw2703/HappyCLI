namespace HappyCLI;

/// <summary>
/// Implement this interface to define a CLI command with typed options.
/// </summary>
/// <typeparam name="TCommand">
/// The class that models the command's arguments. Must have a public parameterless constructor.
/// </typeparam>
public interface ICommandHandler<TCommand> where TCommand : new()
{
    /// <summary>
    /// The name used on the command line to invoke this command (e.g. <c>build</c>).
    /// </summary>
    string CommandName { get; }

    /// <summary>
    /// A short human-readable description shown in the global help output (<c>-h</c>).
    /// </summary>
    string CommandDescription { get; }

    /// <summary>
    /// Declares the options accepted by this command, built with <see cref="OptionsConfigurationBuilder{TOptions}"/>.
    /// </summary>
    OptionsConfiguration<TCommand> OptionsConfiguration { get; }

    /// <summary>
    /// Contains the logic to run when this command is invoked.
    /// </summary>
    /// <param name="command">A fully populated command instance parsed from the command-line arguments.</param>
    Task ExecuteCommand(TCommand command); 
}
