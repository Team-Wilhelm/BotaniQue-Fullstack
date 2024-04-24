using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using api.EventFilters;
using api.Events.PlantEvents.Server;
using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Shared.Models;

namespace api.Events.PlantEvents.Client;

public class ClientWantsPlantByIdDto: BaseDtoWithJwt
{
    [Required] public Guid PlantId { get; set; }
}

[ValidateDataAnnotations]
public class ClientWantsPlantById(PlantService plantService, WebSocketConnectionService connectionService): BaseEventHandler<ClientWantsPlantByIdDto>
{
    public override async Task Handle(ClientWantsPlantByIdDto dto, IWebSocketConnection socket)
    {
        var user = connectionService.GetUser(socket);
        var plant = await plantService.GetPlantById(dto.PlantId, user.UserEmail);
        socket.SendDto(new ServerSendsPlant
        {
            Plant = plant
        });
    }
}