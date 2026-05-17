using HappyCLI.Runtime;

namespace HappyCLI.Configuration.Options;

internal abstract class CommandOption<TCommand> where TCommand : new()
{
    protected string FriendlyName { get; }
    protected string PropertyName { get; }
    protected bool Mandatory { get; }
    public string Flag { get; }
    protected abstract string TypeDescription { get; }

    protected CommandOption(string flag, string friendlyName, string propertyName, bool mandatory = false)
    {
        Flag = flag;
        FriendlyName = friendlyName;
        PropertyName = propertyName;
        Mandatory = mandatory;
    }

    public string GetHelpText()
        => $"-{Flag} {FriendlyName}{GetMandatoryText(Mandatory)} - {TypeDescription}";

    public abstract List<string> ValidateRawOptions(RawOptions rawOptions);

    public abstract void ApplyOptionsToCommand(TCommand command, RawOptions rawOptions);

    private string GetMandatoryText(bool mandatory) => mandatory ? " (mandatory)" : "";
}
