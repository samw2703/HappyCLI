using HappyCLI.Configuration.Options;
using HappyCLI.Exceptions;
using HappyCLI.Runtime;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace HappyCLI.Tests.Temp;

[TestFixture]
public class StringOptionTests
{
    [Test]
    public void ValidateRawOptions_WhenMandatoryFlagMissing_ReturnsMandatoryError()
    {
        var option = new StringOption<TestOptions>("name", "Name", nameof(TestOptions.Name), mandatory: true);

        var errors = option.ValidateRawOptions(new RawOptions(new List<string>()));

        Assert.That(errors.Single(), Does.Contain("No value supplied for mandatory option Name (-name)"));
    }

    [Test]
    public void ValidateRawOptions_WhenFlagProvidedMultipleTimes_ReturnsDuplicateError()
    {
        var option = new StringOption<TestOptions>("name", "Name", nameof(TestOptions.Name));

        var errors = option.ValidateRawOptions(new RawOptions(new List<string> { "-name", "sam", "-name", "bob" }));

        Assert.That(errors.Single(), Does.Contain("Multiple Name (-name) options found"));
    }

    [Test]
    public void ApplyOptionsToCommand_WhenFlagValueExists_SetsCommandProperty()
    {
        var option = new StringOption<TestOptions>("name", "Name", nameof(TestOptions.Name));
        var command = new TestOptions();

        option.ApplyOptionsToCommand(command, new RawOptions(new List<string> { "-name", "sam" }));

        Assert.That(command.Name, Is.EqualTo("sam"));
    }

    [Test]
    public void ValidateRawOptions_WhenNotMandatoryAndNoFlag_NoErrors()
    {
        var option = new StringOption<TestOptions>("name", "Name", nameof(TestOptions.Name));

        var errors = option.ValidateRawOptions(new RawOptions(new List<string>()));

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void ValidateRawOptions_WhenFlagExistsButNotValue_ReturnsValueMissingError()
    {
        var option = new StringOption<TestOptions>("name", "Name", nameof(TestOptions.Name));

        var errors = option.ValidateRawOptions(new RawOptions(new List<string> { "-name" }));

        Assert.That(errors.Single(), Does.Contain("No value for Name (-name) was supplied"));
    }

    [Test]
    public void ValidateRawOptions_WhenFlagValueExists_RemovesFlagAndValueFromRawOptions()
    {
        var option = new StringOption<TestOptions>("name", "Name", nameof(TestOptions.Name));
        var rawOptions = new RawOptions(new List<string> { "-name", "sam", "-age", "4" });

        option.ValidateRawOptions(rawOptions);

        Assert.That(rawOptions.GetRemainingOptions(), Is.EqualTo(new List<string> { "-age", "4" }));
    }

    [Test]
    public void ApplyOptionsToCommand_CommandIsNull_ThrowsHappyCLIException()
    {
        var option = new StringOption<TestOptions>("name", "Name", nameof(TestOptions.Name));

        Assert.Throws<HappyCLIException>(() => option.ApplyOptionsToCommand(null, new RawOptions(new List<string>())));
    }

    [Test]
    public void ApplyOptionsToCommand_NoFlagValueExists_PropertyRemainsUnchanged()
    {
        var option = new StringOption<TestOptions>("name", "Name", nameof(TestOptions.Name));
        var command = new TestOptions { Name = "existing" };

        option.ApplyOptionsToCommand(command, new RawOptions(new List<string> { "-other", "value" }));

        Assert.That(command.Name, Is.EqualTo("existing"));
    }

    private class TestOptions
    {
        public string Name { get; set; }
    }
}
