﻿using api.EventFilters;
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
    public UserDto UserDto { get; set; } = null!;
}

[ValidateDataAnnotations]
public class ClientWantsToUpdateProfile (UserService userService, JwtService jwtService) : BaseEventHandler<ClientWantsToUpdateUserDto>
{
    public override async Task Handle(ClientWantsToUpdateUserDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt);
        try
        {
            var getUserDto = await userService.UpdateUser(dto.UserDto, email);
            socket.SendDto(new ServerConfirmsUpdate
            {
                GetUserDto = getUserDto
            });
        } catch (Exception e) when (e is not NotFoundException)
        {
            var user = await userService.GetUserByEmail(email);
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