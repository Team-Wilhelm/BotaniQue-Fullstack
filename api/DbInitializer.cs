using api.Core.Services;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos;
using Shared.Dtos.FromClient.Collections;
using Shared.Dtos.FromClient.Identity;
using Shared.Dtos.FromClient.Plant;
using Shared.Dtos.FromClient.Requirements;
using Shared.Models;
using Shared.Models.Information;

namespace api;

public class DbInitializer(IServiceProvider serviceProvider, string? defaultPlantImage = null)
{
    private const string DefaultUserEmail = "bob@botanique.com";
    private readonly IServiceScope _scope = serviceProvider.CreateScope();
    private readonly Dictionary<string, Collection> _collections = new();
    private readonly Dictionary<string, Plant> _plants = new();
    
    public async Task InitializeDatabaseAsync()
    {
        var scope = serviceProvider.CreateScope();
        var db = await scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContextAsync();
             
        if (EnvironmentHelper.IsNonProd())
        {
            await db.Database.EnsureDeletedAsync();
        }
            
        await db.Database.EnsureCreatedAsync();
        await db.Database.MigrateAsync();
    }
    
    public async Task PopulateDatabaseAsync()
    {
        await CreateUser();
        await CreateCollections();
        await CreatePlants();
        await CreateHistoricalData();
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Database has been populated");
        Console.ResetColor();
    }

    private async Task CreateUser()
    {
        await _scope.ServiceProvider
            .GetRequiredService<UserService>()
            .CreateUser(new RegisterUserDto
        {
            Email = DefaultUserEmail,
            Password = "SuperSecretPassword123!",
            Username = "Bob"
        });
    }

    private async Task CreateCollections()
    {
        var collectionService = _scope.ServiceProvider.GetRequiredService<CollectionsService>();
        var succulents = await collectionService.CreateCollection(
            new CreateCollectionDto
            {
                Name = "Ferns",
            },
            DefaultUserEmail
        );
        _collections.Add("Succulents", succulents);
        
        var ferns = await collectionService.CreateCollection(
            new CreateCollectionDto
            {
                Name = "Ferns",
            },
            DefaultUserEmail
        );
        _collections.Add("Ferns", ferns);
    }

    private List<CreateRequirementsDto> GetRequirements()
    {
        var createRequirementsList = new List<CreateRequirementsDto>
        {
            // Aloe Vera
            new()
            {
                LightLevel = RequirementLevel.Medium,
                SoilMoistureLevel = RequirementLevel.Medium,
                HumidityLevel = RequirementLevel.Low,
                TemperatureLevel = 22,
            },
            // Prickly pear
            new()
            {
                LightLevel = RequirementLevel.High,
                SoilMoistureLevel = RequirementLevel.Low,
                HumidityLevel = RequirementLevel.Low,
                TemperatureLevel = 27,
            },
            // Dying plant
            new()
            {
                LightLevel = RequirementLevel.Low,
                SoilMoistureLevel = RequirementLevel.High,
                HumidityLevel = RequirementLevel.Medium,
                TemperatureLevel = 20,
            }
        };

        return createRequirementsList;
    }

    private async Task CreatePlants()
    {
        var requirements = GetRequirements();
        
        var plantService = _scope.ServiceProvider.GetRequiredService<PlantService>();
        var plant1 = await plantService.CreatePlant(
            new CreatePlantDto
            {
                Nickname = "Aloe Vera",
                CreateRequirementsDto = requirements[0],
                DeviceId = "264625477326660",
            }, DefaultUserEmail
        );
        _plants.Add("Aloe Vera", plant1);

        var plant2 = await plantService.CreatePlant(
            new CreatePlantDto
            {
                Nickname = "Prickly Pear",
                CreateRequirementsDto = requirements[1],
                CollectionId = _collections["Succulents"].CollectionId,
                DeviceId = "000000000000001"
            }, DefaultUserEmail
        );
        _plants.Add("Prickly Pear", plant2);

        var plant3 = await plantService.CreatePlant(
            new CreatePlantDto
            {
                Nickname = "Dying plant",
                CollectionId = _collections["Ferns"].CollectionId,
                CreateRequirementsDto = requirements[2],
                DeviceId = "000000000000002"
            }, DefaultUserEmail
            );
        _plants.Add("Dying plant", plant3);
    }

    private async Task CreateHistoricalData()
    {
        var conditionsLogService = _scope.ServiceProvider.GetRequiredService<ConditionsLogsService>();
        for (var i = 0; i < 100; i++)
        {
            await conditionsLogService.CreateConditionsLogAsync(
                new CreateConditionsLogDto
                {
                    DeviceId = long.Parse(_plants["Aloe Vera"].DeviceId!),
                    SoilMoisturePercentage = GetRandomLevelValue(),
                    Light = GetRandomLevelValue(),
                    Temperature = GetRandomTemperature(),
                    Humidity = GetRandomLevelValue(),
                }
            );
            await conditionsLogService.CreateConditionsLogAsync(
                new CreateConditionsLogDto
                {
                    DeviceId = long.Parse(_plants["Prickly Pear"].DeviceId!),
                    SoilMoisturePercentage = GetRandomLevelValue(),
                    Light = GetRandomLevelValue(),
                    Temperature = GetRandomTemperature(),
                    Humidity = GetRandomLevelValue(),
                }
            );
        }
        
        var dyingPlant = _plants["Dying plant"];
        await conditionsLogService.CreateConditionsLogAsync(
    
            new CreateConditionsLogDto
            {
                DeviceId = long.Parse(dyingPlant.DeviceId!),
                SoilMoisturePercentage = GetValueOutsideOfIdealRange(dyingPlant.Requirements!.SoilMoistureLevel),
                Light = GetValueOutsideOfIdealRange(dyingPlant.Requirements!.LightLevel),
                Temperature = dyingPlant.Requirements!.TemperatureLevel - 10,
                Humidity = GetValueOutsideOfIdealRange(dyingPlant.Requirements!.HumidityLevel),
            }
        );
    }

    private double GetRandomLevelValue()
    {
        var random = new Random();
        return random.NextDouble() * 100;
    }

    private int GetRandomTemperature()
    {
        var random = new Random();
        return random.Next(-20, 45);
    }
    
    private double GetValueOutsideOfIdealRange(RequirementLevel level)
    {
        var idealValueRange = level.GetRange();
        var random = new Random();
        var value = random.NextDouble() * 100;
        while (value > idealValueRange.Min && value < idealValueRange.Max)
        {
            value = random.NextDouble() * 100;
        }

        return value;
    }
}