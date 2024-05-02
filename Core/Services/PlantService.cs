using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Core.Options;
using Infrastructure.Repositories;
using Microsoft.Extensions.Options;
using Shared.Dtos.FromClient.Plant;
using Shared.Dtos.Plant;
using Shared.Models;
using Shared.Models.Exceptions;
using Shared.Models.Information;

namespace Core.Services;

public class PlantService(
    PlantRepository plantRepository,
    RequirementService requirementService,
    IOptions<AzureBlobStorageOptions> azureBlobStorageOptions)
{
    private const string DefaultImageUrl =
        "https://www.creativefabrica.com/wp-content/uploads/2022/01/20/Animated-Plant-Graphics-23785833-1.jpg";

    // TODO: decode base 64 image and save it to blob storage
    public async Task<Plant> CreatePlant(CreatePlantDto createPlantDto)
    {
        if (string.IsNullOrEmpty(createPlantDto.Nickname))
        {
            createPlantDto.Nickname = GenerateRandomNickname();
        }

        string? ímageUrl = null;
        if (createPlantDto.Base64Image is not null)
        {
            ímageUrl = await SaveImageToBlobStorage(createPlantDto.Base64Image, createPlantDto.UserEmail);
        }
        
        // Insert plant first to get the plantId
        var plant = new Plant
        {
            PlantId = Guid.NewGuid(),
            UserEmail = createPlantDto.UserEmail,
            // CollectionId = Guid.Empty, // TODO: fix when collections are implemented
            Nickname = createPlantDto.Nickname,
            ImageUrl = ímageUrl ?? DefaultImageUrl,
            DeviceId = createPlantDto.DeviceId
        };
        
        await plantRepository.CreatePlant(plant);

        // Create requirements for the plant to crete a link between the two
        var requirementsDto = createPlantDto.CreateRequirementsDto;
        requirementsDto.PlantId = plant.PlantId;
        plant.Requirements =  await requirementService.CreateRequirements(requirementsDto);
        return plant;
    }

    private async Task<string> SaveImageToBlobStorage(string base64Image, string userEmail)
    {
        var imageBytes = Convert.FromBase64String(base64Image);
        var blobUrl = userEmail + "_" + Guid.NewGuid();
        var blobClient = new BlobClient(azureBlobStorageOptions.Value.ConnectionString, azureBlobStorageOptions.Value.PlantImagesContainer, blobUrl);
        var binaryData = new BinaryData(imageBytes);
        await blobClient.UploadAsync(binaryData, true);
        return blobClient.Uri.ToString();
    }

    public async Task<Plant> GetPlantById(Guid id, string requesterEmail)
    {
        var plant = await VerifyPlantExistsAndUserHasAccess(id, requesterEmail);
        return plant;
    }
    
    public async Task<List<Plant>> GetPlantsForUser(string userEmail, int pageNumber, int pageSize)
    {
        var plants = await plantRepository.GetPlantsForUser(userEmail, pageNumber, pageSize);
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
        
        // Update the plant
        plant = new Plant
        {
            PlantId = updatePlantDto.PlantId,
            UserEmail = plant.UserEmail,
            CollectionId = updatePlantDto.CollectionId,
            Nickname = updatePlantDto.Nickname,
            ImageUrl = updatePlantDto.ImageUrl ?? DefaultImageUrl,
            Requirements = requirements,
            ConditionsLogs = plant.ConditionsLogs
        };
        return await plantRepository.UpdatePlant(plant);
    }
    
    public async Task DeletePlant(Guid id, string requesterEmail)
    {
        var plant = await VerifyPlantExistsAndUserHasAccess(id, requesterEmail);
        await plantRepository.DeletePlant(plant);
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
}