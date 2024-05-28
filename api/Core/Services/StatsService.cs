using Shared.Dtos;
using Shared.Models;

namespace api.Core.Services;

public class StatsService(CollectionsService collectionsService, PlantService plantService)
{
    public async Task<Stats> GetStats(string email)
    {
        var totalPlantsTask = plantService.GetTotalPlantsCount(email);
        var happyPlantsTask = plantService.GetHappyPlantsCount(email);
        var collectionsTask = collectionsService.GetTotalCollectionsCount(email);

        await Task.WhenAll(totalPlantsTask, happyPlantsTask, collectionsTask);

        return new Stats
        {
            TotalPlants = totalPlantsTask.Result,
            HappyPlants = happyPlantsTask.Result,
            Collections = collectionsTask.Result
        };
    }
}
