using HappyCLI.Configuration.Options;
using HappyCLI.Exceptions;
using HappyCLI.Reflection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RuntimeHappyCLI = HappyCLI.Runtime.HappyCLI;

namespace HappyCLI.Tests;

[TestFixture]
public class HappyCLIRuntimeTests
{
    [Test]
    public async Task Execute_NoArgs_ReturnsToolHelpText()
    {
        var first = new NoOptionsHandler("alpha", "first command");
        var second = new NoOptionsHandler("beta", "second command");
        var sut = CreateSut(first, second);

        var result = await sut.Execute(Array.Empty<string>());

        Assert.That(result, Is.EqualTo($"alpha - first command{Environment.NewLine}beta - second command"));
        Assert.That(first.CallCount, Is.EqualTo(0));
        Assert.That(second.CallCount, Is.EqualTo(0));
    }

    [TestCase("-h")]
    [TestCase("-unknown")]
    public async Task Execute_FirstArgIsAFlag_ReturnsToolHelpText(string firstArg)
    {
        var handler = new NoOptionsHandler("alpha", "first command");
        var sut = CreateSut(handler);

        var result = await sut.Execute(new[] { firstArg, "value" });

        Assert.That(result, Is.EqualTo("alpha - first command"));
        Assert.That(handler.CallCount, Is.EqualTo(0));
    }

    [Test]
    public async Task Execute_UnknownCommand_ReturnsToolHelpText()
    {
        var handler = new NoOptionsHandler("known", "known command");
        var sut = CreateSut(handler);

        var result = await sut.Execute(new[] { "missing" });

        Assert.That(result, Is.EqualTo("known - known command"));
        Assert.That(handler.CallCount, Is.EqualTo(0));
    }

    [Test]
    public async Task Execute_CommandHelpFlagPresent_ReturnsCommandHelpAndSkipsExecution()
    {
        var handler = new RecordingCommandHandler();
        var sut = CreateSut(handler);

        var result = await sut.Execute(new[] { "record", "-h", "-name", "sam" });

        Assert.That(result, Is.EqualTo($"record - records options{Environment.NewLine}-name Name (mandatory) - A string{Environment.NewLine}-count Count - An integer{Environment.NewLine}-force Force - A boolean{Environment.NewLine}-tag Tag - A collection of strings"));
        Assert.That(handler.CallCount, Is.EqualTo(0));
    }

    [Test]
    public async Task Execute_ValidArguments_ParsesOptionsAndExecutesCommand()
    {
        var handler = new RecordingCommandHandler();
        var sut = CreateSut(handler);

        var result = await sut.Execute(new[] { "record", "-name", "sam", "-count", "42", "-force", "-tag", "first", "-tag", "second" });

        Assert.That(result, Is.EqualTo(string.Empty));
        Assert.That(handler.CallCount, Is.EqualTo(1));
        Assert.That(handler.LastCommand, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(handler.LastCommand!.Name, Is.EqualTo("sam"));
            Assert.That(handler.LastCommand.Count, Is.EqualTo(42));
            Assert.That(handler.LastCommand.Force, Is.True);
            Assert.That(handler.LastCommand.Tags, Is.EqualTo(new List<string> { "first", "second" }));
        });
    }

    [Test]
    public async Task Execute_WhenHandlerIsAsync_WaitsForCommandCompletion()
    {
        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = new RecordingCommandHandler { ExecuteGate = gate.Task };
        var sut = CreateSut(handler);

        var task = sut.Execute(new[] { "record", "-name", "sam" });

        Assert.That(task.IsCompleted, Is.False);
        gate.SetResult(true);

        var result = await task;

        Assert.That(result, Is.EqualTo(string.Empty));
        Assert.That(handler.CallCount, Is.EqualTo(1));
    }

    [Test]
    public async Task Execute_MissingMandatoryValue_ReturnsCommandHelpAndSkipsExecution()
    {
        var handler = new RecordingCommandHandler();
        var sut = CreateSut(handler);

        var result = await sut.Execute(new[] { "record" });

        Assert.That(result, Does.StartWith("record - records options"));
        Assert.That(handler.CallCount, Is.EqualTo(0));
    }

    [Test]
    public async Task Execute_UnknownTrailingArguments_ReturnsCommandHelpAndSkipsExecution()
    {
        var handler = new RecordingCommandHandler();
        var sut = CreateSut(handler);

        var result = await sut.Execute(new[] { "record", "-name", "sam", "-mystery", "value" });

        Assert.That(result, Does.StartWith("record - records options"));
        Assert.That(handler.CallCount, Is.EqualTo(0));
    }

    [Test]
    public async Task Execute_DuplicateSingleValueOption_ReturnsCommandHelpAndSkipsExecution()
    {
        var handler = new RecordingCommandHandler();
        var sut = CreateSut(handler);

        var result = await sut.Execute(new[] { "record", "-name", "sam", "-name", "bob" });

        Assert.That(result, Does.StartWith("record - records options"));
        Assert.That(handler.CallCount, Is.EqualTo(0));
    }

    [TestCase(0)]
    [TestCase(int.MaxValue)]
    public async Task Execute_IntBoundaryValues_AreParsedSuccessfully(int count)
    {
        var handler = new RecordingCommandHandler();
        var sut = CreateSut(handler);

        var result = await sut.Execute(new[] { "record", "-name", "sam", "-count", count.ToString() });

        Assert.That(result, Is.EqualTo(string.Empty));
        Assert.That(handler.LastCommand, Is.Not.Null);
        Assert.That(handler.LastCommand!.Count, Is.EqualTo(count));
    }

    [Test]
    public async Task Execute_InvalidIntegerArgument_ReturnsCommandHelp()
    {
        var handler = new RecordingCommandHandler();
        var sut = CreateSut(handler);

        var result = await sut.Execute(new[] { "record", "-name", "sam", "-count", "not-an-int" });

        Assert.That(result, Does.StartWith("record - records options"));
        Assert.That(handler.CallCount, Is.EqualTo(0));
    }

    [Test]
    public async Task Execute_ConflictingButValidCombination_CommandHelpFlagWins()
    {
        var handler = new RecordingCommandHandler();
        var sut = CreateSut(handler);

        var result = await sut.Execute(new[] { "record", "-h", "-name", "sam", "-name", "bob", "-count", "bad" });

        Assert.That(result, Does.StartWith("record - records options"));
        Assert.That(handler.CallCount, Is.EqualTo(0));
    }

    [Test]
    public void Execute_OptionsConfigurationExceptionFromOption_BubblesAsTargetInvocationException()
    {
        var handler = new ThrowingOptionHandler(new OptionsConfigurationException("invalid option state"));
        var sut = CreateSut(handler);

        var ex = Assert.ThrowsAsync<TargetInvocationException>(async () => await sut.Execute(new[] { "throw-options" }));

        Assert.That(ex!.InnerException, Is.TypeOf<OptionsConfigurationException>());
        Assert.That(ex.InnerException!.Message, Is.EqualTo("invalid option state"));
    }

    [Test]
    public void Execute_InvalidReflectedObjectExceptionFromOption_BubblesAsTargetInvocationException()
    {
        var handler = new ThrowingOptionHandler(new InvalidReflectedObjectException("broken reflected object"));
        var sut = CreateSut(handler);

        var ex = Assert.ThrowsAsync<TargetInvocationException>(async () => await sut.Execute(new[] { "throw-options" }));

        Assert.That(ex!.InnerException, Is.TypeOf<InvalidReflectedObjectException>());
        Assert.That(ex.InnerException!.Message, Is.EqualTo("broken reflected object"));
    }

    [Test]
    public async Task Execute_HappyCliException_ReturnsGenericErrorMessage()
    {
        var handler = new MisconfiguredPropertyHandler();
        var sut = CreateSut(handler);

        var result = await sut.Execute(new[] { "misconfigured", "-name", "sam" });

        Assert.That(result, Is.EqualTo("Something went wrong"));
    }

    [Test]
    public void Execute_WhenCommandThrowsDependencyException_BubblesException()
    {
        var handler = new ExplodingHandler(new InvalidOperationException("dependency failed"));
        var sut = CreateSut(handler);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.Execute(new[] { "explode" }));

        Assert.That(ex!.Message, Is.EqualTo("dependency failed"));
    }

    [Test]
    public void Execute_WhenCommandIsCancelled_BubblesOperationCanceledException()
    {
        var handler = new ExplodingHandler(new OperationCanceledException("cancelled"));
        var sut = CreateSut(handler);

        Assert.ThrowsAsync<OperationCanceledException>(async () => await sut.Execute(new[] { "explode" }));
    }

    [Test]
    public void Execute_NullArgs_ThrowsNullReferenceException()
    {
        var handler = new NoOptionsHandler("alpha", "first command");
        var sut = CreateSut(handler);

        Assert.ThrowsAsync<NullReferenceException>(async () => await sut.Execute(null!));
    }

    private static RuntimeHappyCLI CreateSut(params object[] handlers)
        => new(handlers.Select(handler => new ReflectedCommandHandler(handler)).ToList());

    private class RecordCommand
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public bool Force { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    private class RecordingCommandHandler : ICommandHandler<RecordCommand>
    {
        public string CommandName => "record";
        public string CommandDescription => "records options";
        public OptionsConfiguration<RecordCommand> OptionsConfiguration { get; } = new OptionsConfigurationBuilder<RecordCommand>()
            .Add("name", "Name").ForString(x => x.Name, mandatory: true)
            .Add("count", "Count").ForInt(x => x.Count)
            .Add("force", "Force").ForBool(x => x.Force)
            .Add("tag", "Tag").ForStringCollection(x => x.Tags)
            .Build();

        public int CallCount { get; private set; }
        public RecordCommand LastCommand { get; private set; } = new();
        public Task ExecuteGate { get; set; } = Task.CompletedTask;

        public async Task ExecuteCommand(RecordCommand command)
        {
            CallCount++;
            LastCommand = command;

            await ExecuteGate;
        }
    }

    private class NoOptionsCommand
    {
    }

    private class NoOptionsHandler : ICommandHandler<NoOptionsCommand>
    {
        public NoOptionsHandler(string commandName, string commandDescription)
        {
            CommandName = commandName;
            CommandDescription = commandDescription;
        }

        public string CommandName { get; }
        public string CommandDescription { get; }
        public int CallCount { get; private set; }
        public OptionsConfiguration<NoOptionsCommand> OptionsConfiguration { get; } = new OptionsConfigurationBuilder<NoOptionsCommand>().Build();

        public Task ExecuteCommand(NoOptionsCommand command)
        {
            CallCount++;
            return Task.CompletedTask;
        }
    }

    private class ThrowingCommand
    {
    }

    private class ThrowingOptionHandler : ICommandHandler<ThrowingCommand>
    {
        public ThrowingOptionHandler(Exception exception)
        {
            OptionsConfiguration = new OptionsConfiguration<ThrowingCommand>(
                new List<CommandOption<ThrowingCommand>>
                {
                    new ThrowingOption(exception)
                });
        }

        public string CommandName => "throw-options";
        public string CommandDescription => "throws during option work";
        public OptionsConfiguration<ThrowingCommand> OptionsConfiguration { get; }

        public Task ExecuteCommand(ThrowingCommand command) => Task.CompletedTask;
    }

    private class ThrowingOption : CommandOption<ThrowingCommand>
    {
        private readonly Exception _exception;

        public ThrowingOption(Exception exception)
            : base("x", "X", nameof(ThrowingCommand), false)
        {
            _exception = exception;
        }

        protected override string TypeDescription => "throws";

        public override List<string> ValidateRawOptions(global::HappyCLI.Runtime.RawOptions rawOptions)
        {
            throw _exception;
        }

        public override void ApplyOptionsToCommand(ThrowingCommand command, global::HappyCLI.Runtime.RawOptions rawOptions)
        {
        }
    }

    private class MisconfiguredCommand
    {
        public string Name { get; set; } = string.Empty;
    }

    private class MisconfiguredPropertyHandler : ICommandHandler<MisconfiguredCommand>
    {
        public string CommandName => "misconfigured";
        public string CommandDescription => "has invalid option mapping";

        public OptionsConfiguration<MisconfiguredCommand> OptionsConfiguration { get; } = new OptionsConfiguration<MisconfiguredCommand>(
            new List<CommandOption<MisconfiguredCommand>>
            {
                new StringOption<MisconfiguredCommand>("name", "Name", "MissingProperty")
            });

        public Task ExecuteCommand(MisconfiguredCommand command) => Task.CompletedTask;
    }

    private class ExplodingCommand
    {
    }

    private class ExplodingHandler : ICommandHandler<ExplodingCommand>
    {
        private readonly Exception _exception;

        public ExplodingHandler(Exception exception)
        {
            _exception = exception;
        }

        public string CommandName => "explode";
        public string CommandDescription => "throws from execute";
        public OptionsConfiguration<ExplodingCommand> OptionsConfiguration { get; } = new OptionsConfigurationBuilder<ExplodingCommand>().Build();

        public Task ExecuteCommand(ExplodingCommand command)
            => Task.FromException(_exception);
    }
}
