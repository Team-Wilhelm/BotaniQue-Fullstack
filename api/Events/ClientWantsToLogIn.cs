using System.Security.Authentication;
using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Shared.Dtos.FromClient;
using Shared.Models.Exceptions;

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
            throw new InvalidCredentialsException();
        }

        socket.SendDto(new ServerAuthenticatesUser { Jwt = jwt });
    }
}