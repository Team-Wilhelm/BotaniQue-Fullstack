using Infrastructure.Repositories;
using Shared.Dtos;
using Shared.Models.Exceptions;
using Shared.Models.Information;

namespace Core.Services;

public class ConditionsLogsService (ConditionsLogsRepository conditionsLogsRepository, PlantRepository plantRepository, MqttPublisherService mqttPublisherService)
{
    public async Task CreateConditionsLogAsync(CreateConditionsLogDto createConditionsLogDto)
    {
        var plantId = await plantRepository.GetPlantIdByDeviceIdAsync(createConditionsLogDto.DeviceId.ToString());
        
        if (plantId == Guid.Empty)
        {
            throw new RegisterDeviceException();
        }
        
        var recentMood = conditionsLogsRepository.GetRecentMoodAsync(plantId).Result;
        
        var conditionsLog = new ConditionsLog
        {
            ConditionsId = new Guid(),
            TimeStamp = DateTime.UtcNow,
            SoilMoisture = createConditionsLogDto.SoilMoisturePercentage,
            Light = createConditionsLogDto.Light,
            Temperature = createConditionsLogDto.Temperature,
            Humidity = createConditionsLogDto.Humidity,
            PlantId = plantId
        };
        
        var newMood = CalculateMood(conditionsLog);
        conditionsLog.Mood = newMood;

        await conditionsLogsRepository.CreateConditionsLogAsync(conditionsLog);
        
        
        if (newMood != recentMood)
        {
            //TODO send Event to mobile app
            var moodDto = new MoodDto
            {
                Mood = newMood,
            };
            var deviceId = createConditionsLogDto.DeviceId;
            await mqttPublisherService.PublishAsync(moodDto, deviceId);
        }
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
    
    private int CalculateMood (ConditionsLog conditionsLog)
    {
        // Compare ideal requirements for humidity, temperature, soil moisture and light level with actual conditions and calculate mood from 0-4
        // get ideal requirements from plant
        var requirementsForPlant = plantRepository.GetRequirementsForPlant(conditionsLog.PlantId);
        // compare with actual conditions
        var mood = 0;
        // calculate mood
        mood += CalculateScore((int)requirementsForPlant.Result.HumidityLevel, (int)CalculateHumidityLevel(conditionsLog.Humidity));
        mood += CalculateScore((int)requirementsForPlant.Result.TemperatureLevel, (int)CalculateTemperatureLevel(conditionsLog.Temperature));
        mood += CalculateScore((int)requirementsForPlant.Result.SoilMoistureLevel, (int)CalculateSoilMoistureLevel(conditionsLog.SoilMoisture));
        mood += CalculateScore((int)requirementsForPlant.Result.LightLevel, (int)CalculateLightLevel(conditionsLog.Light));
        
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