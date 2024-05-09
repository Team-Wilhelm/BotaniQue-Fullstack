namespace Shared.Dtos.FromClient.Collections;

public class UpdateCollectionDto
{
    public Guid CollectionId { get; set; }
    public string Name { get; set; } = null!;
}