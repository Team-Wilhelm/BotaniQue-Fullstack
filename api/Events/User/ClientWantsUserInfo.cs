using api.Core.Services;
using api.Core.Services.External.BlobStorage;
using api.Events.Global;
using api.Extensions;
using Fleck;
using lib;
using Shared.Dtos;
using Shared.Models;

namespace api.Events.User;

public class ClientWantsUserInfoDto : BaseDtoWithJwt
{
    
}

public class ClientWantsUserInfo(UserService userService, JwtService jwtService, IBlobStorageService blobStorageService)
    : BaseEventHandler<ClientWantsUserInfoDto>
{
    public override async Task Handle(ClientWantsUserInfoDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt);
        var user = await userService.GetUserByEmail(email);
        var getUserDto = new GetUserDto
        {
            UserEmail = user.UserEmail,
            Username = user.UserName
        };
        if (user.BlobUrl != null)
        {
            getUserDto.BlobUrl = blobStorageService.GenerateSasUri(user.BlobUrl, false);
        }
        socket.SendDto(new ServerSendsUserInfo
        {
            GetUserDto = getUserDto
        });
    }
}