using HappyCLI.Exceptions;
using NUnit.Framework;
using System;
using System.Collections;
using System.Linq;

namespace HappyCLI.Tests.Temp;

[TestFixture]
public class OptionConfigurationBuilderTests
{
    [Test]
    public void Build_WithoutAnyAddedOptions_ReturnsEmptyConfiguration()
    {
        var configuration = new OptionsConfigurationBuilder<TestOptions>().Build();

        Assert.That(Count(configuration), Is.EqualTo(0));
    }

    [Test]
    public void Build_AfterAddingSingleOption_ReturnsConfigurationWithOneOption()
    {
        var configuration = new OptionsConfigurationBuilder<TestOptions>()
            .Add("name", "Name").ForString(x => x.Name)
            .Build();

        Assert.That(Count(configuration), Is.EqualTo(1));
    }

    [Test]
    public void Build_AfterAddingMultipleOptions_ReturnsAllAddedOptions()
    {
        var configuration = new OptionsConfigurationBuilder<TestOptions>()
            .Add("name", "Name").ForString(x => x.Name)
            .Add("age", "Age").ForInt(x => x.Age)
            .Build();

        Assert.That(Count(configuration), Is.EqualTo(2));
        Assert.That(configuration.Cast<object>().Any(), Is.True);
    }

    [Test]
    public void Add_WithNullFlag_ThrowsOptionsConfigurationException()
    {
        var builder = new OptionsConfigurationBuilder<TestOptions>();

        Assert.Throws<OptionsConfigurationException>((Action)(() => builder.Add(null, "Name")));
    }

    [Test]
    public void Add_WithEmptyFlag_ThrowsOptionsConfigurationException()
    {
        var builder = new OptionsConfigurationBuilder<TestOptions>();

        Assert.Throws<OptionsConfigurationException>((Action)(() => builder.Add("", "Name")));
    }

    [Test]
    public void Add_WithDuplicateFlag_ThrowsOptionsConfigurationException()
    {
        var builder = new OptionsConfigurationBuilder<TestOptions>()
            .Add("name", "Name").ForString(x => x.Name);

        Assert.Throws<OptionsConfigurationException>((Action)(() => builder.Add("name", "Name")));
    }

    private static int Count(IEnumerable enumerable)
    {
        var count = 0;
        foreach (var _ in enumerable)
            count++;

        return count;
    }

    private class TestOptions
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
