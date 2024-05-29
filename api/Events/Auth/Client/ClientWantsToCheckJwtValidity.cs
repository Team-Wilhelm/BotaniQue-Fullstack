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
public class ClientWantsToCheckJwtValidity(InitialDataHelper initialDataHelper) : BaseEventHandler<ClientWantsToCheckJwtValidityDto>
{
    public override async Task Handle(ClientWantsToCheckJwtValidityDto dto, IWebSocketConnection socket)
    {
        await initialDataHelper.SendInitialData(socket, dto.Jwt!);
    }
}