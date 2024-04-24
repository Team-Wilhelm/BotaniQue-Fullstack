using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Plant;
using Shared.Models;
using Shared.Models.Exceptions;

namespace Infrastructure.Repositories;

public class PlantRepository (IDbContextFactory<ApplicationDbContext> dbContextFactory)
{
    public async Task<Plant?> CreatePlant(CreatePlantDto createPlantDto)
    {
        var plant = new Plant
        {
            PlantId = Guid.NewGuid(),
            UserEmail = createPlantDto.UserEmail,
            CollectionId = createPlantDto.CollectionId,
            Nickname = createPlantDto.Nickname,
            ImageUrl = createPlantDto.ImageUrl,
            Requirements = createPlantDto.Requirements
        };
        await using var context = await dbContextFactory.CreateDbContextAsync();
        await context.Plants.AddAsync(plant);
        await context.SaveChangesAsync();
        return plant;
    }

    public async Task<Plant?> GetPlantById(Guid id)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Plants.FirstOrDefaultAsync(p => p.PlantId == id);
    }
    
    public async Task<List<Plant>?> GetPlantsForUser(string userEmail, int pageNumber, int pageSize)
    {
        //TODO should we keep track of logged in user instead of passing in email?
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Plants
            .Where(p => p.UserEmail == userEmail)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<Plant> UpdatePlant(UpdatePlantDto updatePlantDto)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var plant = await context.Plants.FirstOrDefaultAsync(p => p.PlantId == updatePlantDto.PlantId);
        if (plant == null) throw new NotFoundException("Plant not found");

        if (updatePlantDto.CollectionId.HasValue && updatePlantDto.CollectionId != plant.CollectionId)
        {
            plant.CollectionId = updatePlantDto.CollectionId;
        }

        if (!string.IsNullOrEmpty(updatePlantDto.Nickname) && updatePlantDto.Nickname != plant.Nickname)
        {
            plant.Nickname = updatePlantDto.Nickname;
        }
        
        if (!string.IsNullOrEmpty(updatePlantDto.ImageUrl) && updatePlantDto.ImageUrl != plant.ImageUrl)
        {
            plant.ImageUrl = updatePlantDto.ImageUrl;
        }
        
        //TODO change this when we implement requirements and apply correct logic
        plant.Requirements = updatePlantDto.Requirements;
        
        context.Plants.Update(plant);
        await context.SaveChangesAsync();
        return plant;
    }
    
    public async Task DeletePlant(Guid id)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var plant = await context.Plants.FirstOrDefaultAsync(p => p.PlantId == id);
        if (plant == null) return;
        context.Plants.Remove(plant);
        await context.SaveChangesAsync();
    }
}