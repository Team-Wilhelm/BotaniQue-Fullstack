using System.ComponentModel.DataAnnotations;
using Shared.Dtos.FromClient.Requirements;

namespace Shared.Dtos.FromClient.Plant;

public class CreatePlantDto
{
    public Guid? CollectionId { get; set; }
    public string? DeviceId { get; set; }
    [MaxLength(50)] public string? Nickname { get; set; }
    public string? Base64Image { get; set; }
    public CreateRequirementsDto CreateRequirementsDto { get; set; } = null!;
}