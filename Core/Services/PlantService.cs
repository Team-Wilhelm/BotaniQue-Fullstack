﻿using Infrastructure.Repositories;
using Shared.Dtos.Plant;
using Shared.Models;
using Shared.Models.Exceptions;

namespace Core.Services;

public class PlantService (PlantRepository plantRepository)
{
    public async Task<Plant> CreatePlant(CreatePlantDto createPlantDto)
    {
        if (string.IsNullOrEmpty(createPlantDto.Nickname))
        {
            createPlantDto.Nickname = GenerateRandomNickname();
        }
        var plant = await plantRepository.CreatePlant(createPlantDto);
        if (plant == null) throw new NotFoundException("Plant not found");
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
        return await plantRepository.UpdatePlant(updatePlantDto);
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