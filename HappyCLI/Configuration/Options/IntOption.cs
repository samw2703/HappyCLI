using HappyCLI.Exceptions;
using HappyCLI.Reflection;
using HappyCLI.Runtime;

namespace HappyCLI.Configuration.Options;

internal class IntOption<TCommand> : CommandOption<TCommand> where TCommand : new()
{
    internal IntOption(string flag, string friendlyName, string propertyName, bool mandatory = false)
        : base(flag, friendlyName, propertyName, mandatory)
    {
    }

    protected override string TypeDescription => "An integer";

    public override List<string> ValidateRawOptions(RawOptions rawOptions)
    {
        var errors = new List<string>();

        var flagCount = rawOptions.GetFlagCount(Flag);

        if (Mandatory && flagCount == 0)
            errors.Add($"No value supplied for mandatory option {FriendlyName} (-{Flag})");

        if (flagCount > 1)
            errors.Add($"Multiple {FriendlyName} (-{Flag}) options found. You must only provide one of these");

        var eachFlagHasAValue = rawOptions.DoesEachFlagHaveAValue(Flag);

        if (!eachFlagHasAValue)
            errors.Add($"No value for {FriendlyName} (-{Flag}) was supplied");

        if (eachFlagHasAValue)
        {
            foreach (var value in rawOptions.GetValuesForFlag(Flag))
            {
                if (!int.TryParse(value, out int _))
                    errors.Add($"\"{value}\" is not a valid integer");
            }
        }

        rawOptions.RemoveKeysAndValuesForFlag(Flag);

        return errors;
    }

    public override void ApplyOptionsToCommand(TCommand command, RawOptions rawOptions)
    {
        if (command is null)
            throw new HappyCLIException($"Cannot apply the {FriendlyName} option because the command instance is null.");

        var value = rawOptions
            .GetValuesForFlag(Flag)
            .SingleOrDefault();

        if (value == null)
            return;

        command.SetPropertyValue(PropertyName, Convert.ToInt32(value));
    }
}
