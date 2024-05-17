using api.Events.User.ServerResponses;
using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Shared.Exceptions;
using Shared.Models;

namespace api.Events.User;

public class ClientWantsToUpdatePasswordDto : BaseDtoWithJwt
{
    public string password { get; set; } = null!;
}

public class ClientWantsToUpdatePassword(UserService userService, JwtService jwtService) : BaseEventHandler<ClientWantsToUpdatePasswordDto>
{
    public override async Task Handle(ClientWantsToUpdatePasswordDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt);
        try
        {
            await userService.UpdatePassword(email, dto.password);
            socket.SendDto(new ServerConfirmsUpdatePassword());
        } catch (Exception e) when (e is not NotFoundException)
        {
            socket.SendDto(new ServerRejectsUpdate
            {
                Error = "Update password failed"
            });
        }
    }
}