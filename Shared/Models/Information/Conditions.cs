namespace Shared.Models.Information;

public abstract class Conditions
{
    public Guid ConditionsId { get; set; }
    public Guid PlantId { get; set; }
    public RequirementLevel SoilMoistureLevel { get; set; }
    public RequirementLevel LightLevel { get; set; }
    public RequirementLevel TemperatureLevel { get; set; }
    public RequirementLevel HumidityLevel { get; set; }
}