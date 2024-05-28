using Shared.Dtos;
using Shared.Models;

namespace api.Core.Services;

public class StatsService(CollectionsService collectionsService, PlantService plantService)
{
    public async Task<Stats> GetStats(string email)
    {
        var totalPlants = await plantService.GetTotalPlantsCount(email);
        var happyPlants = await plantService.GetHappyPlantsCount(email);
        var collections = await collectionsService.GetTotalCollectionsCount(email);

        return new Stats
        {
            TotalPlants = totalPlants,
            HappyPlants = happyPlants,
            Collections = collections
        };
    }
}
