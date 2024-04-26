using Shared.Exceptions;

namespace Shared.Models.Exceptions;

public class UserAlreadyExistsException : AppException
{
    public UserAlreadyExistsException() : base("User with this email already exists")
    {
    }
    
    public UserAlreadyExistsException(string message) : base(message)
    {
    }

    public UserAlreadyExistsException(string message, Exception innerException) : base(message, innerException)
    {
    }
}