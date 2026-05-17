namespace HappyCLI.Exceptions;

internal class InvalidReflectedObjectException : Exception
{
    public InvalidReflectedObjectException()
        : this("Encountered an invalid reflected CLI object.")
    {
    }

    public InvalidReflectedObjectException(string message)
        : base(message)
    {
    }
}
