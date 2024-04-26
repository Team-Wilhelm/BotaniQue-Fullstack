namespace Shared.Models.Exceptions;

public class RegisterDeviceException: AppException
{
    public RegisterDeviceException() : base("Please provide a device ID to the relevant plant.")
    {
    }
    
    public RegisterDeviceException(string message) : base(message)
    {
    }

    public RegisterDeviceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}