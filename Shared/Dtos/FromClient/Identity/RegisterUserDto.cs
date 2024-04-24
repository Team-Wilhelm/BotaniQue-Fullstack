using System.ComponentModel.DataAnnotations;

namespace Shared.Dtos.FromClient;

public class RegisterUserDto
{
    [EmailAddress] public string Email { get; set; }

    [MaxLength(50)] public string Username { get; set; }

    [MinLength(8)] [MaxLength(256)] public string Password { get; set; }
}