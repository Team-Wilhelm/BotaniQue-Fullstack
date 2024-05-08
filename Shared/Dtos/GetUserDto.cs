using lib;

namespace Shared.Dtos;

public class GetUserDto : BaseDto
{
    public string UserEmail { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string? BlobUrl { get; set; }
}