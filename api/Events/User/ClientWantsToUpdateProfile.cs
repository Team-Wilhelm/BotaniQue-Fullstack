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
public class ClientWantsToUpdateProfile (UserService userService, JwtService jwtService) : BaseEventHandler<ClientWantsToUpdateUserDto>
{
    public override async Task Handle(ClientWantsToUpdateUserDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt);
        var getUserDto = await userService.UpdateUser(dto.UpdateUserDto, email);
        
        if (getUserDto != null)
        {
            socket.SendDto(new ServerConfirmsUpdate
            {
                GetUserDto = getUserDto
            });
        }
        else
        {
            var user = await userService.GetUserByEmail(email);
            
            if (user == null) throw new NotFoundException("User not found");
            
            socket.SendDto(new ServerRejectsUpdate
            {
                ErrorMessage = "Update failed",
                GetUserDto =new GetUserDto
                {
                    UserEmail = email,
                    Username = user.UserName,
                    BlobUrl = user.BlobUrl
                }
            });
        }
    }
}

public class ServerConfirmsUpdate : BaseDto
{
    public GetUserDto GetUserDto { get; set; } = null!;
}

public class ServerRejectsUpdate : BaseDto
{
    public string ErrorMessage { get; set; } = null!;
    public GetUserDto? GetUserDto { get; set; }
}