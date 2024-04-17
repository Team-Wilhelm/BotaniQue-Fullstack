namespace Shared.Models.Information;

public abstract class Conditions
{
    public Guid ConditionsId { get; set; }
    public Guid PlantId { get; set; }
    public int SoilMoisture { get; set; }
    public int LightLevel { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
}