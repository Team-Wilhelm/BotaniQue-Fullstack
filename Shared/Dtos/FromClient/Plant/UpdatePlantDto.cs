using System.ComponentModel.DataAnnotations;
using Shared.Models.Information;

namespace Shared.Dtos.Plant;

public class UpdatePlantDto
{
    [Required] public Guid PlantId { get; set; }
    public Guid? CollectionId { get; set; }

    [MaxLength(50)] public string? Nickname { get; set; }
    public string ImageUrl { get; set; } = null!;
    public Requirements Requirements { get; set; } = new();
}