namespace Shared.Exceptions;

public class InvalidFileFormatException : AppException
{
    public InvalidFileFormatException(string message) : base(message)
    {
    }
    
    public InvalidFileFormatException(string message, Exception innerException) : base(message, innerException)
    {
    }
    
    public InvalidFileFormatException() : base("Invalid file format. Please upload a valid file.")
    {
    }
}