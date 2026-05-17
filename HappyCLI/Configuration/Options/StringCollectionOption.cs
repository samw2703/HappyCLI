using HappyCLI.Exceptions;
using HappyCLI.Reflection;
using HappyCLI.Runtime;

namespace HappyCLI.Configuration.Options;

internal class StringCollectionOption<TCommand> : CommandOption<TCommand> where TCommand : new()
{
    internal StringCollectionOption(string flag, string friendlyName, string propertyName, bool mandatory = false)
        : base(flag, friendlyName, propertyName, mandatory)
    {
    }

    protected override string TypeDescription => "A collection of strings";

    public override List<string> ValidateRawOptions(RawOptions rawOptions)
    {
        var errors = new List<string>();

        if (Mandatory && rawOptions.GetFlagCount(Flag) == 0)
            errors.Add($"No value supplied for mandatory option {FriendlyName} (-{Flag})");

        if (!rawOptions.DoesEachFlagHaveAValue(Flag))
            errors.Add($"No value for {FriendlyName} (-{Flag}) was supplied");

        rawOptions.RemoveKeysAndValuesForFlag(Flag);

        return errors;
    }

    public override void ApplyOptionsToCommand(TCommand command, RawOptions rawOptions)
    {
        if (command is null)
            throw new HappyCLIException($"Cannot apply the {FriendlyName} option because the command instance is null.");

        var value = rawOptions.GetValuesForFlag(Flag);

        if (value == null)
            return;

        command.SetPropertyValue(PropertyName, value);
    }
}
