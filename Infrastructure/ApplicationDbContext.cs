using Microsoft.EntityFrameworkCore;
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
            .HasMany<Collection>(e => e.Collections)
            .WithOne()
            .HasForeignKey(e => e.UserEmail);
        
        modelBuilder.Entity<Collection>()
            .HasMany<Plant>(e => e.Plants)
            .WithOne()
            .HasForeignKey(e => e.CollectionId);
        
        modelBuilder.Entity<Plant>()
            .HasMany<ConditionsLog>(e => e.ConditionsLogs)
            .WithOne()
            .HasForeignKey(e => e.PlantId);

        modelBuilder.Entity<Requirements>()
            .HasKey(e => e.ConditionsId);
        
        base.OnModelCreating(modelBuilder);
    }
}