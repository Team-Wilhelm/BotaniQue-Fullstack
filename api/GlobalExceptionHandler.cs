using api.Events.Auth.Server;
using api.Events.Global;
using api.Extensions;
using Fleck;
using Serilog;
using Shared.Models.Exceptions;

namespace api;

public static class GlobalExceptionHandler
{
    public static void Handle(this Exception ex, IWebSocketConnection socket, string? message = null)
    {
        Log.Error(ex, "An error occurred");
        ServerSendsErrorMessage serverResponse;
        if (ex is AppException)
            serverResponse = ex switch
            {
                InvalidCredentialsException => new ServerRejectsWrongCredentials { Error = ex.Message },
                UserAlreadyExistsException => new ServerRespondsUserAlreadyExists { Error = ex.Message },
                ModelValidationException => new ServerRespondsValidationError { Error = ex. Message },
                NotFoundException => new ServerRespondsNotFound { Error = ex.Message },
                NoAccessException => new ServerRespondsNotAuthorized { Error = ex.Message },
                NotAuthenticatedException => new ServerRespondsNotAuthenticated { Error = ex.Message },
                _ => new ServerSendsErrorMessage { Error = message ?? ex.Message }
            };
        else
            serverResponse = new ServerSendsErrorMessage
            {
                Error = "Something went wrong. Please try again later."
            };

        socket.SendDto(serverResponse);
    }
}