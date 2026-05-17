namespace HappyCLI.Exceptions;

internal class HappyCLIException : Exception
{
    public HappyCLIException(string message)
        : base(message)
    {
    }

    public HappyCLIException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
