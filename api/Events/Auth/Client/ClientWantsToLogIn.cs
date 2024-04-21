using api.Events.Auth.Server;
using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Shared.Dtos.FromClient;
using Shared.Models.Exceptions;

namespace api.Events.Auth.Client;

public class ClientWantsToLogInDto : BaseDto
{
    public LoginDto LoginDto { get; set; } = null!;
}

public class ClientWantsToLogIn(WebSocketConnectionService connectionService, UserService userService)
    : BaseEventHandler<ClientWantsToLogInDto>
{
    public override async Task Handle(ClientWantsToLogInDto dto, IWebSocketConnection socket)
    {
        var jwt = await userService.Login(dto.LoginDto);
        if (jwt == null) throw new InvalidCredentialsException();

        var user = await userService.GetUserByEmail(dto.LoginDto.Email);
        connectionService.AuthenticateConnection(socket.ConnectionInfo.Id, user!);
        socket.SendDto(new ServerAuthenticatesUser { Jwt = jwt });
    }
}