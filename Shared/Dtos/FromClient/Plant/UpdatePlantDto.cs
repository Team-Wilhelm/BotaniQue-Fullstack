using System.ComponentModel.DataAnnotations;
using Shared.Dtos.FromClient.Requirements;

namespace Shared.Dtos.FromClient.Plant;

public class UpdatePlantDto
{
    [Required] public Guid PlantId { get; set; }
    public Guid? CollectionId { get; set; }
    public string? DeviceId { get; set; }
    [MaxLength(50)] public string? Nickname { get; set; }
    public string? Base64Image { get; set; } // should be null if the image should not be updated
    public UpdateRequirementDto? UpdateRequirementDto { get; set; } 
}