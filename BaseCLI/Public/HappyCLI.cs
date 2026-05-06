using HappyCLI.Reflection;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace HappyCLI;

public static class HappyCLI
{
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

    public static IServiceCollection AddHappyCLI(this IServiceCollection sc, Assembly[] assemblies)
    {
        var commandHandlerTypes = assemblies
                .SelectMany(assembly => assembly.GetCommandHandlerTypes())
                .ToList();
        commandHandlerTypes.ForEach(c => sc.AddScoped(c));

        sc.AddScoped<IHappyCLI>(sp => new Runtime.HappyCLI(commandHandlerTypes.Select(type => new ReflectedCommandHandler(sp.GetRequiredService(type))).ToList()));

        return sc;
    }
}
