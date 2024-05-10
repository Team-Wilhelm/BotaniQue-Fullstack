using Shared.Models.Information;

namespace Shared.Models;

public class Plant
{
    public required Guid PlantId { get; set; }
    public string? DeviceId { get; set; }
    public required string UserEmail { get; set; }
    public Guid? CollectionId { get; set; }
    public string? Nickname { get; set; } // if not provided make one up
    public required string ImageUrl { get; set; }
    public Requirements? Requirements { get; set; }
    public List<ConditionsLog> ConditionsLogs { get; set; } = new();
}