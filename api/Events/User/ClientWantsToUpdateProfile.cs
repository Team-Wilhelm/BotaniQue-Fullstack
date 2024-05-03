using api.EventFilters;
using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Shared.Dtos;
using Shared.Dtos.FromClient;
using Shared.Exceptions;
using Shared.Models;

namespace api.Events.User;

public class ClientWantsToUpdateUserDto : BaseDtoWithJwt
{
    public UpdateUserDto UpdateUserDto { get; set; } = null!;
}

[ValidateDataAnnotations]
public class ClientWantsToUpdateProfile (UserService userService) : BaseEventHandler<ClientWantsToUpdateUserDto>
{
    public override async Task Handle(ClientWantsToUpdateUserDto dto, IWebSocketConnection socket)
    {
        var getUserDto = await userService.UpdateUser(dto.UpdateUserDto);
        if (getUserDto == null)
        {
            throw new AppException("Failed to update user.");
        }
        socket.SendDto(new ServerConfirmsUpdate
        {
            GetUserDto = getUserDto
        });
    }
}

public class ServerConfirmsUpdate : BaseDto
{
    public GetUserDto? GetUserDto { get; set; } = null!;
}