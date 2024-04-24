using System.ComponentModel.DataAnnotations;
using Shared.Dtos.FromClient.Requirements;

namespace Shared.Dtos.FromClient.Plant;

public class CreatePlantDto
{
    [EmailAddress] public string UserEmail { get; set; } = null!;
    public Guid? CollectionId { get; set; }
    [MaxLength(50)] public string? Nickname { get; set; }
    public string? ImageUrl { get; set; }
    public CreateRequirementsDto CreateRequirementsDto { get; set; } = null!;
}