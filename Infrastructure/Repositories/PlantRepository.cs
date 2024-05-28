using Microsoft.EntityFrameworkCore;
using Shared.Dtos.FromClient.Plant;
using Shared.Exceptions;
using Shared.Models;
using Shared.Models.Information;

namespace Infrastructure.Repositories;

public class PlantRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
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
            .Include(plant => plant.ConditionsLogs.OrderByDescending(log => log.TimeStamp).Take(1))
            .FirstOrDefaultAsync(p => p.PlantId == id);

        return plant;
    }

    public async Task<List<Plant>> GetPlantsForUser(string userEmail, int pageNumber, int pageSize)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Plants
            .Include(plant => plant.Requirements)
            .Include(plant => plant.ConditionsLogs.OrderByDescending(log => log.TimeStamp).Take(1))
            .Where(p => p.UserEmail == userEmail)
            .OrderByDescending(p => p.LatestChange)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Plant>> GetPlantsForCollection(Guid collectionId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Plants
            .Include(plant => plant.Requirements)
            .Include(plant => plant.ConditionsLogs.OrderByDescending(log => log.TimeStamp).Take(1))
            .Where(p => p.CollectionId == collectionId)
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

    public async Task<Guid> GetPlantIdByDeviceIdAsync(string deviceId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var plant = await context.Plants
            .FirstOrDefaultAsync(p => p.DeviceId == deviceId);

        if (plant is null)
        {
            throw new NotFoundException($"Plant with device id {deviceId} not found");
        }

        return plant.PlantId;
    }

    public async Task<Requirements> GetRequirementsForPlant(Guid plantId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var plant = await context.Plants
            .Include(plant => plant.Requirements)
            .FirstOrDefaultAsync(p => p.PlantId == plantId);

        if (plant?.Requirements is null)
        {
            throw new NotFoundException($"Requirements for plant: {plantId} not found");
        }

        return plant.Requirements;
    }

    public async Task<List<Plant>> GetCriticalPlants(string requesterEmail)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Plants
            .Include(plant => plant.Requirements)
            .Include(plant => plant.ConditionsLogs.OrderByDescending(log => log.TimeStamp).Take(1))
            .Where(p => p.UserEmail == requesterEmail && p.ConditionsLogs.Count != 0)
            .Select(p => new
            {
                Plant = p,
                WorstMood = p.ConditionsLogs
                    .OrderBy(log => log.Mood)
                    .ThenByDescending(log => log.TimeStamp)
                    .FirstOrDefault()
            })
            .OrderBy(p => p.WorstMood.Mood)
            .ThenByDescending(p => p.WorstMood.TimeStamp)
            .Take(3)
            .Select(p => p.Plant)
            .ToListAsync();
    }
    
    public async Task<Stats> GetStats(string userEmail)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var totalPlants = await context.Plants.CountAsync(p => p.UserEmail == userEmail);
        var happyPlants = await context.Plants
            .Include(plant => plant.ConditionsLogs)
            .Where(p => p.UserEmail == userEmail && p.ConditionsLogs.Count != 0)
            .CountAsync(p => p.ConditionsLogs.OrderByDescending(log => log.TimeStamp).FirstOrDefault()!.Mood > 2);
        var collections = await context.Collections.CountAsync(c => c.UserEmail == userEmail);

        return new Stats
        {
            TotalPlants = totalPlants,
            HappyPlants = happyPlants,
            Collections = collections
        };
    }
}