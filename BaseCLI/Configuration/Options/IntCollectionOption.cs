using HappyCLI.Exceptions;
using HappyCLI.Reflection;
using HappyCLI.Runtime;

namespace HappyCLI.Configuration.Options;

internal class IntCollectionOption<TCommand> : CommandOption<TCommand> where TCommand : new()
{
    internal IntCollectionOption(string flag, string friendlyName, string propertyName, bool mandatory = false)
        : base(flag, friendlyName, propertyName, mandatory)
    {
    }

    protected override string TypeDescription => "A collection of integers";

    public override List<string> ValidateRawOptions(RawOptions rawOptions)
    {
        var errors = new List<string>();

        if (Mandatory && rawOptions.GetFlagCount(Flag) == 0)
            errors.Add($"No value supplied for mandatory option {FriendlyName} (-{Flag})");

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

            rawOptions.RemoveKeysAndValuesForFlag(Flag);
        }
        else
        {
            rawOptions.RemoveKeysForFlag(Flag);
        }

        return errors;
    }

    public override void ApplyOptionsToCommand(TCommand command, RawOptions rawOptions)
    {
        if (command is null)
            throw new HappyCLIException($"Cannot apply the {FriendlyName} option because the command instance is null.");

        var value = rawOptions
            .GetValuesForFlag(Flag)
            .Select(x => Convert.ToInt32((string?)x))
            .ToList();

        command.SetPropertyValue(PropertyName, value);
    }
}
