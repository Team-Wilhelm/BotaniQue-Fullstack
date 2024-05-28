using api.Core.Services;
using api.Core.Services.External.BlobStorage;
using api.Events.Auth.Server;
using api.Events.Global;
using api.Extensions;
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

public class ClientWantsToLogIn(WebSocketConnectionService webSocketConnectionService, UserService userService, IBlobStorageService blobStorageService)
    : BaseEventHandler<ClientWantsToLogInDto>
{
    public override async Task Handle(ClientWantsToLogInDto dto, IWebSocketConnection socket)
    {
        var jwt = await userService.Login(dto.LoginDto);
        if (jwt == null) throw new InvalidCredentialsException();

        var user = await userService.GetUserByEmail(dto.LoginDto.Email);
        
        webSocketConnectionService.UpdateConnectionEmail(socket, dto.LoginDto.Email);
        
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
        
       /* socket.SendDto(new ServerSendsUserInfo
        {
            GetUserDto = getUserDto
        });
        */
    }
}

