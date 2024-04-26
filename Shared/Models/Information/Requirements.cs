using Shared.Dtos.FromClient.Requirements;

namespace Shared.Models.Information;

public class Requirements : Conditions
{
    public Requirements()
    {
    }
    
    public Requirements(CreateRequirementsDto createRequirementsDto)
    {
        ConditionsId = Guid.NewGuid();
        PlantId = createRequirementsDto.PlantId!.Value; // plantID should be assigned to the dto before using this constructor
        SoilMoistureLevel = createRequirementsDto.SoilMoistureLevel;
        LightLevel = createRequirementsDto.LightLevel;
        TemperatureLevel = createRequirementsDto.TemperatureLevel;
        HumidityLevel = createRequirementsDto.HumidityLevel;
    }
    
    public Requirements(UpdateRequirementDto updateRequirementDto, Guid plantId)
    {
        ConditionsId = updateRequirementDto.ConditionsId;
        PlantId = plantId;
        SoilMoistureLevel = updateRequirementDto.SoilMoistureLevel;
        LightLevel = updateRequirementDto.LightLevel;
        TemperatureLevel = updateRequirementDto.TemperatureLevel;
        HumidityLevel = updateRequirementDto.HumidityLevel;
    }
}