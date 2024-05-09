using Infrastructure.Repositories;
using Shared.Dtos.FromClient.Collections;
using Shared.Exceptions;
using Shared.Models;

namespace Core.Services;

public class CollectionsService(CollectionsRepository collectionsRepository, PlantService plantService)
{
    public async Task<IEnumerable<Collection>> GetCollectionsForUser(string userEmail)
    {
        return await collectionsRepository.GetCollectionsForUser(userEmail);
    }
    
    public async Task<Collection> GetCollection(Guid collectionId, string userEmail)
    {
        return await VerifyCollectionExistsAndUserHasAccess(collectionId, userEmail);
    }
    
    public async Task<Collection> CreateCollection(CreateCollectionDto createCollectionDto, string loggedInUser)
    {
        var collection = new Collection
        {
            CollectionId = Guid.NewGuid(),
            UserEmail = loggedInUser,
            Name = createCollectionDto.Name
        };
        return await collectionsRepository.CreateCollection(collection);
    }
    
    public async Task<Collection> UpdateCollection(UpdateCollectionDto updateCollectionDto, string loggedInUser)
    {
        var collection = await VerifyCollectionExistsAndUserHasAccess(updateCollectionDto.CollectionId, loggedInUser);
        collection.Name = updateCollectionDto.Name;
        return await collectionsRepository.UpdateCollection(collection);
    }
    
    public async Task DeleteCollection(Guid collectionId, string loggedInUser)
    {
        var collection = await VerifyCollectionExistsAndUserHasAccess(collectionId, loggedInUser);
        await collectionsRepository.DeleteCollection(collection);
    }
    
    public async Task AddPlantToCollection(Guid collectionId, Guid plantId, string loggedInUser)
    {
        var collection = await VerifyCollectionExistsAndUserHasAccess(collectionId, loggedInUser);
        var plant = await plantService.GetPlantById(plantId, loggedInUser);
        await collectionsRepository.AddPlantToCollection(collection, plant);
    }
    
    public async Task RemovePlantFromCollection(Guid collectionId, Guid plantId, string loggedInUser)
    {
        var collection = await VerifyCollectionExistsAndUserHasAccess(collectionId, loggedInUser);
        var plant = await plantService.GetPlantById(plantId, loggedInUser);
        await collectionsRepository.RemovePlantFromCollection(collection, plant);
    }
    
    private async Task<Collection> VerifyCollectionExistsAndUserHasAccess(Guid collectionId, string userEmail)
    {
        var collection = await collectionsRepository.GetCollection(collectionId);
        if (collection is null) throw new NotFoundException("Collection not found");
        if (collection.UserEmail != userEmail) throw new NoAccessException("You don't have access to this collection");
        return collection;
    }
}