using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Dtos.FromClient.Identity;
using Shared.Models;
using Shared.Models.Identity;
using Shared.Models.Information;

namespace Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; init; }
    public DbSet<Plant> Plants { get; init; }
    public DbSet<Collection> Collections { get; init; }
    public DbSet<ConditionsLog> ConditionsLogs { get; init; }
    public DbSet<Requirements> Requirements { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasKey(e => e.UserEmail);

        modelBuilder.Entity<User>()
            .HasMany(e => e.Collections)
            .WithOne()
            .HasForeignKey(e => e.UserEmail);

        modelBuilder.Entity<User>()
            .HasMany(e => e.Plants)
            .WithOne()
            .HasForeignKey(e => e.UserEmail);

        modelBuilder.Entity<Collection>()
            .HasMany(e => e.Plants)
            .WithOne()
            .HasForeignKey(e => e.CollectionId);

        modelBuilder.Entity<Plant>()
            .HasMany(e => e.ConditionsLogs)
            .WithOne()
            .HasForeignKey(e => e.PlantId);

        modelBuilder.Entity<Plant>()
            .HasOne(e => e.Requirements)
            .WithOne()
            .HasForeignKey<Requirements>(e => e.PlantId);

        modelBuilder.Entity<Requirements>()
            .HasKey(e => e.RequirementsId);

        modelBuilder.Entity<ConditionsLog>()
            .HasKey(e => e.ConditionsId);

        base.OnModelCreating(modelBuilder);
    }

    public async Task SeedDevelopmentDataAsync(IServiceScope scope, string defaultPlantImage)
    {
        var userRepository = scope.ServiceProvider.GetRequiredService<UserRepository>();
        await userRepository.CreateUser(new RegisterUserDto
        {
            Email = "bob@app.com",
            Password = "password",
            Username = "bob"
        });

        var collectionsRepository = scope.ServiceProvider.GetRequiredService<CollectionsRepository>();
        var collection1 = await collectionsRepository.CreateCollection(
            new Collection
            {
                CollectionId = Guid.NewGuid(),
                Name = "Succulents",
                UserEmail = "bob@app.com",
            }
        );
        var collection2 = await collectionsRepository.CreateCollection(
            new Collection
            {
                CollectionId = Guid.NewGuid(),
                Name = "Cacti",
                UserEmail = "bob@app.com",
            }
        );

        var plantRepository = scope.ServiceProvider.GetRequiredService<PlantRepository>();
        await plantRepository.CreatePlant(
            new Plant
            {
                PlantId = Guid.NewGuid(),
                Nickname = "Aloe Vera",
                UserEmail = "bob@app.com",
                ImageUrl = defaultPlantImage,
                CollectionId = collection1.CollectionId,
                LatestChange = DateTime.UtcNow.Subtract(TimeSpan.FromDays(5))
            }
        );

        await plantRepository.CreatePlant(
            new Plant
            {
                PlantId = Guid.NewGuid(),
                Nickname = "Prickly Pear",
                UserEmail = "bob@app.com",
                ImageUrl = defaultPlantImage,
                CollectionId = collection2.CollectionId,
                LatestChange = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7))
            }
        );

        await plantRepository.CreatePlant(
            new Plant
            {
                PlantId = Guid.NewGuid(),
                Nickname = "Dying plant",
                UserEmail = "bob@app.com",
                ImageUrl = defaultPlantImage,
                CollectionId = collection2.CollectionId,
                LatestChange = DateTime.UtcNow
            }
        );

        var plants = await plantRepository.GetPlantsForUser("bob@app.com", 1, 5);

        var requirementsRepository = scope.ServiceProvider.GetRequiredService<RequirementsRepository>();
        await requirementsRepository.CreateRequirements(
            new Requirements
            {
                RequirementsId = Guid.NewGuid(),
                PlantId = plants.First(p => p.Nickname == "Aloe Vera").PlantId,
                LightLevel = RequirementLevel.Low,
                SoilMoistureLevel = RequirementLevel.Medium,
                HumidityLevel = RequirementLevel.High,
                TemperatureLevel = 22,
            }
        );

      await requirementsRepository.CreateRequirements(
            new Requirements
            {
                RequirementsId = Guid.NewGuid(),
                PlantId =  plants.First(p => p.Nickname == "Prickly Pear").PlantId,
                LightLevel = RequirementLevel.High,
                SoilMoistureLevel = RequirementLevel.Low,
                HumidityLevel = RequirementLevel.Low,
                TemperatureLevel = 27,
            }
        );
      
       await requirementsRepository.CreateRequirements(
            new Requirements
            {
                RequirementsId = Guid.NewGuid(),
                PlantId =  plants.First(p => p.Nickname == "Dying plant").PlantId,
                LightLevel = RequirementLevel.High,
                SoilMoistureLevel = RequirementLevel.Low,
                HumidityLevel = RequirementLevel.Medium,
                TemperatureLevel = 24,
            }
        );

        var conditionsLogRepository = scope.ServiceProvider.GetRequiredService<ConditionsLogsRepository>();

        for (var i = 0; i < 12; i++)
        {
            await conditionsLogRepository.CreateConditionsLogAsync(
                GetRandomConditionsLog(plants.First(p => p.Nickname == "Prickly Pear").PlantId, i * 6)
            );
            await conditionsLogRepository.CreateConditionsLogAsync(
                GetRandomConditionsLog(plants.First(p => p.Nickname == "Aloe Vera").PlantId, i * 6)
            );
        }

        await conditionsLogRepository.CreateConditionsLogAsync(
            new ConditionsLog()
            {
                ConditionsId = Guid.NewGuid(),
                PlantId = plants.First(p => p.Nickname == "Dying plant").PlantId,
                TimeStamp = DateTime.UtcNow,
                Mood = -1,
                SoilMoisture = 55,
                Light = 13,
                Humidity = 68,
                Temperature = 25,
            }
        );
    }

    private double GetRandomLevelValue()
    {
        var random = new Random();
        return random.NextDouble() * 100;
    }

    private int GetRandomMood()
    {
        var random = new Random();
        return random.Next(0, 5);
    }

    private int GetRandomTemperature()
    {
        var random = new Random();
        return random.Next(-20, 45);
    }

    private ConditionsLog GetRandomConditionsLog(Guid plantId, int hoursAgo = 0)
    {
        return new ConditionsLog
        {
            ConditionsId = Guid.NewGuid(),
            PlantId = plantId,
            TimeStamp = DateTime.UtcNow.Subtract(TimeSpan.FromHours(hoursAgo)),
            Mood = GetRandomMood(),
            SoilMoisture = GetRandomLevelValue(),
            Light = GetRandomLevelValue(),
            Temperature = GetRandomTemperature(),
            Humidity = GetRandomLevelValue(),
        };
    }
}