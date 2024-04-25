using Infrastructure.Repositories;
using Shared.Dtos;
using Shared.Models.Exceptions;
using Shared.Models.Information;

namespace Core.Services;

public class ConditionsLogsService (ConditionsLogsRepository conditionsLogsRepository, PlantRepository plantRepository)
{
    public async Task CreateConditionsLogAsync(CreateConditionsLogDto createConditionsLogDto)
    {
        var plantId = await plantRepository.GetPlantIdByDeviceIdAsync(createConditionsLogDto.DeviceId.ToString());
        
        if (plantId == Guid.Empty)
        {
            throw new RegisterDeviceException();
        }
        var conditionsLog = new ConditionsLog
        {
            ConditionsId = new Guid(),
            TimeStamp = DateTime.UtcNow, //TODO get this from the right place
            SoilMoisture = CalculateSoilMoistureLevel(createConditionsLogDto.SoilMoisturePercentage),
            LightLevel = CalculateLightLevel(createConditionsLogDto.LightLevel),
            Temperature = CalculateTemperatureLevel(createConditionsLogDto.Temperature),
            Humidity = CalculateHumidityLevel(createConditionsLogDto.Humidity),
            PlantId = plantId
        };
        
        conditionsLog.Mood = CalculateMood(conditionsLog);

        Console.WriteLine("Conditions log created");
        Console.WriteLine(conditionsLog);

        await conditionsLogsRepository.CreateConditionsLogAsync(conditionsLog);
    }
    
    private RequirementLevel CalculateTemperatureLevel (double value)
    {
        return value switch
        {
            //TODO fix these values
            < 1 => RequirementLevel.Low,
            > 2 => RequirementLevel.High,
            _ => RequirementLevel.Medium
        };
    }
    
    private RequirementLevel CalculateLightLevel (double value)
    {
        return value switch
        {
            //TODO fix these values
            < 1 => RequirementLevel.Low,
            > 2 => RequirementLevel.High,
            _ => RequirementLevel.Medium
        };
    }
    
    private RequirementLevel CalculateSoilMoistureLevel (double value)
    {
        return value switch
        {
            //TODO fix these values
            < 1 => RequirementLevel.Low,
            > 2 => RequirementLevel.High,
            _ => RequirementLevel.Medium
        };
    }
    
    private RequirementLevel CalculateHumidityLevel (double value)
    {
        return value switch
        {
            //TODO fix these values
            < 1 => RequirementLevel.Low,
            > 2 => RequirementLevel.High,
            _ => RequirementLevel.Medium
        };
    }
    
    private int CalculateMood (Conditions conditions)
    {
        // Compare ideal requirements for humidity, temperature, soil moisture and light level with actual conditions and calculate mood from 0-4
        // get ideal requirements from plant
        var requirementsForPlant = plantRepository.GetRequirementsForPlant(conditions.PlantId);
        // compare with actual conditions
        var mood = 0;
        // calculate mood
        mood += CalculateScore((int)requirementsForPlant.Result.Humidity, (int)conditions.Humidity);
        mood += CalculateScore((int)requirementsForPlant.Result.Temperature, (int)conditions.Temperature);
        mood += CalculateScore((int)requirementsForPlant.Result.SoilMoisture, (int)conditions.SoilMoisture);
        mood += CalculateScore((int)requirementsForPlant.Result.LightLevel, (int)conditions.LightLevel);
        
        if (mood == 0)
        {
            return 0;
        }
        return mood / 4;
    }
    
    private int CalculateScore(int ideal, int actual)
    {
        var difference = Math.Abs(ideal - actual);
        switch (difference)
        {
            case 0:
                return 4; // Exact match
            case 1:
                return 2; // One away
            default:
                return 0; // Two away
        }
    }
}