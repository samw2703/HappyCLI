using HappyCLI.Configuration.Options;
using System.Collections;

namespace HappyCLI;

public class OptionsConfiguration<TOptions> : IEnumerable where TOptions : new()
{
    private readonly List<CommandOption<TOptions>> _options;

    internal OptionsConfiguration(List<CommandOption<TOptions>> options)
    {
        _options = options;
    }

    IEnumerator IEnumerable.GetEnumerator() => _options.GetEnumerator();
}
