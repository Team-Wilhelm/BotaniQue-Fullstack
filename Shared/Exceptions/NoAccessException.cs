using Shared.Exceptions;

namespace Shared.Models.Exceptions;

public class NoAccessException : AppException
{
    public NoAccessException(string message) : base(message)
    {
    }
    
    public NoAccessException() : base("You do not have access to this resource.")
    {
    }
    
    public NoAccessException(string message, Exception innerException) : base(message, innerException)
    {
    }
}