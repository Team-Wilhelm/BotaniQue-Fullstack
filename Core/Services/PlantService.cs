using Infrastructure.Repositories;
using Shared.Dtos.FromClient.Plant;
using Shared.Dtos.Plant;
using Shared.Models;
using Shared.Models.Exceptions;

namespace Core.Services;

public class PlantService (PlantRepository plantRepository, RequirementService requirementService)
{
    public async Task<Plant> CreatePlant(CreatePlantDto createPlantDto)
    {
        if (string.IsNullOrEmpty(createPlantDto.Nickname))
        {
            createPlantDto.Nickname = GenerateRandomNickname();
        }
         
        // Insert plant first to get the plantId
        var plant = new Plant
        {
            PlantId = Guid.NewGuid(),
            UserEmail = createPlantDto.UserEmail,
            // CollectionId = Guid.Empty, // TODO: fix when collections are implemented
            Nickname = createPlantDto.Nickname,
            ImageUrl = createPlantDto.ImageUrl,
        };
        
        await plantRepository.CreatePlant(plant);

        // Create requirements for the plant to crete a link between the two
        var requirementsDto = createPlantDto.CreateRequirementsDto;
        requirementsDto.PlantId = plant.PlantId;
        await requirementService.CreateRequirements(requirementsDto);
        
        return plant;
    }
    
    public async Task<Plant> GetPlantById(Guid id)
    {
        var plant = await plantRepository.GetPlantById(id);
        if (plant == null) throw new NotFoundException("Plant not found");
        return plant;
    }
    
    public async Task<List<Plant>> GetPlantsForUser(string userEmail, int pageNumber, int pageSize)
    {
        var plants = await plantRepository.GetPlantsForUser(userEmail, pageNumber, pageSize);
        if (plants == null) throw new NotFoundException("No plants found");
        return plants;
    }
    
    public async Task<Plant> UpdatePlant(UpdatePlantDto updatePlantDto)
    {
        var plant = await plantRepository.GetPlantById(updatePlantDto.PlantId);
        if (plant == null) throw new NotFoundException("Plant not found");
        
        plant = new Plant
        {
            PlantId = updatePlantDto.PlantId,
            UserEmail = plant.UserEmail,
            CollectionId = updatePlantDto.CollectionId,
            Nickname = updatePlantDto.Nickname,
            ImageUrl = updatePlantDto.ImageUrl,
            Requirements = plant.Requirements,
            ConditionsLogs = plant.ConditionsLogs
        };
        return await plantRepository.UpdatePlant(plant);
    }
    
    public async Task DeletePlant(Guid id)
    {
        await plantRepository.DeletePlant(id);
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
}