using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace Infrastructure.Repositories;

public class CollectionsRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
{
    public async Task<IEnumerable<Collection>> GetCollectionsForUser(string userEmail)
    {
        await using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
        return await applicationDbContext.Collections
            .Where(collection => collection.UserEmail == userEmail)
            .ToListAsync();
    }
    
    public async Task<Collection?> GetCollectionWithoutPlants(Guid collectionId)
    {
        await using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
        return await applicationDbContext.Collections
            .FirstOrDefaultAsync(collection => collection.CollectionId == collectionId);
    }
    
    public async Task<Collection> CreateCollection(Collection collection)
    {
        await using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
        var createdCollection = (await applicationDbContext.Collections.AddAsync(collection)).Entity;
        await applicationDbContext.SaveChangesAsync();
        return createdCollection;
    }
    
    public async Task<Collection> UpdateCollection(Collection collection)
    {
        await using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
        var updatedCollection = applicationDbContext.Collections.Update(collection).Entity;
        await applicationDbContext.SaveChangesAsync();
        return updatedCollection;
    }
    
    public async Task DeleteCollection(Collection collection)
    {
        await using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
        
        // Remove the collection reference from plants
        var plantsToUpdate = await applicationDbContext.Plants
            .Where(p => p.CollectionId == collection.CollectionId)
            .ToListAsync();

        foreach (var plant in plantsToUpdate)
        {
            plant.CollectionId = null;
        }
        applicationDbContext.Plants.UpdateRange(plantsToUpdate);
        applicationDbContext.Collections.Remove(collection);
        await applicationDbContext.SaveChangesAsync();
    }
    
    public async Task AddPlantToCollection(Collection collection, Plant plant)
    {
        await using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
        collection.Plants.Add(plant);
        await applicationDbContext.SaveChangesAsync();
    }
    
    public async Task RemovePlantFromCollection(Collection collection, Plant plant)
    {        
        await using var applicationDbContext = await dbContextFactory.CreateDbContextAsync();
        collection.Plants.Remove(plant);
        await applicationDbContext.SaveChangesAsync();
    }
}