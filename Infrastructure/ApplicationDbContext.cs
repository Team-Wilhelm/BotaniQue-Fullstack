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
}