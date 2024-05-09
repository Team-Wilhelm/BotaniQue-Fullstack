namespace Shared.Models;

public class Collection
{
    public required Guid CollectionId { get; set; }
    public required string UserEmail { get; set; }
    public required string Name { get; set; } = null!;
    public List<Plant> Plants { get; set; } = [];
}