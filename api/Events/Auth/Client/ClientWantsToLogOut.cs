using api.Events.Auth.Server;
using api.Extensions;
using Fleck;
using lib;

namespace api.Events.Auth.Client;

public class ClientWantsToLogOutDto : BaseDto;

public class ClientWantsToLogOut(WebSocketConnectionService connectionService)
    : BaseEventHandler<ClientWantsToLogOutDto>
{
    public override Task Handle(ClientWantsToLogOutDto dto, IWebSocketConnection socket)
    {
        connectionService.RemoveEmailFromConnection(socket);
        socket.SendDto(new ServerLogsOutUser());
        return Task.CompletedTask;
    }
}