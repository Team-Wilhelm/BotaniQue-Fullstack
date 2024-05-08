using api.EventFilters;
using api.Events.Auth.Server;
using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Shared.Dtos.FromClient;
using Shared.Dtos.FromClient.Identity;

namespace api.Events.Auth.Client;

public class ClientWantsToSignUpDto : BaseDto
{
    public RegisterUserDto RegisterUserDto { get; set; }
}

[ValidateDataAnnotations]
public class ClientWantsToSignUp(UserService userService) : BaseEventHandler<ClientWantsToSignUpDto>
{
    public override async Task Handle(ClientWantsToSignUpDto dto, IWebSocketConnection socket)
    {
        var registerUserDto = dto.RegisterUserDto;
        await userService.CreateUser(registerUserDto);
        var serverSignsUserUp = new ServerSignsUserUp();
        socket.SendDto(serverSignsUserUp);
    }
}