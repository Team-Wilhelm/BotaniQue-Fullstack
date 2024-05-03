using api.Events.Auth.Server;
using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Shared.Dtos;
using Shared.Dtos.FromClient;
using Shared.Dtos.FromClient.Identity;
using Shared.Exceptions;
using Shared.Models;

namespace api.Events.Auth.Client;

public class ClientWantsToLogInDto : BaseDtoWithJwt
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
        if (user == null) throw new NotFoundException();
        
        connectionService.AuthenticateConnection(socket.ConnectionInfo.Id, user);
        
        var getUserDto = new GetUserDto
        {
            UserEmail = user.UserEmail,
            Username = user.UserName,
            BlobUrl = user.BlobUrl
        };
        
        socket.SendDto(new ServerAuthenticatesUser
        {
            Jwt = jwt,
            GetUserDto = getUserDto
        });
    }
}

