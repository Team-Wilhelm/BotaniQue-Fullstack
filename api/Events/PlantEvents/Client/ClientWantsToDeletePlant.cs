using System.ComponentModel.DataAnnotations;
using api.EventFilters;
using api.Events.PlantEvents.Server;
using api.Extensions;
using Core.Services;
using Fleck;
using lib;

namespace api.Events.PlantEvents.Client;

public class ClientWantsToDeletePlantDto: BaseDto
{
    [Required] public Guid PlantId { get; set; }
}

[ValidateDataAnnotations]
public class ClientWantsToDeletePlant(PlantService plantService): BaseEventHandler<ClientWantsToDeletePlantDto>
{
    public override async Task Handle(ClientWantsToDeletePlantDto dto, IWebSocketConnection socket)
    {
        await plantService.DeletePlant(dto.PlantId);
        var serverConfirmsDelete = new ServerConfirmsDelete();
        socket.SendDto(serverConfirmsDelete);
    }
}

