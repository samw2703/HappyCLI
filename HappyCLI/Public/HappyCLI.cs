using HappyCLI.Reflection;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace HappyCLI;

/// <summary>
/// Entry point for HappyCLI. Use <see cref="Execute"/> for a zero-boilerplate setup,
/// or call <see cref="AddHappyCLI"/> to integrate with your own DI container.
/// </summary>
public static class HappyCLI
{
    /// <summary>
    /// Discovers all <see cref="ICommandHandler{TCommand}"/> implementations in <paramref name="commandAssemblies"/>,
    /// parses <paramref name="args"/>, and executes the matching command.
    /// </summary>
    /// <param name="args">The raw command-line arguments from <c>Main</c>.</param>
    /// <param name="commandAssemblies">
    /// One or more assemblies that contain your <see cref="ICommandHandler{TCommand}"/> implementations.
    /// </param>
    /// <param name="setupCustomServices">
    /// Optional callback to register additional services into the DI container before it is built.
    /// Use this to inject your own dependencies into command handlers.
    /// </param>
    /// <param name="outputHandler">
    /// Optional callback that receives any help or error text produced by the CLI.
    /// When omitted, output is written to <see cref="Console.WriteLine(string)"/>.
    /// </param>
    public static async Task Execute(string[] args, Assembly[] commandAssemblies, Action<IServiceCollection>? setupCustomServices = null, Action<string>? outputHandler = null)
    {
        var services = new ServiceCollection();

        services.AddHappyCLI(commandAssemblies);

        setupCustomServices?.Invoke(services);

        var result = await services.BuildServiceProvider().GetRequiredService<IHappyCLI>().Execute(args);

        if (string.IsNullOrEmpty(result))
            return;

        if (outputHandler != null)
            outputHandler(result);
        else
            Console.WriteLine(result);
    }

    /// <summary>
    /// Registers HappyCLI services into <paramref name="services"/>, discovering all
    /// <see cref="ICommandHandler{TCommand}"/> implementations in <paramref name="assemblies"/>.
    /// </summary>
    /// <param name="services">The service collection to add HappyCLI to.</param>
    /// <param name="assemblies">
    /// One or more assemblies that contain your <see cref="ICommandHandler{TCommand}"/> implementations.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> to allow chaining.</returns>
    public static IServiceCollection AddHappyCLI(this IServiceCollection services, Assembly[] assemblies)
    {
        var commandHandlerTypes = assemblies
                .SelectMany(assembly => assembly.GetCommandHandlerTypes())
                .ToList();
        commandHandlerTypes.ForEach(c => services.AddScoped(c));

        services.AddScoped<IHappyCLI>(sp => new Runtime.HappyCLI(commandHandlerTypes.Select(type => new ReflectedCommandHandler(sp.GetRequiredService(type))).ToList()));

        return services;
    }
}
