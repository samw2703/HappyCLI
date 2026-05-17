using HappyCLI.Exceptions;
using HappyCLI.Reflection;

namespace HappyCLI.Runtime;

internal class HappyCLI : IHappyCLI
{
    private readonly List<ReflectedCommandHandler> _reflectedCommandHandlers;

    public HappyCLI(List<ReflectedCommandHandler> reflectedCommandHandlers)
    {
        _reflectedCommandHandlers = reflectedCommandHandlers;
    }

    public async Task<string> Execute(string[] args)
    {
        try
        {
            (var commandHandler, var rawOptions) = GetCommandHandlerAndRawOptions(args);

            if (commandHandler == null)
                return GetToolHelpText();

            if (rawOptions.ContainsHelpFlag())
                return commandHandler.GetCommandHelpText();

            var validationErrors = ValidateRawOptions(commandHandler, rawOptions);

            if (validationErrors.Any())
                return commandHandler.GetCommandHelpText();

            var command = ParseRawOptionsToCommand(commandHandler, rawOptions);
            await commandHandler.ExecuteCommand(command);

            return "";
        }
        catch (OptionsConfigurationException ex)
        {
            return $"Options configuration error: {ex.Message}";
        }
        catch (InvalidReflectedObjectException)
        {
            return "Something went wrong";
        }
        catch (HappyCLIException)
        {
            return "Something went wrong";
        }
    }

    private (ReflectedCommandHandler? CommandHandler, RawOptions RawOptions) GetCommandHandlerAndRawOptions(string[] args)
    {
        if (args.Length == 0)
            return (null, new RawOptions(new List<string>()));

        if (GetIndexOfFirstFlag(args) == 0)
            return (null, new RawOptions(args.ToList()));

        return (GetReflectedCommandHandler(args.First()), new RawOptions(args.Skip(1).Take(args.Length - 1).ToList()));
    }

    private string GetToolHelpText() => string.Join(Environment.NewLine, _reflectedCommandHandlers.Select(x => $"{x.CommandName} - {x.CommandDescription}"));

    private ReflectedCommandHandler? GetReflectedCommandHandler(string commandName) => _reflectedCommandHandlers.SingleOrDefault(cmd => cmd.CommandName == commandName);

    private int? GetIndexOfFirstFlag(string[] args)
    {
        var arg = args.FirstOrDefault(x => x.StartsWith('-'));

        return arg == null ? null : Array.IndexOf(args, arg);
    }

    private static object ParseRawOptionsToCommand(ReflectedCommandHandler reflectedCommandHandler, RawOptions rawOptions)
    {
        var command = Activator.CreateInstance(reflectedCommandHandler.CommandType);

        if (command == null)
            throw new HappyCLIException($"Unable to create an instance of the command type {reflectedCommandHandler.CommandType.FullName}.");

        foreach (var option in reflectedCommandHandler.CommandOptions)
            option.ApplyOptionsToCommand(command, rawOptions);

        return command!;
    }

    private static List<string> ValidateRawOptions(ReflectedCommandHandler reflectedCommandHandler, RawOptions rawOptions)
    {
        var rawOptionsCopy = rawOptions.CreateCopy();
        var errors = new List<string>();

        foreach (var option in reflectedCommandHandler.CommandOptions)
            errors.AddRange(option.ValidateRawOptions(rawOptionsCopy));

        var remainingOptions = rawOptionsCopy.GetRemainingOptions();
        if (remainingOptions.Any())
            errors.Add($"Unable to parse the following arguments:\n{string.Join("\n", remainingOptions)}");

        return errors;
    }
}
