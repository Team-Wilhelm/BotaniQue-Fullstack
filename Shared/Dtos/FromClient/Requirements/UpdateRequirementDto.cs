using Shared.Models.Information;

namespace Shared.Dtos.FromClient.Requirements;

public class UpdateRequirementDto
{
    public Guid ConditionsId { get; set; }
    public RequirementLevel SoilMoisture { get; set; }
    public RequirementLevel LightLevel { get; set; }
    public RequirementLevel Temperature { get; set; }
    public RequirementLevel Humidity { get; set; }
}