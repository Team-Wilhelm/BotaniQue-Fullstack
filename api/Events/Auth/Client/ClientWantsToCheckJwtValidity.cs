using api.Core.Services;
using api.Events.Auth.Server;
using api.Extensions;
using Fleck;
using lib;
using Shared.Models;

namespace api.Events.Auth.Client;

public class ClientWantsToCheckJwtValidityDto : BaseDtoWithJwt;

/// <summary>
/// The token's validation is actually checked in Program.cs, but this event is used to request the validation.
/// If the token is not valid, an exception will be thrown, and the GlobalExceptionHandler will catch it, and send a
/// corresponding message to the client.
/// </summary>
public class ClientWantsToCheckJwtValidity(WebSocketConnectionService webSocketConnectionService, JwtService jwtService) : BaseEventHandler<ClientWantsToCheckJwtValidityDto>
{
    public override Task Handle(ClientWantsToCheckJwtValidityDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt);
        webSocketConnectionService.UpdateConnectionEmail(socket, email);
        socket.SendDto(new ServerAuthenticatesUser
        {
            Jwt = dto.Jwt,
        });
        return Task.CompletedTask;
    }
}