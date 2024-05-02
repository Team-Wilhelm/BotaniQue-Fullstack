namespace Shared.Exceptions;

public class InvalidCredentialsException : AppException
{
    public InvalidCredentialsException() : base("Wrong username or password.")
    {
    }

    public InvalidCredentialsException(string message) : base(message)
    {
    }

    public InvalidCredentialsException(string message, Exception innerException) : base(message, innerException)
    {
    }
}