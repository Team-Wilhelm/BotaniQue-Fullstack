using System.Security.Authentication;
using api.Extensions;
using Fleck;
using lib;
using Serilog;
using Shared.Models.Exceptions;

namespace api;

public static class GlobalExceptionHandler
{
    public static void Handle(this Exception ex, IWebSocketConnection socket, string? message = null)
    {
        Log.Error(ex, "GlobalExceptionHandler");
        ServerSendsErrorMessage serverResponse;
        if (ex is AppException)
        {
            serverResponse = ex switch
            {
                InvalidCredentialsException => new ServerRejectsWrongCredentials { Error = ex.Message },
                UserAlreadyExistsException => new ServerRespondsUserAlreadyExists { Error = ex.Message },
                _ => new ServerSendsErrorMessage { Error = message ?? ex.Message }
            };
        }
        else
        {
            serverResponse = new ServerSendsErrorMessage
            {
                Error = "Something went wrong. Please try again later."
            };
        }

        socket.SendDto(serverResponse);
    }
}

public class ServerSendsErrorMessage : BaseDto
{
    public string Error { get; set; } = null!;
}

public class ServerRejectsWrongCredentials : ServerSendsErrorMessage;
public class ServerRespondsUserAlreadyExists : ServerSendsErrorMessage;