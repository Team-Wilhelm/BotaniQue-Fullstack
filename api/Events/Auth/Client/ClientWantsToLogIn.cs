using api.Events.Auth.Server;
using api.Events.Global;
using api.Extensions;
using Core.Services;
using Core.Services.External.BlobStorage;
using Fleck;
using lib;
using Shared.Dtos;
using Shared.Dtos.FromClient.Identity;
using Shared.Exceptions;

namespace api.Events.Auth.Client;

public class ClientWantsToLogInDto : BaseDto
{
    public LoginDto LoginDto { get; set; } = null!;
}

public class ClientWantsToLogIn(UserService userService, IBlobStorageService blobStorageService)
    : BaseEventHandler<ClientWantsToLogInDto>
{
    public override async Task Handle(ClientWantsToLogInDto dto, IWebSocketConnection socket)
    {
        var jwt = await userService.Login(dto.LoginDto);
        if (jwt == null) throw new InvalidCredentialsException();

        var user = await userService.GetUserByEmail(dto.LoginDto.Email);
        
        var getUserDto = new GetUserDto
        {
            UserEmail = user.UserEmail,
            Username = user.UserName,
        };
        
        if (!string.IsNullOrEmpty(user.BlobUrl))
        {
            getUserDto.BlobUrl = blobStorageService.GenerateSasUri(user.BlobUrl, false);
        }
        
        socket.SendDto(new ServerAuthenticatesUser
        {
            Jwt = jwt,
            
        });
        socket.SendDto(new ServerSendsUserInfo
        {
            GetUserDto = getUserDto
        });
    }
}

