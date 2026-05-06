using HappyCLI.Configuration.Options;
using HappyCLI.Exceptions;
using System.Linq.Expressions;
using System.Reflection;

namespace HappyCLI;

public class OptionsConfigurationBuilder<TOptions> where TOptions : new()
{
    private readonly List<CommandOption<TOptions>> _options = new List<CommandOption<TOptions>>();

    public OptionTypeSelector<TOptions> Add(string flag, string friendlyName)
    {
        if (string.IsNullOrEmpty(flag))
            throw new OptionsConfigurationException("Flag cannot be null or empty");

        if (DuplicateFlag(flag))
            throw new OptionsConfigurationException($"Duplicate options with flag {flag} registered");

        return new OptionTypeSelector<TOptions>(flag, friendlyName, this);
    }

    public OptionsConfiguration<TOptions> Build() => new OptionsConfiguration<TOptions>(_options);

    internal void Add(CommandOption<TOptions> option) => _options.Add(option);

    private bool DuplicateFlag(string flag) => _options.Any(x => x.Flag == flag);
}

public class OptionTypeSelector<TOptions> where TOptions : new()
{
    private readonly string _flag;
    private readonly string _friendlyName;
    private readonly OptionsConfigurationBuilder<TOptions> _builder;

    internal OptionTypeSelector(string flag, string friendlyName, OptionsConfigurationBuilder<TOptions> builder)
    {
        _flag = flag;
        _friendlyName = friendlyName;
        _builder = builder;
    }

    public OptionsConfigurationBuilder<TOptions> ForString(Expression<Func<TOptions, string>> getProperty, bool mandatory = false)
        => For(getProperty, propertyName => new StringOption<TOptions>(_flag, _friendlyName, propertyName, mandatory));

    public OptionsConfigurationBuilder<TOptions> ForInt(Expression<Func<TOptions, int>> getProperty, bool mandatory = false)
        => For(getProperty, propertyName => new IntOption<TOptions>(_flag, _friendlyName, propertyName, mandatory));

    public OptionsConfigurationBuilder<TOptions> ForStringCollection(Expression<Func<TOptions, List<string>>> getProperty, bool mandatory = false)
        => For(getProperty, propertyName => new StringCollectionOption<TOptions>(_flag, _friendlyName, propertyName, mandatory));

    public OptionsConfigurationBuilder<TOptions> ForIntCollection(Expression<Func<TOptions, List<int>>> getProperty, bool mandatory = false)
        => For(getProperty, propertyName => new IntCollectionOption<TOptions>(_flag, _friendlyName, propertyName, mandatory));

    public OptionsConfigurationBuilder<TOptions> ForBool(Expression<Func<TOptions, bool>> getProperty)
        => For(getProperty, propertyName => new BoolOption<TOptions>(_flag, _friendlyName, propertyName));


    private OptionsConfigurationBuilder<TOptions> For<T>(Expression<Func<TOptions, T>> getProperty, Func<string, CommandOption<TOptions>> createOption)
    {
        if (getProperty == null)
            throw new OptionsConfigurationException("getProperty delegate cannot be null");

        var propertyName = GetPropertyName(getProperty);
        if (!PropertyHasSetter(propertyName))
            throw new OptionsConfigurationException($"Property with name {propertyName} does not have a setter");

        _builder.Add(createOption(propertyName));

        return _builder;
    }

    private string GetPropertyName<TProperty>(Expression<Func<TOptions, TProperty>> getProperty)
    {
        if (getProperty.Body.NodeType != ExpressionType.MemberAccess)
            throw new OptionsConfigurationException("getProperty must select a property on your options class");

        return ((MemberExpression)getProperty.Body).Member.Name;
    }

    private bool PropertyHasSetter(string name)
        => typeof(TOptions).GetRuntimeProperties().Single(x => x.Name == name).SetMethod != null;
}
