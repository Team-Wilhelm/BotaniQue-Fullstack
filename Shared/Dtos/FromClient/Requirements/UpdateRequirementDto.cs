using Shared.Models.Information;

namespace Shared.Dtos.FromClient.Requirements;

public class UpdateRequirementDto
{
    public Guid ConditionsId { get; set; }
    public RequirementLevel SoilMoistureLevel { get; set; }
    public RequirementLevel LightLevel { get; set; }
    public RequirementLevel TemperatureLevel { get; set; }
    public RequirementLevel HumidityLevel { get; set; }
}