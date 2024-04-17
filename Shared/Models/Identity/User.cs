using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Identity;

public class User
{
    public string UserEmail { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    
    public List<Collection> Collections { get; set; } = new();
    public List<Plant> Plants { get; set; } = new();
}