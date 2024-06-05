namespace Shared.Exceptions;

public class AppException : Exception
{
    public AppException(string message) : base(message) { }

    protected AppException(string message, Exception innerException) : base(message, innerException) { }

    protected AppException() { }
}