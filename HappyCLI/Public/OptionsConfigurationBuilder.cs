using HappyCLI.Configuration.Options;
using HappyCLI.Exceptions;
using System.Linq.Expressions;
using System.Reflection;

namespace HappyCLI;

/// <summary>
/// Fluent builder for constructing an <see cref="OptionsConfiguration{TOptions}"/>.
/// Chain <see cref="Add(string, string)"/> calls — one per option — then call <see cref="Build"/> to finalise.
/// </summary>
/// <typeparam name="TOptions">
/// The options class that models the command's arguments.
/// Must have a public parameterless constructor.
/// </typeparam>
public class OptionsConfigurationBuilder<TOptions> where TOptions : new()
{
    private readonly List<CommandOption<TOptions>> _options = new List<CommandOption<TOptions>>();

    /// <summary>
    /// Begins the registration of a new option with the given command-line flag and display name.
    /// Call one of the <c>For…</c> methods on the returned <see cref="OptionTypeSelector{TOptions}"/>
    /// to specify the option's type and target property.
    /// </summary>
    /// <param name="flag">
    /// The short flag used on the command line, without the leading dash (e.g. <c>"s"</c> for <c>-s</c>).
    /// Must be unique within this builder and must not be null or empty.
    /// </param>
    /// <param name="friendlyName">A human-readable label shown in help output.</param>
    /// <returns>An <see cref="OptionTypeSelector{TOptions}"/> used to specify the option type.</returns>
    public OptionTypeSelector<TOptions> Add(string flag, string friendlyName)
    {
        if (string.IsNullOrEmpty(flag))
            throw new OptionsConfigurationException("Flag cannot be null or empty");

        if (DuplicateFlag(flag))
            throw new OptionsConfigurationException($"Duplicate options with flag {flag} registered");

        return new OptionTypeSelector<TOptions>(flag, friendlyName, this);
    }

    /// <summary>
    /// Builds and returns the <see cref="OptionsConfiguration{TOptions}"/> containing all registered options.
    /// Assign the result to <see cref="ICommandHandler{TCommand}.OptionsConfiguration"/>.
    /// </summary>
    public OptionsConfiguration<TOptions> Build() => new OptionsConfiguration<TOptions>(_options);

    internal void Add(CommandOption<TOptions> option) => _options.Add(option);

    private bool DuplicateFlag(string flag) => _options.Any(x => x.Flag == flag);
}

/// <summary>
/// Returned by <see cref="OptionsConfigurationBuilder{TOptions}.Add(string, string)"/> to let you specify
/// the type and target property for an option before returning to the builder.
/// </summary>
/// <typeparam name="TOptions">The options class that models the command's arguments.</typeparam>
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

    /// <summary>
    /// Registers this option as a <see cref="string"/> that maps to <paramref name="getProperty"/>.
    /// Pass <c>-flag "some value"</c> on the command line.
    /// </summary>
    /// <param name="getProperty">A property selector expression (e.g. <c>x => x.MyProp</c>).</param>
    /// <param name="mandatory">When <see langword="true"/>, the CLI will show an error if the flag is omitted.</param>
    /// <returns>The parent <see cref="OptionsConfigurationBuilder{TOptions}"/> to continue chaining.</returns>
    public OptionsConfigurationBuilder<TOptions> ForString(Expression<Func<TOptions, string>> getProperty, bool mandatory = false)
        => For(getProperty, propertyName => new StringOption<TOptions>(_flag, _friendlyName, propertyName, mandatory));

    /// <summary>
    /// Registers this option as an <see cref="int"/> that maps to <paramref name="getProperty"/>.
    /// Pass <c>-flag 42</c> on the command line.
    /// </summary>
    /// <param name="getProperty">A property selector expression (e.g. <c>x => x.MyProp</c>).</param>
    /// <param name="mandatory">When <see langword="true"/>, the CLI will show an error if the flag is omitted.</param>
    /// <returns>The parent <see cref="OptionsConfigurationBuilder{TOptions}"/> to continue chaining.</returns>
    public OptionsConfigurationBuilder<TOptions> ForInt(Expression<Func<TOptions, int>> getProperty, bool mandatory = false)
        => For(getProperty, propertyName => new IntOption<TOptions>(_flag, _friendlyName, propertyName, mandatory));

    /// <summary>
    /// Registers this option as a <see cref="List{T}">List&lt;string&gt;</see> that maps to <paramref name="getProperty"/>.
    /// Repeat the flag for each value: <c>-flag hello -flag world</c>.
    /// </summary>
    /// <param name="getProperty">A property selector expression (e.g. <c>x => x.MyProp</c>).</param>
    /// <param name="mandatory">When <see langword="true"/>, the CLI will show an error if the flag is omitted.</param>
    /// <returns>The parent <see cref="OptionsConfigurationBuilder{TOptions}"/> to continue chaining.</returns>
    public OptionsConfigurationBuilder<TOptions> ForStringCollection(Expression<Func<TOptions, List<string>>> getProperty, bool mandatory = false)
        => For(getProperty, propertyName => new StringCollectionOption<TOptions>(_flag, _friendlyName, propertyName, mandatory));

    /// <summary>
    /// Registers this option as a <see cref="List{T}">List&lt;int&gt;</see> that maps to <paramref name="getProperty"/>.
    /// Repeat the flag for each value: <c>-flag 10 -flag 20</c>.
    /// </summary>
    /// <param name="getProperty">A property selector expression (e.g. <c>x => x.MyProp</c>).</param>
    /// <param name="mandatory">When <see langword="true"/>, the CLI will show an error if the flag is omitted.</param>
    /// <returns>The parent <see cref="OptionsConfigurationBuilder{TOptions}"/> to continue chaining.</returns>
    public OptionsConfigurationBuilder<TOptions> ForIntCollection(Expression<Func<TOptions, List<int>>> getProperty, bool mandatory = false)
        => For(getProperty, propertyName => new IntCollectionOption<TOptions>(_flag, _friendlyName, propertyName, mandatory));

    /// <summary>
    /// Registers this option as a <see cref="bool"/> that maps to <paramref name="getProperty"/>.
    /// The flag acts as a switch: its mere presence sets the property to <see langword="true"/> (e.g. <c>-flag</c>).
    /// Bool options are never mandatory.
    /// </summary>
    /// <param name="getProperty">A property selector expression (e.g. <c>x => x.MyProp</c>).</param>
    /// <returns>The parent <see cref="OptionsConfigurationBuilder{TOptions}"/> to continue chaining.</returns>
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
