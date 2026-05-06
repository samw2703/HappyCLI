namespace HappyCLI;

public interface ICommandHandler<TCommand> where TCommand : new()
{
    string CommandName { get; }
    string CommandDescription { get; }
    OptionsConfiguration<TCommand> OptionsConfiguration { get; }

    Task ExecuteCommand(TCommand command);
}

public delegate void ConfigureOptions<TCommand>(OptionsConfigurationBuilder<TCommand> builder) where TCommand : new();