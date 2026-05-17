namespace HappyCLI;

/// <summary>
/// Defines the core CLI execution contract. Resolve this from the DI container
/// after calling <see cref="HappyCLI.AddHappyCLI"/> to run commands.
/// </summary>
public interface IHappyCLI
{
    /// <summary>
    /// Parses <paramref name="args"/> and dispatches to the matching command handler.
    /// </summary>
    /// <param name="args">The raw command-line arguments passed to the application.</param>
    /// <returns>
    /// A non-empty string containing help or error text that should be shown to the user,
    /// or an empty string when a command was executed successfully.
    /// </returns>
    Task<string> Execute(string[] args);
}
