using api.Core.Services;
using api.Events.PlantEvents.Server;
using api.Events.Statistics;
using api.Extensions;
using Fleck;
using lib;
using Shared.Dtos.FromClient.Plant;
using Shared.Models;

namespace api.Events.PlantEvents.Client;

public class ClientWantsToUpdatePlantDto: BaseDtoWithJwt
{
    public required UpdatePlantDto UpdatePlantDto { get; set; }
}

public class ClientWantsToUpdatePlant(PlantService plantService, JwtService jwtService, StatsService statsService): BaseEventHandler<ClientWantsToUpdatePlantDto>
{
    public override async Task Handle(ClientWantsToUpdatePlantDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt!);
        
        var plant = await plantService.UpdatePlant(dto.UpdatePlantDto, email);
        socket.SendDto(new ServerSavesPlant
        {
            Plant = plant
        });
        
        var allPlants = await plantService.GetPlantsForUser(email, 1, 100);
        socket.SendDto(new ServerSendsPlants
        {
            Plants = allPlants
        });
        
        var stats = await statsService.GetStats(email);
        socket.SendDto(new ServerSendsStats{Stats = stats});
    }
}