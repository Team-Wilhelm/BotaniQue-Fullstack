﻿using api.EventFilters;
using api.Events.PlantEvents.Server;
using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Shared.Dtos.Plant;
using Shared.Models;

namespace api.Events.PlantEvents.Client;

public class ClientWantsToCreatePlantDto: BaseDto
{
    public CreatePlantDto CreatePlantDto { get; set; }
}

[ValidateDataAnnotations]
public class ClientWantsToCreatePlant(PlantService plantService): BaseEventHandler<ClientWantsToCreatePlantDto>
{
    public override async Task Handle(ClientWantsToCreatePlantDto dto, IWebSocketConnection socket)
    {
        var createPlantDto = dto.CreatePlantDto;
        var plant = await plantService.CreatePlant(createPlantDto);
        var serverSendsPlant = new ServerSendsPlant
        {
            Plant = plant
        };
        socket.SendDto(serverSendsPlant);
    }
}