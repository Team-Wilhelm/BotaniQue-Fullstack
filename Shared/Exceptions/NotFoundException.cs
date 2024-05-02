namespace Shared.Exceptions;

public class NotFoundException: AppException
{
    public NotFoundException() : base("Not found")
    {
    }
    
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}