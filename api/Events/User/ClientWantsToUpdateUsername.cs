﻿using api.Core.Services;
using api.Extensions;
using Fleck;
using lib;
using Shared.Exceptions;
using Shared.Models;

namespace api.Events.User;

public class ClientWantsToUpdateUsernameDto : BaseDtoWithJwt
{
    public string Username { get; set; } = null!;
}

public class ClientWantsToUpdateUsername(UserService userService, JwtService jwtService) : BaseEventHandler<ClientWantsToUpdateUsernameDto>
{
    public override async Task Handle(ClientWantsToUpdateUsernameDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt);
        try
        {
            var username = await userService.UpdateUsername(email, dto.Username);
            socket.SendDto(new ServerConfirmsUpdateUsername
            {
                Username = username
            });
        }
        catch (Exception e) when (e is not NotFoundException)
        {
            socket.SendDto(new ServerRejectsUpdate
            {
                Error = "Update username failed"
            });
        }
    }
}