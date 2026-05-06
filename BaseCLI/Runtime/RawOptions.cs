namespace HappyCLI.Runtime;

public class RawOptions
{
    private readonly List<string> _rawOptions;

    public RawOptions(List<string> rawOptions)
    {
        _rawOptions = rawOptions;
    }

    public bool ContainsFlag(string flag) => _rawOptions.Contains($"-{flag}");

    public bool ContainsHelpFlag() => ContainsFlag(Constants.HelpFlag);

    public List<string> GetValuesForFlag(string flag)
        => GetIndexesForFlag(flag).Select(x => _rawOptions[x + 1]).ToList();

    public int GetFlagCount(string flag) => GetIndexesForFlag(flag).Count;

    public List<string> GetRemainingOptions() => new List<string>(_rawOptions);

    public void RemoveKeysAndValuesForFlag(string flag)
    {
        var indexes = GetIndexesForFlag(flag).OrderByDescending(x => x);

        foreach (var index in indexes)
        {
            if (index + 1 < _rawOptions.Count && (_rawOptions[index + 1] is null || !_rawOptions[index + 1].StartsWith('-')))
                _rawOptions.RemoveAt(index + 1);

            _rawOptions.RemoveAt(index);
        }
    }

    public bool DoesEachFlagHaveAValue(string flag)
    {
        var indexes = GetIndexesForFlag(flag);
        if (indexes.Zip(indexes.Skip(1), (current, next) => next - current).Any(distance => distance == 1))
            return false;

        var valueIndexes = indexes.Select(x => x + 1);

        return valueIndexes.All(index => index < _rawOptions.Count);
    }

    public void RemoveKeysForFlag(string flag)
    {
        var indexes = GetIndexesForFlag(flag).OrderByDescending(x => x);

        foreach (var index in indexes)
            _rawOptions.RemoveAt(index);
    }

    public RawOptions CreateCopy() => new RawOptions(new List<string>(_rawOptions));

    private List<int> GetIndexesForFlag(string flag)
    {
        var indexes = new List<int>();
        for (int idx = 0; idx < _rawOptions.Count; idx++)
        {
            if (_rawOptions[idx] == $"-{flag}")
                indexes.Add(idx);
        }

        return indexes;
    }
}
