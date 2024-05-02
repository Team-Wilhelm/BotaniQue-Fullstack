using lib;

namespace api.Events.ImageUpload;

public class ServerSendsImageWithoutBackground : BaseDto
{
    public required string Base64Image { get; set; } = null!;
}