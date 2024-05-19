using Infrastructure.Repositories;
using Shared.Dtos;
using Shared.Exceptions;
using Shared.Models.Information;

namespace Core.Services;

public class ConditionsLogsService (ConditionsLogsRepository conditionsLogsRepository, PlantService plantService, RequirementService requirementService ,MqttPublisherService mqttPublisherService)
{
    public async Task CreateConditionsLogAsync(CreateConditionsLogDto createConditionsLogDto)
    {
        var plantId = await plantService.GetPlantIdByDeviceIdAsync(createConditionsLogDto.DeviceId.ToString());
        
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
            PlantId = plantId,
            Mood = -1
        };
        
        var newMood = await CalculateMood(conditionsLog);
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
    
    private async Task<int> CalculateMood (ConditionsLog conditionsLog)
    {
        // Compare ideal requirements for humidity, temperature, soil moisture and light level with actual conditions and calculate mood from 0-4
        // get ideal requirements from plant
        var requirementsForPlant = await requirementService.GetRequirements(conditionsLog.PlantId);
        // compare with actual conditions
        var mood = 0;
        // calculate mood
        mood += CalculateScore((int)requirementsForPlant.HumidityLevel, (int)CalculateHumidityLevel(conditionsLog.Humidity));
        mood += CalculateScore((int)requirementsForPlant.TemperatureLevel, (int)CalculateTemperatureLevel(conditionsLog.Temperature));
        mood += CalculateScore((int)requirementsForPlant.SoilMoistureLevel, (int)CalculateSoilMoistureLevel(conditionsLog.SoilMoisture));
        mood += CalculateScore((int)requirementsForPlant.LightLevel, (int)CalculateLightLevel(conditionsLog.Light));
        
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
    
    public async Task<ConditionsLog> GetLatestConditionsLogForPlant(Guid plantId, string loggedInUser)
    {
        var plant = await plantService.GetPlantById(plantId, loggedInUser);
        var conditionsLog = await conditionsLogsRepository.GetLatestConditionsLogForPlant(plantId);
        if (conditionsLog is null) throw new NotFoundException($"No conditions log found for plant with id {plantId}");
        return conditionsLog;
    }

    public async Task<List<ConditionsLog>> GetConditionsLogsForPlant(Guid dtoPlantId, DateTime dtoStartDate, DateTime dtoEndDate, string loggedInUser)
    {
        var plant = plantService.GetPlantById(dtoPlantId, loggedInUser).Result;
        if (plant == null) throw new NotFoundException("Plant not found");
        return await conditionsLogsRepository.GetConditionsLogsForPlant(dtoPlantId, dtoStartDate, dtoEndDate);
    }
}