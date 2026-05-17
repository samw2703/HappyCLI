using HappyCLI.Configuration.Options;
using HappyCLI.Exceptions;
using HappyCLI.Runtime;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HappyCLI.Tests.Temp;

[TestFixture]
public class StringCollectionOptionTests
{
    [Test]
    public void ValidateRawOptions_WhenMandatoryFlagMissing_ReturnsMandatoryError()
    {
        var option = new StringCollectionOption<TestOptions>("tag", "Tag", nameof(TestOptions.Tags), mandatory: true);

        var errors = option.ValidateRawOptions(new RawOptions(new List<string>()));

        Assert.That(errors.Single(), Does.Contain("No value supplied for mandatory option Tag (-tag)"));
    }

    [Test]
    public void ValidateRawOptions_WhenFlagExistsButNotValue_ReturnsValueMissingError()
    {
        var option = new StringCollectionOption<TestOptions>("tag", "Tag", nameof(TestOptions.Tags));

        var errors = option.ValidateRawOptions(new RawOptions(new List<string> { "-tag" }));

        Assert.That(errors.Single(), Does.Contain("No value for Tag (-tag) was supplied"));
    }

    [Test]
    public void ValidateRawOptions_WhenNotMandatoryAndNoFlag_NoErrors()
    {
        var option = new StringCollectionOption<TestOptions>("tag", "Tag", nameof(TestOptions.Tags));

        var errors = option.ValidateRawOptions(new RawOptions(new List<string>()));

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void ValidateRawOptions_WhenFlagValuesExist_RemovesFlagAndValuesFromRawOptions()
    {
        var option = new StringCollectionOption<TestOptions>("tag", "Tag", nameof(TestOptions.Tags));
        var rawOptions = new RawOptions(new List<string> { "-tag", "a", "-name", "sam", "-tag", "b" });

        option.ValidateRawOptions(rawOptions);

        Assert.That(rawOptions.GetRemainingOptions(), Is.EqualTo(new List<string> { "-name", "sam" }));
    }

    [Test]
    public void ApplyOptionsToCommand_WhenFlagValuesExist_SetsCommandProperty()
    {
        var option = new StringCollectionOption<TestOptions>("tag", "Tag", nameof(TestOptions.Tags));
        var command = new TestOptions();

        option.ApplyOptionsToCommand(command, new RawOptions(new List<string> { "-tag", "a", "-tag", "b" }));

        Assert.That(command.Tags, Is.EqualTo(new List<string> { "a", "b" }));
    }

    [Test]
    public void ApplyOptionsToCommand_WhenNoFlagValuesExist_SetsEmptyCollection()
    {
        var option = new StringCollectionOption<TestOptions>("tag", "Tag", nameof(TestOptions.Tags));
        var command = new TestOptions { Tags = new List<string> { "existing" } };

        option.ApplyOptionsToCommand(command, new RawOptions(new List<string> { "-other", "value" }));

        Assert.That(command.Tags, Is.Empty);
    }

    [Test]
    public void ApplyOptionsToCommand_CommandIsNull_ThrowsHappyCLIException()
    {
        var option = new StringCollectionOption<TestOptions>("tag", "Tag", nameof(TestOptions.Tags));

        Assert.Throws<HappyCLIException>((Action)(() => option.ApplyOptionsToCommand(null, new RawOptions(new List<string>()))));
    }

    private class TestOptions
    {
        public List<string> Tags { get; set; } = new();
    }
}
