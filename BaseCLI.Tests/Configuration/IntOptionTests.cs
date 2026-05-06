using HappyCLI.Configuration.Options;
using HappyCLI.Exceptions;
using HappyCLI.Runtime;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace HappyCLI.Tests.Temp;

[TestFixture]
public class IntOptionTests
{
    [Test]
    public void ValidateRawOptions_WhenMandatoryFlagMissing_ReturnsMandatoryError()
    {
        var option = new IntOption<TestOptions>("age", "Age", nameof(TestOptions.Age), mandatory: true);

        var errors = option.ValidateRawOptions(new RawOptions(new List<string>()));

        Assert.That(errors.Single(), Does.Contain("No value supplied for mandatory option Age (-age)"));
    }

    [Test]
    public void ValidateRawOptions_WhenFlagProvidedMultipleTimes_ReturnsDuplicateError()
    {
        var option = new IntOption<TestOptions>("age", "Age", nameof(TestOptions.Age));

        var errors = option.ValidateRawOptions(new RawOptions(new List<string> { "-age", "1", "-age", "2" }));

        Assert.That(errors.Single(), Does.Contain("Multiple Age (-age) options found"));
    }

    [Test]
    public void ValidateRawOptions_WhenFlagExistsButNotValue_ReturnsValueMissingError()
    {
        var option = new IntOption<TestOptions>("age", "Age", nameof(TestOptions.Age));

        var errors = option.ValidateRawOptions(new RawOptions(new List<string> { "-age" }));

        Assert.That(errors.Single(), Does.Contain("No value for Age (-age) was supplied"));
    }

    [Test]
    public void ValidateRawOptions_WhenValueIsNotInteger_ReturnsInvalidIntegerError()
    {
        var option = new IntOption<TestOptions>("age", "Age", nameof(TestOptions.Age));

        var errors = option.ValidateRawOptions(new RawOptions(new List<string> { "-age", "abc" }));

        Assert.That(errors.Single(), Does.Contain("\"abc\" is not a valid integer"));
    }

    [Test]
    public void ValidateRawOptions_WhenNotMandatoryAndNoFlag_NoErrors()
    {
        var option = new IntOption<TestOptions>("age", "Age", nameof(TestOptions.Age));

        var errors = option.ValidateRawOptions(new RawOptions(new List<string>()));

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void ValidateRawOptions_WhenFlagValueExists_RemovesFlagAndValueFromRawOptions()
    {
        var option = new IntOption<TestOptions>("age", "Age", nameof(TestOptions.Age));
        var rawOptions = new RawOptions(new List<string> { "-age", "4", "-name", "sam" });

        option.ValidateRawOptions(rawOptions);

        Assert.That(rawOptions.GetRemainingOptions(), Is.EqualTo(new List<string> { "-name", "sam" }));
    }

    [Test]
    public void ApplyOptionsToCommand_WhenFlagValueExists_SetsCommandProperty()
    {
        var option = new IntOption<TestOptions>("age", "Age", nameof(TestOptions.Age));
        var command = new TestOptions();

        option.ApplyOptionsToCommand(command, new RawOptions(new List<string> { "-age", "42" }));

        Assert.That(command.Age, Is.EqualTo(42));
    }

    [Test]
    public void ApplyOptionsToCommand_WhenNoFlagValueExists_PropertyRemainsUnchanged()
    {
        var option = new IntOption<TestOptions>("age", "Age", nameof(TestOptions.Age));
        var command = new TestOptions { Age = 10 };

        option.ApplyOptionsToCommand(command, new RawOptions(new List<string> { "-name", "sam" }));

        Assert.That(command.Age, Is.EqualTo(10));
    }

    [Test]
    public void ApplyOptionsToCommand_CommandIsNull_ThrowsHappyCLIException()
    {
        var option = new IntOption<TestOptions>("age", "Age", nameof(TestOptions.Age));

        Assert.Throws<HappyCLIException>(() => option.ApplyOptionsToCommand(null, new RawOptions(new List<string>())));
    }

    private class TestOptions
    {
        public int Age { get; set; }
    }
}
