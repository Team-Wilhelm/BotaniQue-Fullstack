using Shared.Models.Information;

namespace Shared.Dtos.FromClient.Requirements;

public class CreateRequirementsDto
{
    public Guid? PlantId { get; set; } // is not sent with the request, but assigned when creating a plant
    public RequirementLevel SoilMoisture { get; set; }
    public RequirementLevel LightLevel { get; set; }
    public RequirementLevel Temperature { get; set; }
    public RequirementLevel Humidity { get; set; }
}