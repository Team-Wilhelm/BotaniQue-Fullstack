namespace Shared.Dtos.FromClient;

public class UpdateUserDto
{
    public string UserEmail { get; set; } = null!;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Base64Image { get; set; }
}