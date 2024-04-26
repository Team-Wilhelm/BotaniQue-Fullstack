using lib;

namespace api.Events.ImageUpload;

public class ServerSendsImageWithoutBackground : BaseDto
{
    public byte[] Image { get; set; } = null!;
}