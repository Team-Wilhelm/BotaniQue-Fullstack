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
            }
        );

        var plants = await plantRepository.GetPlantsForUser("bob@app.com", 1, 5);
        Console.WriteLine(plants.Count);
        
        var requirementsRepository = scope.ServiceProvider.GetRequiredService<RequirementsRepository>();
        var requirements1 = await requirementsRepository.CreateRequirements(
            new Requirements
            {
                RequirementsId = Guid.NewGuid(),
                PlantId = plants[0].PlantId,
                LightLevel = RequirementLevel.Low,
                SoilMoistureLevel = RequirementLevel.Medium,
                HumidityLevel = RequirementLevel.High,
                TemperatureLevel = RequirementLevel.Medium,
            }
        );
        plants[0].Requirements = requirements1;
        
        var requirements2 = await requirementsRepository.CreateRequirements(
            new Requirements
            {
                RequirementsId = Guid.NewGuid(),
                PlantId = plants[1].PlantId,
                LightLevel = RequirementLevel.High,
                SoilMoistureLevel = RequirementLevel.Low,
                HumidityLevel = RequirementLevel.Low,
                TemperatureLevel = RequirementLevel.Medium,
            }
        );
        plants[1].Requirements = requirements2;
    }
}