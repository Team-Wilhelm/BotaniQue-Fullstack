using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Shared.Dtos.FromClient;

namespace api.Events.User;

public class ClientWantsToUpdateUserDto : BaseDto
{
    public UpdateUserDto UpdateUserDto { get; set; } = null!;
}

public class ClientWantsToUpdateProfile (UserService userService) : BaseEventHandler<ClientWantsToUpdateUserDto>
{
    public override async Task Handle(ClientWantsToUpdateUserDto dto, IWebSocketConnection socket)
    {
        await userService.UpdateUser(dto.UpdateUserDto);
        socket.SendDto(new ServerConfirmsUpdate());
    }
}

public class ServerConfirmsUpdate : BaseDto
{
}