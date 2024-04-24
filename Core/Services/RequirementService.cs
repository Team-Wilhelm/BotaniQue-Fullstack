using Infrastructure.Repositories;
using Shared.Dtos.FromClient.Requirements;
using Shared.Models.Exceptions;
using Shared.Models.Information;

namespace Core.Services;

public class RequirementService(RequirementsRepository requirementsRepository)
{
    public async Task<Requirements?> GetRequirements(Guid plantId)
    {
        var requirements = await requirementsRepository.GetRequirements(plantId); 
        if (requirements is null) throw new NotFoundException("Requirements not found");
        return requirements;
    }
    
    public async Task<Requirements> CreateRequirements(CreateRequirementsDto createRequirementsDto)
    {
        var requirements = new Requirements(createRequirementsDto);
        return await requirementsRepository.CreateRequirements(requirements);
    }
    
    public async Task<Requirements?> UpdateRequirements(UpdateRequirementDto updateRequirementDto)
    {
        var requirements = await requirementsRepository.GetRequirements(updateRequirementDto.PlantId);
        if (requirements is null) throw new NotFoundException("Requirements not found");
        
        requirements = new Requirements(updateRequirementDto);
        return await requirementsRepository.UpdateRequirements(requirements);
    }
    
    public async Task DeleteRequirements(Guid plantId)
    {
        var requirements = await requirementsRepository.GetRequirements(plantId);
        if (requirements is null) throw new NotFoundException("Requirements not found");
        
        await requirementsRepository.DeleteRequirements(requirements);
    }
}