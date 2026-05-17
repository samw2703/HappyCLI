using HappyCLI.Configuration.Options;
using HappyCLI.Exceptions;
using HappyCLI.Runtime;

namespace HappyCLI.Reflection;

internal class ReflectedCommandOption
{
    private readonly object _commandOption;

    public ReflectedCommandOption(object? commandOption)
    {
        if (commandOption == null || !IsCommandOption(commandOption))
            throw new InvalidReflectedObjectException();

        _commandOption = commandOption;
    }

    public string GetHelpText() => (string)_commandOption.InvokeMethod(nameof(CommandOption<object>.GetHelpText))!;

    public List<string> ValidateRawOptions(RawOptions rawOptions) => (List<string>)_commandOption.InvokeMethod(nameof(CommandOption<object>.ValidateRawOptions), rawOptions)!;

    public void ApplyOptionsToCommand(object command, RawOptions rawOptions) => _commandOption.InvokeMethod(nameof(CommandOption<object>.ApplyOptionsToCommand), command, rawOptions);

    private bool IsCommandOption(object commandOption)
        => GetGenericInheritanceHierarchy(commandOption.GetType()).Contains(typeof(CommandOption<>));

    private IEnumerable<Type> GetGenericInheritanceHierarchy(Type type)
    {
        for (var current = type; current != null; current = current.BaseType)
            yield return current.IsGenericType ? current.GetGenericTypeDefinition() : current;
    }
}
