using System.ComponentModel.DataAnnotations;
using Shared.Dtos.FromClient.Requirements;

namespace Shared.Models.Information;

public class Requirements
{
    public Guid RequirementsId { get; set; }
    public Guid PlantId { get; set; }
    public RequirementLevel SoilMoistureLevel { get; set; }
    public RequirementLevel LightLevel { get; set; }
    [Range(-20, 45)]
    public double TemperatureLevel { get; set; }
    public RequirementLevel HumidityLevel { get; set; }
   
    public Requirements()
    {
        
    }
    
    public Requirements(CreateRequirementsDto createRequirementsDto)
    {
        RequirementsId = Guid.NewGuid();
        PlantId = createRequirementsDto.PlantId!.Value; // plantID should be assigned to the dto before using this constructor
        SoilMoistureLevel = createRequirementsDto.SoilMoistureLevel;
        LightLevel = createRequirementsDto.LightLevel;
        TemperatureLevel = createRequirementsDto.TemperatureLevel;
        HumidityLevel = createRequirementsDto.HumidityLevel;
    }

    public Requirements(UpdateRequirementDto updateRequirementDto, Guid plantId)
    {
        RequirementsId = updateRequirementDto.RequirementsId;
        PlantId = plantId;
        SoilMoistureLevel = updateRequirementDto.SoilMoistureLevel;
        LightLevel = updateRequirementDto.LightLevel;
        TemperatureLevel = updateRequirementDto.TemperatureLevel;
        HumidityLevel = updateRequirementDto.HumidityLevel;
    }
}