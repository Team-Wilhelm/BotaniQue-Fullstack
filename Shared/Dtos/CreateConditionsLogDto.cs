namespace Shared.Dtos;

public class CreateConditionsLogDto
{
    public DateTime TimeStamp { get; set; }
    public double SoilMoisturePercentage { get; set; }
    public double LightLevel { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public long DeviceId { get; set; }
}