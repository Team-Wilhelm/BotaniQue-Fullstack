using Infrastructure.Repositories;
using Shared.Dtos.FromClient.Requirements;
using Shared.Exceptions;
using Shared.Models.Information;

namespace api.Core.Services;

public class RequirementService(RequirementsRepository requirementsRepository)
{
    public async Task<Requirements> GetRequirements(Guid plantId)
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
    
    public async Task<Requirements?> UpdateRequirements(UpdateRequirementDto updateRequirementDto, Guid plantId)
    {
        var requirements = await requirementsRepository.GetRequirements(updateRequirementDto.RequirementsId);
        if (requirements is null) throw new NotFoundException("Requirements not found");
        if (requirements.PlantId != plantId) throw new NoAccessException("You don't have access to this plant");
        
        requirements = new Requirements(updateRequirementDto, plantId);
        return await requirementsRepository.UpdateRequirements(requirements);
    }
    
    public async Task DeleteRequirements(Guid plantId)
    {
        var requirements = await requirementsRepository.GetRequirements(plantId);
        if (requirements is null) throw new NotFoundException("Requirements not found");
        
        await requirementsRepository.DeleteRequirements(requirements);
    }
}