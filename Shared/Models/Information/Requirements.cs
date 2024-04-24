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
        SoilMoisture = createRequirementsDto.SoilMoisture;
        LightLevel = createRequirementsDto.LightLevel;
        Temperature = createRequirementsDto.Temperature;
        Humidity = createRequirementsDto.Humidity;
    }
    
    public Requirements(UpdateRequirementDto updateRequirementDto)
    {
        ConditionsId = updateRequirementDto.ConditionsId;
        PlantId = updateRequirementDto.PlantId;
        SoilMoisture = updateRequirementDto.SoilMoisture;
        LightLevel = updateRequirementDto.LightLevel;
        Temperature = updateRequirementDto.Temperature;
        Humidity = updateRequirementDto.Humidity;
    }
}