namespace Shared.Models.Exceptions;

public class UserAlreadyExistsException : AppException
{
    public UserAlreadyExistsException(string message) : base(message)
    {
    }

    public UserAlreadyExistsException(string message, Exception innerException) : base(message, innerException)
    {
    }
}