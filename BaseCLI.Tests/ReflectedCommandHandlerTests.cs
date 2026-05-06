using HappyCLI.Configuration.Options;
using HappyCLI.Exceptions;
using HappyCLI.Reflection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyCLI.Tests;

[TestFixture]
public class ReflectedCommandHandlerTests
{
    [Test]
    public void Ctor_WithNullHandler_ThrowsInvalidReflectedObjectException()
    {
        Assert.Throws<InvalidReflectedObjectException>(() => new ReflectedCommandHandler(null));
    }

    [Test]
    public async Task ExecuteCommand_WithValidCommand_InvokesUnderlyingHandler()
    {
        var handler = new RecordingCommandHandler();
        var reflected = new ReflectedCommandHandler(handler);

        await reflected.ExecuteCommand(new RecordingOptions { Name = "sam" });

        Assert.That(handler.CallCount, Is.EqualTo(1));
        Assert.That(handler.LastOptions?.Name, Is.EqualTo("sam"));
    }

    [Test]
    public void GetCommandHelpText_IncludesCommandAndOptionHelp()
    {
        var reflected = new ReflectedCommandHandler(new RecordingCommandHandler());

        var helpText = reflected.GetCommandHelpText();

        Assert.That(helpText, Is.EqualTo($"record - records options{Environment.NewLine}-name Name - A string"));
    }

    [Test]
    public void Ctor_WithNonCommandHandlerObject_ThrowsInvalidReflectedObjectException()
    {
        Assert.Throws<InvalidReflectedObjectException>(() => new ReflectedCommandHandler(""));
    }

    [Test]
    public void Ctor_WithCommandHandlerObject_NoExceptionThrown()
    {
        Assert.DoesNotThrow(() => new ReflectedCommandHandler(new RecordingCommandHandler()));
    }

    [Test]
    public void CommandName_ReturnsUnderlyingHandlerCommandName()
    {
        var reflected = new ReflectedCommandHandler(new RecordingCommandHandler());

        Assert.That(reflected.CommandName, Is.EqualTo("record"));
    }

    [Test]
    public void CommandDescription_ReturnsUnderlyingHandlerCommandDescription()
    {
        var reflected = new ReflectedCommandHandler(new RecordingCommandHandler());

        Assert.That(reflected.CommandDescription, Is.EqualTo("records options"));
    }

    [Test]
    public void CommandType_ReturnsUnderlyingHandlerCommandType()
    {
        var reflected = new ReflectedCommandHandler(new RecordingCommandHandler());

        Assert.That(reflected.CommandType, Is.EqualTo(typeof(RecordingOptions)));
    }

    [Test]
    public void CommandOptions_ReturnsCorrectReflectedCommandOptions()
    {
        var reflected = new ReflectedCommandHandler(new RecordingCommandHandler());

        Assert.That(reflected.CommandOptions, Has.Count.EqualTo(1));
        Assert.That(reflected.CommandOptions[0].GetHelpText(), Is.EqualTo("-name Name - A string"));
    }

    private class RecordingCommandHandler : ICommandHandler<RecordingOptions>
    {
        public string CommandName => "record";
        public string CommandDescription => "records options";
        public OptionsConfiguration<RecordingOptions> OptionsConfiguration { get; } = new(
            new List<CommandOption<RecordingOptions>>
            {
                new StringOption<RecordingOptions>("name", "Name", nameof(RecordingOptions.Name))
            });

        public int CallCount { get; private set; }
        public RecordingOptions LastOptions { get; private set; }

        public Task ExecuteCommand(RecordingOptions options)
        {
            CallCount++;
            LastOptions = options;
            return Task.CompletedTask;
        }
    }

    private class RecordingOptions
    {
        public string Name { get; set; }
    }
}
