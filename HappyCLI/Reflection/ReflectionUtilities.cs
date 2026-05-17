using HappyCLI.Exceptions;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace HappyCLI.Reflection;

internal static class ReflectionUtilities
{
    public static List<Type> GetCommandHandlerTypes(this Assembly assembly)
        => assembly.GetTypes().Where(type => type.IsCommandHandler()).ToList();

    public static Type GetCommandType(this Type commandHandlerType)
    {
        var interfaceType = commandHandlerType
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<>));

        if (interfaceType == null)
            throw new HappyCLIException($"The type {commandHandlerType.FullName} does not implement ICommandHandler<>.");

        return interfaceType.GetGenericArguments()[0];
    }

    public static void SetPropertyValue(this object obj, string name, object value)
    {
        var propertyInfo = GetProperty(obj, name);

        if (propertyInfo == null)
            throw new HappyCLIException($"The property '{name}' does not exist on type '{obj.GetType().FullName}'.");

        try
        {
            propertyInfo.SetValue(obj, value);
        }
        catch (Exception ex)
        {
            throw new HappyCLIException($"Unable to set the property '{name}' on type '{obj.GetType().FullName}'.", ex);
        }
    }

    public static T GetPropertyValue<T>(this object obj, string name)
    {
        var propertyInfo = GetProperty(obj, name);

        if (propertyInfo == null)
            throw new HappyCLIException($"The property '{name}' does not exist on type '{obj.GetType().FullName}'.");

        var value = propertyInfo.GetValue(obj);

        return (T)(value ?? default(T)!);
    }

    public static object? InvokeMethod(this object obj, string methodName, params object[] parameters)
    {
        try
        {
            return obj.GetType().GetRuntimeMethods().Single(x => x.Name == methodName).Invoke(obj, parameters);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is HappyCLIException)
        {
            return RethrowInnerException(ex.InnerException);
        }
    }

    private static object? RethrowInnerException(Exception exception)
    {
        ExceptionDispatchInfo.Capture(exception).Throw();
        return null;
    }

    private static PropertyInfo? GetProperty(object obj, string name) => obj.GetType().GetRuntimeProperties().SingleOrDefault(x => x.Name == name);

    private static bool IsCommandHandler(this Type type)
        => !type.IsInterface && !type.IsAbstract && type.GetImplementedGenericInterfaces().Contains(typeof(ICommandHandler<>));

    private static IEnumerable<Type> GetImplementedGenericInterfaces(this Type type)
    {
        foreach (var @interface in type.GetInterfaces())
        {
            yield return @interface.IsGenericType
                ? @interface.GetGenericTypeDefinition()
                : @interface;
        }
    }
}
