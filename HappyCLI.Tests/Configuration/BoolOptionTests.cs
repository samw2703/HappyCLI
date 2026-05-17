using HappyCLI.Configuration.Options;
using HappyCLI.Exceptions;
using HappyCLI.Runtime;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HappyCLI.Tests.Temp;

[TestFixture]
public class BoolOptionTests
{
    [Test]
    public void ValidateRawOptions_WhenFlagProvidedMultipleTimes_ReturnsDuplicateError()
    {
        var option = new BoolOption<TestOptions>("debug", "Debug", nameof(TestOptions.Debug));

        var errors = option.ValidateRawOptions(new RawOptions(new List<string> { "-debug", "-debug" }));

        Assert.That(errors.Single(), Does.Contain("Multiple Debug (-debug) options found"));
    }

    [Test]
    public void ValidateRawOptions_WhenNoFlagProvided_ReturnsNoErrors()
    {
        var option = new BoolOption<TestOptions>("debug", "Debug", nameof(TestOptions.Debug));

        var errors = option.ValidateRawOptions(new RawOptions(new List<string>()));

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void ValidateRawOptions_WhenFlagProvided_RemovesFlagsFromRawOptions()
    {
        var option = new BoolOption<TestOptions>("debug", "Debug", nameof(TestOptions.Debug));
        var rawOptions = new RawOptions(new List<string> { "-debug", "-name", "sam", "-debug" });

        option.ValidateRawOptions(rawOptions);

        Assert.That(rawOptions.GetRemainingOptions(), Is.EqualTo(new List<string> { "-name", "sam" }));
    }

    [Test]
    public void ApplyOptionsToCommand_WhenFlagIsPresent_SetsPropertyToTrue()
    {
        var option = new BoolOption<TestOptions>("debug", "Debug", nameof(TestOptions.Debug));
        var command = new TestOptions();

        option.ApplyOptionsToCommand(command, new RawOptions(new List<string> { "-debug" }));

        Assert.That(command.Debug, Is.True);
    }

    [Test]
    public void ApplyOptionsToCommand_WhenFlagIsMissing_PropertyRemainsUnchanged()
    {
        var option = new BoolOption<TestOptions>("debug", "Debug", nameof(TestOptions.Debug));
        var command = new TestOptions { Debug = true };

        option.ApplyOptionsToCommand(command, new RawOptions(new List<string> { "-other", "value" }));

        Assert.That(command.Debug, Is.True);
    }

    [Test]
    public void ApplyOptionsToCommand_CommandIsNull_ThrowsHappyCLIException()
    {
        var option = new BoolOption<TestOptions>("debug", "Debug", nameof(TestOptions.Debug));

        Assert.Throws<HappyCLIException>((Action)(() => option.ApplyOptionsToCommand(null, new RawOptions([]))));
    }

    private class TestOptions
    {
        public bool Debug { get; set; }
    }
}
