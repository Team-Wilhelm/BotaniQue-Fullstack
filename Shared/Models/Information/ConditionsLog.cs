namespace Shared.Models.Information;

public class ConditionsLog : Conditions
{
    public Guid ConditionsLogId { get; set; }
    public DateTime TimeStamp { get; set; }
    public Guid PlantId { get; set; }
    public int Mood { get; set; }
}