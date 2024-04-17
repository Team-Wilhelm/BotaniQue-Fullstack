using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Shared.Dtos.FromClient;

namespace api.Events;

public class ClientWantsToLogInDto : BaseDto
{
    public LoginDto LoginDto { get; set; } = null!;
}

public class ServerAuthenticatesUser : BaseDto
{
    public string? Jwt { get; set; }
}

public class ClientWantsToLogIn(UserService userService) : BaseEventHandler<ClientWantsToLogInDto>
{
    public override async Task Handle(ClientWantsToLogInDto dto, IWebSocketConnection socket)
    {
        var jwt = await userService.Login(dto.LoginDto);
        if (jwt == null)
        {
            await socket.Send("Wrong credentials dumbass");
            return;
        }

        socket.SendDto(new ServerAuthenticatesUser { Jwt = jwt });
    }
}