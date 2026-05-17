using HappyCLI.Exceptions;
using HappyCLI.Reflection;
using HappyCLI.Runtime;

namespace HappyCLI.Configuration.Options;

internal class BoolOption<TCommand> : CommandOption<TCommand> where TCommand : new()
{
	internal BoolOption(string flag, string friendlyName, string propertyName) 
		: base(flag, friendlyName, propertyName, false)
	{
	}

	protected override string TypeDescription => "A boolean";

    public override List<string> ValidateRawOptions(RawOptions rawOptions)
	{
        var errors = new List<string>();

		if (rawOptions.GetFlagCount(Flag) > 1)
			errors.Add($"Multiple {FriendlyName} (-{Flag}) options found. You must only provide one of these");

		rawOptions.RemoveKeysForFlag(Flag);

        return errors;
	}

    public override void ApplyOptionsToCommand(TCommand command, RawOptions rawOptions)
	{
        if (command is null)
            throw new HappyCLIException($"Cannot apply the {FriendlyName} option because the command instance is null.");

        var flagPresent = rawOptions.ContainsFlag(Flag);

		if (flagPresent)
			command.SetPropertyValue(PropertyName, true);
	}
}
