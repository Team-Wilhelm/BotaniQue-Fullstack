using System.ComponentModel.DataAnnotations;

namespace Shared.Dtos.FromClient.Identity;

public class RegisterUserDto
{
    [EmailAddress] public required string Email { get; set; }
    [MaxLength(50)] public required string Username { get; set; }
    [MinLength(8)] [MaxLength(256)] public required string Password { get; set; }
    public string? Base64Image { get; set; }
    public string? BlobUrl { get; set; }
}