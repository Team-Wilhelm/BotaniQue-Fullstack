using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace Infrastructure.Repositories;

public class CollectionsRepository(ApplicationDbContext applicationDbContext)
{
    public async Task<IEnumerable<Collection>> GetCollectionsForUser(string userEmail)
    {
        return await applicationDbContext.Collections
            .Where(collection => collection.UserEmail == userEmail)
            .ToListAsync();
    }
    
    public async Task<Collection?> GetCollection(Guid collectionId)
    {
        return await applicationDbContext.Collections
            .Include(collection => collection.Plants)
            .FirstOrDefaultAsync(collection => collection.CollectionId == collectionId);
    }
    
    public async Task<Collection> CreateCollection(Collection collection)
    {
        var createdCollection = (await applicationDbContext.Collections.AddAsync(collection)).Entity;
        await applicationDbContext.SaveChangesAsync();
        return createdCollection;
    }
    
    public async Task<Collection> UpdateCollection(Collection collection)
    {
        var updatedCollection = applicationDbContext.Collections.Update(collection).Entity;
        await applicationDbContext.SaveChangesAsync();
        return updatedCollection;
    }
    
    public async Task DeleteCollection(Collection collection)
    {
        applicationDbContext.Collections.Remove(collection);
        await applicationDbContext.SaveChangesAsync();
    }
    
    public async Task AddPlantToCollection(Collection collection, Plant plant)
    {
        collection.Plants.Add(plant);
        await applicationDbContext.SaveChangesAsync();
    }
    
    public async Task RemovePlantFromCollection(Collection collection, Plant plant)
    {
        collection.Plants.Remove(plant);
        await applicationDbContext.SaveChangesAsync();
    }
}