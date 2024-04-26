namespace Shared.Models.Information;

public class ConditionsLog
{
    public Guid ConditionsId { get; set; }
    public Guid PlantId { get; set; }
    public DateTime TimeStamp { get; set; }
    public int Mood { get; set; }
    public double SoilMoisture { get; set; }
    public double Light { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
}