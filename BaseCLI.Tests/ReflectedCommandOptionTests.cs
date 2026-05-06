using HappyCLI.Configuration.Options;
using HappyCLI.Exceptions;
using HappyCLI.Reflection;
using HappyCLI.Runtime;
using NUnit.Framework;
using System.Collections.Generic;

namespace HappyCLI.Tests;

[TestFixture]
public class ReflectedCommandOptionTests
{
    [Test]
    public void Ctor_WithNullOption_ThrowsInvalidReflectedObjectException()
    {
        Assert.Throws<InvalidReflectedObjectException>(() => new ReflectedCommandOption(null));
    }

    [Test]
    public void ValidateRawOptions_DelegatesToUnderlyingOption()
    {
        var option = new RecordingOption("name", "Name", nameof(TestOptions.Name));
        var reflected = new ReflectedCommandOption(option);

        var result = reflected.ValidateRawOptions(new RawOptions(new List<string>()));

        Assert.That(option.ValidateCallCount, Is.EqualTo(1));
        Assert.That(result, Is.EqualTo(new List<string> { "validation-error" }));
    }

    [Test]
    public void ApplyOptionsToCommand_DelegatesToUnderlyingOption()
    {
        var option = new RecordingOption("name", "Name", nameof(TestOptions.Name));
        var reflected = new ReflectedCommandOption(option);

        reflected.ApplyOptionsToCommand(new TestOptions(), new RawOptions(new List<string>()));

        Assert.That(option.ApplyCallCount, Is.EqualTo(1));
    }

    [Test]
    public void Ctor_WithNonCommandOptionObject_ThrowsInvalidReflectedObjectException()
    {
        Assert.Throws<InvalidReflectedObjectException>(() => new ReflectedCommandOption(""));
    }

    [Test]
    public void Ctor_WithCommandOptionObject_NoExceptionThrown()
    {
        Assert.DoesNotThrow(() => new ReflectedCommandOption(new RecordingOption("name", "Name", nameof(TestOptions.Name))));
    }

    [Test]
    public void GetHelpText_ReturnsUnderlyingHelpText()
    {
        var reflected = new ReflectedCommandOption(new RecordingOption("name", "Name", nameof(TestOptions.Name)));

        var helpText = reflected.GetHelpText();

        Assert.That(helpText, Is.EqualTo("-name Name - test"));
    }

    private class RecordingOption : CommandOption<TestOptions>
    {
        public RecordingOption(string flag, string friendlyName, string propertyName)
            : base(flag, friendlyName, propertyName)
        {
        }

        public int ValidateCallCount { get; private set; }
        public int ApplyCallCount { get; private set; }

        protected override string TypeDescription => "test";

        public override List<string> ValidateRawOptions(RawOptions rawOptions)
        {
            ValidateCallCount++;
            return new List<string> { "validation-error" };
        }

        public override void ApplyOptionsToCommand(TestOptions command, RawOptions rawOptions)
        {
            ApplyCallCount++;
        }
    }

    private class TestOptions
    {
        public string Name { get; set; }
    }
}
