using api.Core.Services;
using api.Events.PlantEvents.Server;
using api.Events.Statistics;
using api.Extensions;
using Fleck;
using lib;
using Shared.Models;

namespace api.Events.PlantEvents.Client;

public class ClientWantsToGetCriticalPlantsDto : BaseDtoWithJwt;

public class ClientWantsToGetCriticalPlants(JwtService jwtService, PlantService plantService, StatsService statsService) : BaseEventHandler<ClientWantsToGetCriticalPlantsDto>
{
    public override async Task Handle(ClientWantsToGetCriticalPlantsDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt!);
        var plants = await plantService.GetCriticalPlants(email);
        var stats = await statsService.GetStats(email);
        
        var serverResponse = new ServerSendsCriticalPlants
        {
            Plants = plants
        };
        socket.SendDto(serverResponse);
        
        socket.SendDto(new ServerSendsStats{Stats = stats});
    }
}