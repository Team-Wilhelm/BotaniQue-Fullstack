using api.Core.Services;
using api.EventFilters;
using api.Events.PlantEvents.Server;
using api.Events.Statistics;
using api.Extensions;
using Fleck;
using lib;
using Shared.Dtos.FromClient.Plant;
using Shared.Models;

namespace api.Events.PlantEvents.Client;

public class ClientWantsToCreatePlantDto: BaseDtoWithJwt
{
    public required CreatePlantDto CreatePlantDto { get; set; }
}

[ValidateDataAnnotations]
public class ClientWantsToCreatePlant(PlantService plantService, JwtService jwtService, StatsService statsService): BaseEventHandler<ClientWantsToCreatePlantDto>
{
    public override async Task Handle(ClientWantsToCreatePlantDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt!);
        var plant = await plantService.CreatePlant(dto.CreatePlantDto, email);
        
        var serverCreatesNewPlant = new ServerSavesPlant
        {
            Plant = plant
        };
        socket.SendDto(serverCreatesNewPlant);
        
       var stats = await statsService.GetStats(email);
       socket.SendDto(new ServerSendsStats{Stats = stats});
    }
}