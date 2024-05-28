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
        var stats = await statsService.GetStats(email);
        var allPlants = await plantService.GetPlantsForUser(email, 1, 100);
        
        socket.SendDto(new ServerSavesPlant
        {
            Plant = plant
        });
        
        socket.SendDto(new ServerSendsPlants
        {
            Plants = allPlants
        });
        
        socket.SendDto(new ServerSendsStats{Stats = stats});
    }
}