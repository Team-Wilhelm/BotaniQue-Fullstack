using api.Core.Options;
using api.Core.Services.External.BlobStorage;
using Infrastructure.Repositories;
using Microsoft.Extensions.Options;
using Shared.Dtos.FromClient.Plant;
using Shared.Exceptions;
using Shared.Models;
using Shared.Models.Information;

namespace api.Core.Services;

public class PlantService(
    PlantRepository plantRepository,
    RequirementService requirementService,
    IBlobStorageService blobStorageService,
    IOptions<AzureBlobStorageOptions> azureBlobStorageOptions)
{
    public async Task<Plant> CreatePlant(CreatePlantDto createPlantDto, string loggedInUser)
    {
        if (string.IsNullOrEmpty(createPlantDto.Nickname))
        {
            createPlantDto.Nickname = GenerateRandomNickname();
        }

        var ímageUrl = azureBlobStorageOptions.Value.DefaultPlantImageUrl;
        if (createPlantDto.Base64Image is not null)
        {
            ímageUrl = await blobStorageService.SaveImageToBlobStorage(createPlantDto.Base64Image, loggedInUser, true);
        }
        
        // Insert plant first to get the plantId
        var plant = new Plant
        {
            PlantId = Guid.NewGuid(),
            UserEmail = loggedInUser,
            CollectionId = createPlantDto.CollectionId,
            Nickname = createPlantDto.Nickname,
            ImageUrl = ímageUrl,
            DeviceId = createPlantDto.DeviceId,
            LatestChange = DateTime.UtcNow
        };
        
        await plantRepository.CreatePlant(plant);

        // Create requirements for the plant to crete a link between the two
        var requirementsDto = createPlantDto.CreateRequirementsDto;
        requirementsDto.PlantId = plant.PlantId;
        plant.Requirements = await requirementService.CreateRequirements(requirementsDto);
        plant.ImageUrl = blobStorageService.GenerateSasUri(plant.ImageUrl, true);
        return plant;
    }

    public async Task<Plant> GetPlantById(Guid id, string requesterEmail)
    {
        var plant = await VerifyPlantExistsAndUserHasAccess(id, requesterEmail);
        plant.ImageUrl = blobStorageService.GenerateSasUri(plant.ImageUrl, true);
        return plant;
    }
    
    public async Task<List<Plant>> GetPlantsForUser(string userEmail, int pageNumber, int pageSize)
    {
        var plants = await plantRepository.GetPlantsForUser(userEmail, pageNumber, pageSize);
        plants.ForEach(plant => plant.ImageUrl = blobStorageService.GenerateSasUri(plant.ImageUrl, true)); // Otherwise the client can't access the image
        return plants;
    }

    public async Task<List<Plant>> GetPlantsForCollection(Guid collectionId)
    {
        var plants = await plantRepository.GetPlantsForCollection(collectionId);
        plants.ForEach(plant => plant.ImageUrl = blobStorageService.GenerateSasUri(plant.ImageUrl, true)); // Otherwise the client can't access the image
        return plants;
    }
    
    public async Task<Plant> UpdatePlant(UpdatePlantDto updatePlantDto, string requesterEmail)
    {
        var plant = await VerifyPlantExistsAndUserHasAccess(updatePlantDto.PlantId, requesterEmail);

        // Update plant requirements if they are provided
        var requirements = plant.Requirements;
        if (updatePlantDto.UpdateRequirementDto is not null)
        {
            requirements = await requirementService.UpdateRequirements(updatePlantDto.UpdateRequirementDto, plant.PlantId);
        }
        
        // The urls coming from the client are SAS urls, so we need to convert them to normal urls
        var imageUrl = blobStorageService.GetBlobUrlFromSasUri(plant.ImageUrl);
        if (updatePlantDto.Base64Image is not null)
        {
          imageUrl = await blobStorageService.SaveImageToBlobStorage(updatePlantDto.Base64Image, requesterEmail, true, plant.ImageUrl);
        }
        
        // Update the plant
        plant = new Plant
        {
            PlantId = updatePlantDto.PlantId,
            UserEmail = plant.UserEmail,
            CollectionId = updatePlantDto.CollectionId,
            Nickname = updatePlantDto.Nickname ?? plant.Nickname,
            ImageUrl = imageUrl,
            Requirements = requirements,
            ConditionsLogs = plant.ConditionsLogs,
            LatestChange = DateTime.UtcNow
        };
        
        plant.ImageUrl = blobStorageService.GenerateSasUri(plant.ImageUrl, true);
        return await plantRepository.UpdatePlant(plant);
    }
    
    public async Task DeletePlant(Guid id, string requesterEmail)
    {
        var plant = await VerifyPlantExistsAndUserHasAccess(id, requesterEmail);
        await plantRepository.DeletePlant(plant);
    }
    
    public async Task<Guid> GetPlantIdByDeviceIdAsync(string deviceId)
    {
        return await plantRepository.GetPlantIdByDeviceIdAsync(deviceId);
    }

    public async Task<List<GetCriticalPlantDto>> GetCriticalPlants(string requesterEmail)
    {
        var plants = await plantRepository.GetCriticalPlants(requesterEmail);
        plants.ForEach(plant => plant.ImageUrl = blobStorageService.GenerateSasUri(plant.ImageUrl, true));

        var criticalPlants = plants.Select(GetCriticalPlantDto.FromPlant)
            .Where(plant => plant.Mood < 2)
            .ToList();
        criticalPlants.ForEach(criticalPlant =>
        {
            var conditionsLog = plants.First(plant => plant.PlantId == criticalPlant.PlantId).ConditionsLogs
                .FirstOrDefault();
            criticalPlant.SuggestedAction = GetSuggestedAction(criticalPlant.Requirements, conditionsLog);
        });

        return criticalPlants;
    }
    
    public async Task<int> GetHappyPlantsCount(string userEmail)
    {
        return await plantRepository.GetHappyPlantsCount(userEmail);
    }
    
    public async Task<int> GetTotalPlantsCount(string userEmail)
    {
        return await plantRepository.GetTotalPlantsCount(userEmail);
    }

    private string GenerateRandomNickname()
    {
        var firstName = new List<string>
        {
            "Alice", "Bob", "Charlie", "Daisy", "Edward", "Fiona", "George", "Mallory", "Rose", "Fern", "Jeppe", "Rasmus", "Alex"
        };
        var lastName = new List<string>
        {
            "Leaf", "Sprout", "Root", "Petal", "Bud", "Bloom", "Thistle", "Stem", "Twig", "Green", "Bush"
        };
        
        var random = new Random();
        var randomHumanName = firstName[random.Next(firstName.Count)];
        var randomPlantName = lastName[random.Next(lastName.Count)];
        var plantNickname = $"{randomHumanName} {randomPlantName}";
        
        return plantNickname;
    }
    
    private async Task<Plant> VerifyPlantExistsAndUserHasAccess(Guid plantId, string requesterEmail)
    {
        var plant = await plantRepository.GetPlantById(plantId);
        if (plant == null) throw new NotFoundException("Plant not found");
        if (plant.UserEmail != requesterEmail) throw new NoAccessException("You don't have access to this plant");
        return plant;
    }

    // TODO: Consider multiple suggestions
    private string? GetSuggestedAction(Requirements idealRequirements, ConditionsLog? conditionsLog)
    {
        if (conditionsLog is null) return null;

        var mostCriticalRequirement =
            ConditionsLogsService.GetMostCriticalRequirement(idealRequirements, conditionsLog);
        if (mostCriticalRequirement is null) return null; // No need to suggest anything if everything is perfect

        switch (mostCriticalRequirement.Value.Key)
        {
            case RequirementType.SoilMoisture:
                var idealSoilMoisture = idealRequirements.SoilMoistureLevel.GetRange();
                return conditionsLog.SoilMoisture < idealSoilMoisture.Min ? "More water" : "Less water";
            case RequirementType.Temperature:
                return  conditionsLog.Temperature < idealRequirements.TemperatureLevel ? "Warmer place" : "Cooler place";
            case RequirementType.Humidity:
                var idealHumidity = idealRequirements.HumidityLevel.GetRange();
                return conditionsLog.Humidity < idealHumidity.Min ? "More humidity" : "Less humidity";
            case RequirementType.Light:
                var idealLight = idealRequirements.LightLevel.GetRange();
                return conditionsLog.Light < idealLight.Min ? "More light" : "Less light";
            default:
                throw new ArgumentException("Unknown requirement type");
        }
    }
}