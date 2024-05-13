using api.Events.PlantEvents.Server;
using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Shared.Models;

namespace api.Events.PlantEvents.Client;

public class ClientWantsToGetCriticalPlantsDto : BaseDtoWithJwt;

public class ClientWantsToGetCriticalPlants(JwtService jwtService, PlantService plantService) : BaseEventHandler<ClientWantsToGetCriticalPlantsDto>
{
    public override async Task Handle(ClientWantsToGetCriticalPlantsDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt!);
        var plants = await plantService.GetCriticalPlants(email);
        var serverResponse = new ServerSendsCriticalPlants
        {
            Plants = plants
        };
        socket.SendDto(serverResponse);
    }
}