using Shared.Models.Information;

namespace Shared.Models;

public class Plant
{
    public Guid PlantId { get; set; }
    public string? DeviceId { get; set; }
    public string UserEmail { get; set; } = null!;
    public Guid? CollectionId { get; set; }
    public string? Nickname { get; set; } // if not provided make one up
    public string ImageUrl { get; set; } = null!;
    public Requirements? Requirements { get; set; }
    public List<ConditionsLog> ConditionsLogs { get; set; } = new();
}