using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Shared.Models.Information;

namespace Shared.Dtos.Plant;

public class CreatePlantDto
{
    [EmailAddress] public string UserEmail { get; set; } = null!;
    public Guid? CollectionId { get; set; }
    [MaxLength(50)] public string? Nickname { get; set; }
    public string ImageUrl { get; set; } = null!;
    public Requirements Requirements { get; set; } = new();
}