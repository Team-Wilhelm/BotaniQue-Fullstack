using api.Events.Global;
using api.Events.User.ServerResponses;
using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Serilog;
using Shared.Exceptions;
using Shared.Models;

namespace api.Events.User;

public class ClientWantsToDeleteProfileImageDto : BaseDtoWithJwt
{
}

public class ClientWantsToDeleteProfileImage(UserService userService, JwtService jwtService) : BaseEventHandler<ClientWantsToDeleteProfileImageDto>
{
    public override async Task Handle(ClientWantsToDeleteProfileImageDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt);
        try
        {
            await userService.DeleteProfileImage(email);
            socket.SendDto(new ServerConfirmsDeleteProfileImage());
        } catch (Exception e) when (e is not NotFoundException)
        {
            socket.SendDto(new ServerRejectsUpdate
            {
                Error = "Delete failed"
            });
        }
    }
}

