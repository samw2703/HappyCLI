using HappyCLI.Exceptions;
using System.Collections;

namespace HappyCLI.Reflection;

internal class ReflectedCommandHandler
{
    private readonly object _commandHandler;

    public string CommandName { get; }
    public string CommandDescription { get; }
    public Type CommandType { get; }
    public List<ReflectedCommandOption> CommandOptions { get; }

    public ReflectedCommandHandler(object? commandHandler)
    {
        if (commandHandler == null || commandHandler.GetType().GetInterface(typeof(ICommandHandler<>).Name) == null)
            throw new InvalidReflectedObjectException();

        CommandName = commandHandler.GetPropertyValue<string>(nameof(ICommandHandler<object>.CommandName));
        CommandDescription = commandHandler.GetPropertyValue<string>(nameof(ICommandHandler<object>.CommandDescription));
        CommandType = commandHandler.GetType().GetCommandType();
        CommandOptions = commandHandler
            .GetPropertyValue<IEnumerable>(nameof(ICommandHandler<object>.OptionsConfiguration))
            .Cast<object>()
            .Select(option => new ReflectedCommandOption(option))
            .ToList();

        _commandHandler = commandHandler;
    }

    public async Task ExecuteCommand(object command)
    {
        var task = (Task)_commandHandler.InvokeMethod(nameof(ICommandHandler<object>.ExecuteCommand), command)!;

        await task.ConfigureAwait(false);
    }

    public string GetCommandHelpText()
    {
        var helpStrings = CommandOptions.Select(info => info.GetHelpText());

        return $"{CommandName} - {CommandDescription}{Environment.NewLine}{string.Join(Environment.NewLine, helpStrings)}";
    }
}
