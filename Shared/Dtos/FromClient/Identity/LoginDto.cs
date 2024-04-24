using System.ComponentModel.DataAnnotations;

namespace Shared.Dtos.FromClient.Identity;

public class LoginDto
{
    [EmailAddress] public string Email { get; set; }

    public string Password { get; set; }
}