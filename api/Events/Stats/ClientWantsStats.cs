using api.Core.Services;
using api.Extensions;
using Fleck;
using lib;
using Shared.Models;

namespace api.Events.Stats;

public class ClientWantsStatsDto : BaseDtoWithJwt
{
    
}

public class ClientWantsStats(PlantService plantService, CollectionsService collectionsService, JwtService jwtService) : BaseEventHandler<ClientWantsStatsDto>
{
    public override async Task Handle(ClientWantsStatsDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt!);
        
        var totalPlants = await plantService.GetTotalPlantsCount(email);
        var happyPlants = await plantService.GetHappyPlantsCount(email);
        var collections = await collectionsService.GetTotalCollectionsCount(email);
        
        var statsDto = new ServerSendsStats
        {
            Stats = new Stats
            {
                TotalPlants = totalPlants,
                HappyPlants = happyPlants,
                Collections = collections
            }
        };
        
        socket.SendDto(statsDto);
    }
}

public class ServerSendsStats : BaseDto
{
    public Stats Stats { get; set; }
}

public class Stats
{
    public int TotalPlants { get; set; }
    public int HappyPlants { get; set; }
    public int Collections { get; set; }
}