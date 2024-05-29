using api.Core.Services;
using Fleck;
using lib;
using Shared.Dtos.FromClient.Identity;
using Shared.Exceptions;

namespace api.Events.Auth.Client;

public class ClientWantsToLogInDto : BaseDto
{
    public LoginDto LoginDto { get; set; } = null!;
}

public class ClientWantsToLogIn(UserService userService, InitialDataHelper initialDataHelper)
    : BaseEventHandler<ClientWantsToLogInDto>
{
    public override async Task Handle(ClientWantsToLogInDto dto, IWebSocketConnection socket)
    {
        var jwt = await userService.Login(dto.LoginDto);
        if (jwt == null) throw new InvalidCredentialsException();

        await initialDataHelper.SendInitialData(socket, jwt);
    }
}

