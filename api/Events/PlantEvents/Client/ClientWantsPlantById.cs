﻿using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using api.EventFilters;
using api.Events.PlantEvents.Server;
using api.Extensions;
using Core.Services;
using Fleck;
using lib;

namespace api.Events.PlantEvents.Client;

public class ClientWantsPlantByIdDto: BaseDto
{
    [Required] public Guid PlantId { get; set; }
}

[ValidateDataAnnotations]
public class ClientWantsPlantById(PlantService plantService): BaseEventHandler<ClientWantsPlantByIdDto>
{
    public override async Task Handle(ClientWantsPlantByIdDto dto, IWebSocketConnection socket)
    {
        var plant = await plantService.GetPlantById(dto.PlantId);
        socket.SendDto(new ServerSendsPlant
        {
            Plant = plant
        });
    }
}