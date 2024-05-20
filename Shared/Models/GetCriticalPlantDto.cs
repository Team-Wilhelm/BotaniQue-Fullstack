using Shared.Models.Information;

namespace Shared.Models;

public class GetCriticalPlantDto
{
    public required Guid PlantId { get; set; }
    public required string Nickname { get; set; }
    public required string ImageUrl { get; set; }
    public required int Mood { get; set; }
    public string? SuggestedAction { get; set; }
    public required Requirements Requirements { get; set; }
    
    public static GetCriticalPlantDto FromPlant(Plant plant)
    {
        var conditionLog = plant.ConditionsLogs.FirstOrDefault();
        var mood = conditionLog?.Mood ?? 0;
        
        return new GetCriticalPlantDto
        {
            PlantId = plant.PlantId,
            Nickname = plant.Nickname,
            ImageUrl = plant.ImageUrl,
            Mood = mood,
            Requirements = plant.Requirements!
        };
    }
}