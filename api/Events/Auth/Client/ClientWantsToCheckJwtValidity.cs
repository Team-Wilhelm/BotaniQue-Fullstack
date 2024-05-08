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
public class ClientWantsToCheckJwtValidity : BaseEventHandler<ClientWantsToCheckJwtValidityDto>
{
    public override Task Handle(ClientWantsToCheckJwtValidityDto dto, IWebSocketConnection socket)
    {
        socket.SendDto(new ServerAuthenticatesUser
        {
            Jwt = dto.Jwt,
        });
        return Task.CompletedTask;
    }
}