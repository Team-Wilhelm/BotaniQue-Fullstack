using api.Events.PlantEvents.Server;
using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Shared.Dtos.FromClient.Plant;
using Shared.Models;

namespace api.Events.PlantEvents.Client;

public class ClientWantsToUpdatePlantDto: BaseDtoWithJwt
{
    public UpdatePlantDto UpdatePlantDto { get; set; }
}

public class ClientWantsToUpdatePlant(PlantService plantService, WebSocketConnectionService connectionService): BaseEventHandler<ClientWantsToUpdatePlantDto>
{
    public override async Task Handle(ClientWantsToUpdatePlantDto dto, IWebSocketConnection socket)
    {
        var user = connectionService.GetUser(socket);
        var plant = await plantService.UpdatePlant(dto.UpdatePlantDto, user.UserEmail);
        socket.SendDto(new ServerSendsPlant
        {
            Plant = plant
        });
    }
}