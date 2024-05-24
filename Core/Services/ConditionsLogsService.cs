using Infrastructure.Repositories;
using Shared.Dtos;
using Shared.Exceptions;
using Shared.Models.Information;

namespace Core.Services;

public class ConditionsLogsService (UserService userService, ConditionsLogsRepository conditionsLogsRepository, PlantService plantService, RequirementService requirementService ,MqttPublisherService mqttPublisherService)
{
    private const int TemperatureTolerance = 1;
    
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
            var email = await userService.GetEmailFromDeviceId(createConditionsLogDto.DeviceId.ToString());
            //get connection by email
            if (!string.IsNullOrEmpty(email))
            {
                //TODO send event here
            }
            
            var moodDto = new MoodDto
            {
                Mood = newMood,
            };
            await mqttPublisherService.PublishAsync(moodDto, createConditionsLogDto.DeviceId);
        }
    }
    
    // TODO: make this more sensitive
    private async Task<int> CalculateMood (ConditionsLog conditionsLog)
    {
        var requirementsForPlant = await requirementService.GetRequirements(conditionsLog.PlantId);
        var mood = 0;

        var idealSoilMoisture = requirementsForPlant.SoilMoistureLevel.GetRange();
        if (idealSoilMoisture.Min <= conditionsLog.SoilMoisture && conditionsLog.SoilMoisture <= idealSoilMoisture.Max)
        {
            mood++;
        }
        
        var idealTemperature = requirementsForPlant.TemperatureLevel;
        if (idealTemperature - 5 <= conditionsLog.Temperature && conditionsLog.Temperature <= idealTemperature + 5)
        {
            mood++;
        }
        
        var idealHumidity = requirementsForPlant.HumidityLevel.GetRange();
        if (idealHumidity.Min <= conditionsLog.Humidity && conditionsLog.Humidity <= idealHumidity.Max)
        {
            mood++;
        }
        
        var idealLight = requirementsForPlant.LightLevel.GetRange();
        if (idealLight.Min <= conditionsLog.Light && conditionsLog.Light <= idealLight.Max)
        {
            mood++;
        }
        
        return mood;
    }
    
    public static KeyValuePair<RequirementType, double>? GetMostCriticalRequirement(Requirements idealRequirements, ConditionsLog conditionsLog)
    {
       var notInIdealRange = new Dictionary<RequirementType, double>();
       
        // Soil Moisture
         if (!idealRequirements.SoilMoistureLevel.IsInRange(conditionsLog.SoilMoisture))
         {
              notInIdealRange.Add(RequirementType.SoilMoisture, 0);
              
              // Check if below or above ideal value
              var idealSoilMoisture = idealRequirements.SoilMoistureLevel.GetRange();
              var smaller = conditionsLog.SoilMoisture < idealSoilMoisture.Min;

              if (smaller) notInIdealRange[RequirementType.SoilMoisture] = idealSoilMoisture.Min - conditionsLog.SoilMoisture;
              else notInIdealRange[RequirementType.SoilMoisture] = conditionsLog.SoilMoisture - idealSoilMoisture.Max;
         }
         
         // Temperature
         if (Math.Abs(idealRequirements.TemperatureLevel - conditionsLog.Temperature) > TemperatureTolerance)
         {
             notInIdealRange.Add(RequirementType.Temperature, 0);
             
             // Check if below or above ideal value
             var idealTemperature = idealRequirements.TemperatureLevel;
             var smaller = conditionsLog.Temperature < idealTemperature;

             if (smaller) notInIdealRange[RequirementType.Temperature] = idealTemperature - conditionsLog.Temperature;
             else notInIdealRange[RequirementType.Temperature] = conditionsLog.Temperature - idealTemperature;
         }
         
         // Humidity
         if (!idealRequirements.HumidityLevel.IsInRange(conditionsLog.Humidity))
         {
             notInIdealRange.Add(RequirementType.Humidity, 0);
             
             // Check if below or above ideal value
             var idealHumidity = idealRequirements.HumidityLevel.GetRange();
             var smaller = conditionsLog.Humidity < idealHumidity.Min;

             if (smaller) notInIdealRange[RequirementType.Humidity] = idealHumidity.Min - conditionsLog.Humidity;
             else notInIdealRange[RequirementType.Humidity] = conditionsLog.Humidity - idealHumidity.Max;
         }
         
         // Light
         if (!idealRequirements.LightLevel.IsInRange(conditionsLog.Light))
         {
             notInIdealRange.Add(RequirementType.Light, 0);
             
             // Check if below or above ideal value
             var idealLight = idealRequirements.LightLevel.GetRange();
             var  smaller = conditionsLog.Light < idealLight.Min;

             if (smaller) notInIdealRange[RequirementType.Light] = idealLight.Min - conditionsLog.Light;
             else notInIdealRange[RequirementType.Light] = conditionsLog.Light - idealLight.Max;
         }
         
        
         // The higher the difference from the ideal state, the more critical the requirement
         try
         {
             return notInIdealRange.MaxBy(suggestion => suggestion.Value);
         } catch (InvalidOperationException)
         {
             return null;
         }
    }
    
    public async Task<ConditionsLog> GetLatestConditionsLogForPlant(Guid plantId, string loggedInUser)
    {
        var plant = await plantService.GetPlantById(plantId, loggedInUser);
        var conditionsLog = await conditionsLogsRepository.GetLatestConditionsLogForPlant(plantId);
        if (conditionsLog is null) throw new NotFoundException($"No conditions log found for plant with id {plantId}");
        return conditionsLog;
    }

    public async Task<List<ConditionsLog>> GetConditionsLogsForPlant(Guid dtoPlantId, int timeSpanInDays, string loggedInUser)
    {
        var plant = plantService.GetPlantById(dtoPlantId, loggedInUser).Result;
        if (plant == null) throw new NotFoundException("Plant not found");
        return await conditionsLogsRepository.GetConditionsLogsForPlant(dtoPlantId, timeSpanInDays);
    }
}