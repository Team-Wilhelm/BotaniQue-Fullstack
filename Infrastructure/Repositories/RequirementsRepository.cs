using Microsoft.EntityFrameworkCore;
using Shared.Models.Information;

namespace Infrastructure.Repositories;

public class RequirementsRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
{
    public async Task<Requirements?> GetRequirementsForPlant(Guid plantId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Requirements.FirstOrDefaultAsync(r => r.PlantId == plantId);
    }
    
    public async Task<Requirements> CreateRequirements(Requirements requirements)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        context.Requirements.Add(requirements);
        await context.SaveChangesAsync();
        return requirements;
    }
    
    public async Task<Requirements?> UpdateRequirements(Requirements requirements)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        context.Requirements.Update(requirements);
        await context.SaveChangesAsync();
        return requirements;
    }
    
    public async Task DeleteRequirements(Requirements requirements)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        context.Requirements.Remove(requirements);
        await context.SaveChangesAsync();
    }
}