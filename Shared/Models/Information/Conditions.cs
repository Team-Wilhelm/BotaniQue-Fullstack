namespace Shared.Models.Information;

public abstract class Conditions
{
    public Guid ConditionsId { get; set; }
    public Guid PlantId { get; set; }
    public RequirementLevel SoilMoisture { get; set; }
    public RequirementLevel LightLevel { get; set; }
    public RequirementLevel Temperature { get; set; }
    public RequirementLevel Humidity { get; set; }
}