namespace Shared.Models.Information;

public class ConditionsLog
{
    public required Guid ConditionsId { get; set; }
    public required Guid PlantId { get; set; }
    public required DateTime TimeStamp { get; set; }
    public required int Mood { get; set; }
    public required double SoilMoisture { get; set; }
    public required double Light { get; set; }
    public required double Temperature { get; set; }
    public required double Humidity { get; set; }
}