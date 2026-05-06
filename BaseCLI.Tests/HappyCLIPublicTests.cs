using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using PublicHappyCLI = HappyCLI.HappyCLI;

namespace HappyCLI.Tests;

[TestFixture]
public class HappyCLIPublicTests
{
    [Test]
    public async Task Execute_WhenResultIsEmpty_DoesNotInvokeOutputHandler()
    {
        var outputs = new List<string>();

        await PublicHappyCLI.Execute(
            new[] { "arg1" },
            Array.Empty<Assembly>(),
            setupCustomServices: services => services.AddSingleton<IHappyCLI>(new StubHappyCLI(string.Empty)),
            outputHandler: outputs.Add);

        Assert.That(outputs, Is.Empty);
    }

    [Test]
    public async Task Execute_WhenResultHasContent_InvokesOutputHandler()
    {
        var outputs = new List<string>();

        await PublicHappyCLI.Execute(
            new[] { "arg1" },
            Array.Empty<Assembly>(),
            setupCustomServices: services => services.AddSingleton<IHappyCLI>(new StubHappyCLI("hello")),
            outputHandler: outputs.Add);

        Assert.That(outputs, Is.EqualTo(new[] { "hello" }));
    }

    [Test]
    public async Task Execute_WhenOutputHandlerIsNull_WritesToConsole()
    {
        var originalOut = Console.Out;
        var writer = new StringWriter();
        Console.SetOut(writer);

        try
        {
            await PublicHappyCLI.Execute(
                new[] { "arg1" },
                Array.Empty<Assembly>(),
                setupCustomServices: services => services.AddSingleton<IHappyCLI>(new StubHappyCLI("hello-console")));
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        Assert.That(writer.ToString(), Does.Contain("hello-console"));
    }

    [Test]
    public async Task Execute_UsesSetupCustomServicesInDependencyGraph()
    {
        var outputs = new List<string>();

        await PublicHappyCLI.Execute(
            new[] { "arg1" },
            Array.Empty<Assembly>(),
            setupCustomServices: services =>
            {
                services.AddSingleton<IResultProvider>(new ConstantResultProvider("configured-result"));
                services.AddSingleton<IHappyCLI>(sp => new StubHappyCLI(sp.GetRequiredService<IResultProvider>().Get()));
            },
            outputHandler: outputs.Add);

        Assert.That(outputs, Is.EqualTo(new[] { "configured-result" }));
    }

    [Test]
    public void Execute_WhenDependencyMissing_ThrowsInvalidOperationException()
    {
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await PublicHappyCLI.Execute(
                new[] { "arg1" },
                Array.Empty<Assembly>(),
                setupCustomServices: services => services.AddSingleton<IHappyCLI>(sp =>
                    new StubHappyCLI(sp.GetRequiredService<IResultProvider>().Get()))));
    }

    [Test]
    public void Execute_WhenSetupCustomServicesThrows_BubblesException()
    {
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await PublicHappyCLI.Execute(
                new[] { "arg1" },
                Array.Empty<Assembly>(),
                setupCustomServices: _ => throw new InvalidOperationException("setup failure")));
    }

    [Test]
    public void Execute_NullArgs_ThrowsNullReferenceException()
    {
        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await PublicHappyCLI.Execute(
                null!,
                Array.Empty<Assembly>()));
    }

    [Test]
    public void Execute_NullAssemblies_ThrowsArgumentNullException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await PublicHappyCLI.Execute(
                new[] { "arg1" },
                null!));
    }

    [Test]
    public void AddHappyCLI_NullServiceCollection_ThrowsArgumentNullException()
    {
        IServiceCollection sc = null!;

        Assert.Throws<ArgumentNullException>(() => PublicHappyCLI.AddHappyCLI(sc, Array.Empty<Assembly>()));
    }

    [Test]
    public void AddHappyCLI_NullAssemblies_ThrowsArgumentNullException()
    {
        var sc = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() => sc.AddHappyCLI(null!));
    }

    [Test]
    public async Task AddHappyCLI_EmptyAssemblies_RegistersRuntimeWithNoCommands()
    {
        var sc = new ServiceCollection();
        sc.AddHappyCLI(Array.Empty<Assembly>());

        var provider = sc.BuildServiceProvider();
        var runtime = provider.GetRequiredService<IHappyCLI>();

        var result = await runtime.Execute(Array.Empty<string>());

        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void AddHappyCLI_ReturnsSameServiceCollectionInstance()
    {
        var sc = new ServiceCollection();

        var returned = sc.AddHappyCLI(Array.Empty<Assembly>());

        Assert.That(ReferenceEquals(sc, returned), Is.True);
    }

    private interface IResultProvider
    {
        string Get();
    }

    private class ConstantResultProvider : IResultProvider
    {
        private readonly string _value;

        public ConstantResultProvider(string value)
        {
            _value = value;
        }

        public string Get() => _value;
    }

    private class StubHappyCLI : IHappyCLI
    {
        private readonly string _result;

        public StubHappyCLI(string result)
        {
            _result = result;
        }

        public Task<string> Execute(string[] args) => Task.FromResult(_result);
    }
}
