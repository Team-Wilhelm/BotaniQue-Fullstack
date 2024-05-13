﻿using api.EventFilters;
using api.Events.PlantEvents.Server;
using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Shared.Dtos.FromClient.Plant;
using Shared.Models;

namespace api.Events.PlantEvents.Client;

public class ClientWantsToCreatePlantDto: BaseDtoWithJwt
{
    public CreatePlantDto CreatePlantDto { get; set; }
}

[ValidateDataAnnotations]
public class ClientWantsToCreatePlant(PlantService plantService, JwtService jwtService): BaseEventHandler<ClientWantsToCreatePlantDto>
{
    public override async Task Handle(ClientWantsToCreatePlantDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt!);
        var plant = await plantService.CreatePlant(dto.CreatePlantDto, email);
        
        var serverCreatesNewPlant = new ServerSavesPlant
        {
            Plant = plant
        };
        
        socket.SendDto(serverCreatesNewPlant);
    }
}