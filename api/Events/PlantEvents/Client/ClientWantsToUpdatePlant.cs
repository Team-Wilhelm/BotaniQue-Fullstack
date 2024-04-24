using api.Events.PlantEvents.Server;
using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Shared.Dtos.Plant;

namespace api.Events.PlantEvents.Client;

public class ClientWantsToUpdatePlantDto: BaseDto
{
    public UpdatePlantDto UpdatePlantDto { get; set; }
}

public class ClientWantsToUpdatePlant(PlantService plantService): BaseEventHandler<ClientWantsToUpdatePlantDto>
{
    public override async Task Handle(ClientWantsToUpdatePlantDto dto, IWebSocketConnection socket)
    {
        var plant = await plantService.UpdatePlant(dto.UpdatePlantDto);
        socket.SendDto(new ServerSendsPlant
        {
            Plant = plant
        });
    }
}