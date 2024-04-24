namespace Shared.Models.Exceptions;

public class NotAuthenticatedException : AppException
{ 
    public NotAuthenticatedException() : base("User is not authenticated")
    {
    }
    
    public NotAuthenticatedException(string message) : base(message)
    {
    }
    
    public NotAuthenticatedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}