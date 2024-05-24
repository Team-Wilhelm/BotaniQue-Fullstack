using api.Core.Services;
using api.Extensions;
using Fleck;
using lib;
using Shared.Exceptions;
using Shared.Models;

namespace api.Events.User;

public class ClientWantsToUpdateProfileImageDto : BaseDtoWithJwt
{
    public string Base64Image { get; set; } = null!;
}

public class ClientWantsToUpdateProfileImage(UserService userService, JwtService jwtService) : BaseEventHandler<ClientWantsToUpdateProfileImageDto>
{
    public override async Task Handle(ClientWantsToUpdateProfileImageDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt);
        try
        {
            var blobUrl = await userService.UpdateUserProfileImage(email, dto.Base64Image);
            socket.SendDto(new ServerConfirmsProfileImageUpdate
            {
                BlobUrl = blobUrl
            });
        }
        catch (Exception e) when (e is not NotFoundException)
        {
            socket.SendDto(new ServerRejectsUpdate
            {
                Error = "Update profile image failed"
            });
        }
    }
}

