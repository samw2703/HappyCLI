using HappyCLI.Configuration.Options;
using System.Collections;

namespace HappyCLI;

/// <summary>
/// Holds the fully validated set of options for a command, produced by <see cref="OptionsConfigurationBuilder{TOptions}.Build"/>.
/// Assign an instance of this class to the <see cref="ICommandHandler{TCommand}.OptionsConfiguration"/> property of your command handler.
/// </summary>
/// <typeparam name="TOptions">The options class that models the command's arguments.</typeparam>
public class OptionsConfiguration<TOptions> : IEnumerable where TOptions : new()
{
    private readonly List<CommandOption<TOptions>> _options;

    internal OptionsConfiguration(List<CommandOption<TOptions>> options)
    {
        _options = options;
    }

    IEnumerator IEnumerable.GetEnumerator() => _options.GetEnumerator();
}
