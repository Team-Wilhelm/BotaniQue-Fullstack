using Microsoft.EntityFrameworkCore;
using Shared.Dtos.FromClient.Plant;
using Shared.Dtos.Plant;
using Shared.Models;
using Shared.Models.Exceptions;
using Shared.Models.Information;

namespace Infrastructure.Repositories;

public class PlantRepository (IDbContextFactory<ApplicationDbContext> dbContextFactory)
{
    public async Task CreatePlant(Plant plant)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        await context.Plants.AddAsync(plant);
        await context.SaveChangesAsync();
    }

    public async Task<Plant?> GetPlantById(Guid id)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var plant = await context.Plants
            .Include(plant => plant.Requirements)
            .FirstOrDefaultAsync(p => p.PlantId == id);
        
        return plant;
    }
    
    public async Task<List<Plant>> GetPlantsForUser(string userEmail, int pageNumber, int pageSize)
    {
        //TODO should we keep track of logged in user instead of passing in email?
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Plants
            .Include(plant => plant.Requirements)
            .Where(p => p.UserEmail == userEmail)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<Plant> UpdatePlant(Plant plant)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        context.Plants.Update(plant);
        await context.SaveChangesAsync();
        return plant;
    }
    
    public async Task DeletePlant(Plant plant)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        context.Plants.Remove(plant);
        await context.SaveChangesAsync();
    }
}