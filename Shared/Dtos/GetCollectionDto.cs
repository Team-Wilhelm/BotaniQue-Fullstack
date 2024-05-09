namespace Shared.Dtos;

public class GetCollectionDto
{
    public required Guid CollectionId { get; set; }
    public required string Name { get; set; }
}