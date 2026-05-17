using HappyCLI.Configuration.Options;
using HappyCLI.Exceptions;
using HappyCLI.Runtime;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HappyCLI.Tests.Temp;

[TestFixture]
public class IntCollectionOptionTests
{
    [Test]
    public void ValidateRawOptions_WhenMandatoryFlagMissing_ReturnsMandatoryError()
    {
        var option = new IntCollectionOption<TestOptions>("num", "Number", nameof(TestOptions.Numbers), mandatory: true);

        var errors = option.ValidateRawOptions(new RawOptions(new List<string>()));

        Assert.That(errors.Single(), Does.Contain("No value supplied for mandatory option Number (-num)"));
    }

    [Test]
    public void ValidateRawOptions_WhenFlagExistsButNotValue_ReturnsValueMissingErrorAndRemovesOnlyFlag()
    {
        var option = new IntCollectionOption<TestOptions>("num", "Number", nameof(TestOptions.Numbers));
        var rawOptions = new RawOptions(new List<string> { "-num" });

        var errors = option.ValidateRawOptions(rawOptions);

        Assert.That(errors.Single(), Does.Contain("No value for Number (-num) was supplied"));
        Assert.That(rawOptions.GetRemainingOptions(), Is.Empty);
    }

    [Test]
    public void ValidateRawOptions_WhenValueIsNotInteger_ReturnsInvalidIntegerError()
    {
        var option = new IntCollectionOption<TestOptions>("num", "Number", nameof(TestOptions.Numbers));

        var errors = option.ValidateRawOptions(new RawOptions(new List<string> { "-num", "abc" }));

        Assert.That(errors.Single(), Does.Contain("\"abc\" is not a valid integer"));
    }

    [Test]
    public void ValidateRawOptions_WhenNotMandatoryAndNoFlag_NoErrors()
    {
        var option = new IntCollectionOption<TestOptions>("num", "Number", nameof(TestOptions.Numbers));

        var errors = option.ValidateRawOptions(new RawOptions(new List<string>()));

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void ValidateRawOptions_WhenFlagValuesExist_RemovesFlagAndValuesFromRawOptions()
    {
        var option = new IntCollectionOption<TestOptions>("num", "Number", nameof(TestOptions.Numbers));
        var rawOptions = new RawOptions(new List<string> { "-num", "1", "-name", "sam", "-num", "2" });

        option.ValidateRawOptions(rawOptions);

        Assert.That(rawOptions.GetRemainingOptions(), Is.EqualTo(new List<string> { "-name", "sam" }));
    }

    [Test]
    public void ApplyOptionsToCommand_WhenFlagValuesExist_SetsCommandProperty()
    {
        var option = new IntCollectionOption<TestOptions>("num", "Number", nameof(TestOptions.Numbers));
        var command = new TestOptions();

        option.ApplyOptionsToCommand(command, new RawOptions(new List<string> { "-num", "1", "-num", "2" }));

        Assert.That(command.Numbers, Is.EqualTo(new List<int> { 1, 2 }));
    }

    [Test]
    public void ApplyOptionsToCommand_WhenNoFlagValuesExist_SetsEmptyCollection()
    {
        var option = new IntCollectionOption<TestOptions>("num", "Number", nameof(TestOptions.Numbers));
        var command = new TestOptions { Numbers = new List<int> { 99 } };

        option.ApplyOptionsToCommand(command, new RawOptions(new List<string> { "-other", "value" }));

        Assert.That(command.Numbers, Is.Empty);
    }

    [Test]
    public void ApplyOptionsToCommand_CommandIsNull_ThrowsHappyCLIException()
    {
        var option = new IntCollectionOption<TestOptions>("num", "Number", nameof(TestOptions.Numbers));

        Assert.Throws<HappyCLIException>((Action)(() => option.ApplyOptionsToCommand(null, new RawOptions(new List<string>()))));
    }

    private class TestOptions
    {
        public List<int> Numbers { get; set; } = new();
    }
}
