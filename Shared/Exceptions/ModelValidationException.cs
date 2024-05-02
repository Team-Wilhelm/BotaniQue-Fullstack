namespace Shared.Exceptions;

public class ModelValidationException : AppException
{
    public ModelValidationException(string message) : base(message)
    {
    }

    public ModelValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ModelValidationException()
    {
    }
}