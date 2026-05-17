using HappyCLI.Runtime;
using NUnit.Framework;
using System.Collections.Generic;

namespace HappyCLI.Tests;

[TestFixture]
public class RawOptionsTests
{
    [Test]
    public void GetValuesForFlag_WhenFlagAppearsMultipleTimes_ReturnsAllAssociatedValues()
    {
        var rawOptions = new RawOptions(new List<string> { "-name", "sam", "-age", "4", "-name", "bob" });

        var values = rawOptions.GetValuesForFlag("name");

        Assert.That(values, Is.EqualTo(new List<string> { "sam", "bob" }));
    }

    [Test]
    public void DoesEachFlagHaveAValue_WhenFlagsAreConsecutive_ReturnsFalse()
    {
        var rawOptions = new RawOptions(new List<string> { "-name", "-name", "sam" });

        var result = rawOptions.DoesEachFlagHaveAValue("name");

        Assert.That(result, Is.False);
    }

    [Test]
    public void RemoveKeysAndValuesForFlag_RemovesOnlyMatchingFlagsAndTheirValues()
    {
        var rawOptions = new RawOptions(new List<string> { "-name", "sam", "-age", "4", "-name", "bob", "tail" });

        rawOptions.RemoveKeysAndValuesForFlag("name");

        Assert.That(rawOptions.GetRemainingOptions(), Is.EqualTo(new List<string> { "-age", "4", "tail" }));
    }

    [Test]
    public void ContainsFlag_WhenFlagExists_ReturnsTrue()
    {
        var rawOptions = new RawOptions(new List<string> { "-name", "sam" });

        var result = rawOptions.ContainsFlag("name");

        Assert.That(result, Is.True);
    }

    [Test]
    public void ContainsFlag_WhenFlagDoesNotExist_ReturnsFalse()
    {
        var rawOptions = new RawOptions(new List<string> { "-name", "sam" });

        var result = rawOptions.ContainsFlag("age");

        Assert.That(result, Is.False);
    }

    [Test]
    public void ContainsHelpFlag_WhenHelpFlagExists_ReturnsTrue()
    {
        var rawOptions = new RawOptions(new List<string> { "-h" });

        var result = rawOptions.ContainsHelpFlag();

        Assert.That(result, Is.True);
    }

    [Test]
    public void GetFlagCount_WhenFlagExistsMultipleTimes_ReturnsCount()
    {
        var rawOptions = new RawOptions(new List<string> { "-name", "sam", "-name", "bob", "-age", "4" });

        var count = rawOptions.GetFlagCount("name");

        Assert.That(count, Is.EqualTo(2));
    }

    [Test]
    public void GetRemainingOptions_ReturnsCopyOfUnderlyingOptions()
    {
        var original = new List<string> { "-name", "sam" };
        var rawOptions = new RawOptions(original);

        var remaining = rawOptions.GetRemainingOptions();
        remaining.Add("-age");

        Assert.That(rawOptions.GetRemainingOptions(), Is.EqualTo(original));
    }

    [Test]
    public void DoesEachFlagHaveAValue_WhenEachOccurrenceHasValue_ReturnsTrue()
    {
        var rawOptions = new RawOptions(new List<string> { "-name", "sam", "-age", "4", "-name", "bob" });

        var result = rawOptions.DoesEachFlagHaveAValue("name");

        Assert.That(result, Is.True);
    }

    [Test]
    public void DoesEachFlagHaveAValue_WhenLastFlagHasNoValue_ReturnsFalse()
    {
        var rawOptions = new RawOptions(new List<string> { "-name", "sam", "-name" });

        var result = rawOptions.DoesEachFlagHaveAValue("name");

        Assert.That(result, Is.False);
    }

    [Test]
    public void RemoveKeysForFlag_RemovesOnlyMatchingKeys()
    {
        var rawOptions = new RawOptions(new List<string> { "-name", "sam", "-name", "bob", "-age", "4" });

        rawOptions.RemoveKeysForFlag("name");

        Assert.That(rawOptions.GetRemainingOptions(), Is.EqualTo(new List<string> { "sam", "bob", "-age", "4" }));
    }

    [Test]
    public void CreateCopy_ReturnsIndependentCopy()
    {
        var rawOptions = new RawOptions(new List<string> { "-name", "sam", "-age", "4" });
        var copy = rawOptions.CreateCopy();

        copy.RemoveKeysAndValuesForFlag("name");

        Assert.That(rawOptions.GetRemainingOptions(), Is.EqualTo(new List<string> { "-name", "sam", "-age", "4" }));
        Assert.That(copy.GetRemainingOptions(), Is.EqualTo(new List<string> { "-age", "4" }));
    }
}
