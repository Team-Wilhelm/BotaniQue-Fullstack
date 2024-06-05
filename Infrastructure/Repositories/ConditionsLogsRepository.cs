using Microsoft.EntityFrameworkCore;
using Shared.Models.Information;

namespace Infrastructure.Repositories;

public class ConditionsLogsRepository (IDbContextFactory<ApplicationDbContext> dbContextFactory)
{
    public async Task<ConditionsLog> CreateConditionsLogAsync(ConditionsLog conditionsLog)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var entityEntry = await context.ConditionsLogs.AddAsync(conditionsLog);
        await context.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public async Task<int> GetRecentMoodAsync(Guid plantId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        try
        {
            var recentMood = await context.ConditionsLogs
                .Where(log => log.PlantId == plantId)
                .OrderByDescending(log => log.TimeStamp)
                .Select(log => log.Mood)
                .FirstAsync();

            return recentMood;
        }
        catch (InvalidOperationException)
        {
            // Return -1 if no entries are found
            return -1;
        }
    }

    public async Task<ConditionsLog?> GetLatestConditionsLogForPlant(Guid plantId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.ConditionsLogs
            .Where(log => log.PlantId == plantId)
            .OrderByDescending(log => log.TimeStamp)
            .FirstOrDefaultAsync();
    }

    // TODO: returns empty if there is no data in the past 7 days, but should then look further back
    public async Task<List<ConditionsLog>> GetConditionsLogsForPlant(Guid plantId, int timeSpanInDays)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var logs = await context.ConditionsLogs
            .Where(log => log.PlantId == plantId && log.TimeStamp >= DateTime.UtcNow.AddDays(-timeSpanInDays))
            .ToListAsync();
        
        if (logs.Count == 0)
        {
           logs = await context.ConditionsLogs
                .Where(log => log.PlantId == plantId)
                .OrderByDescending(log => log.TimeStamp)
                .Take(7)
                .ToListAsync();
        }

        var groupedLogs = logs
            .GroupBy(log => new DateTime(log.TimeStamp.Year, log.TimeStamp.Month, timeSpanInDays == 365 ? 1 : log.TimeStamp.Day))
            .Select(group => new ConditionsLog
            {
                TimeStamp = group.Key,
                Temperature  = group.Average(log => log.Temperature),
                Light = group.Average(log => log.Light),
                SoilMoisture = group.Average(log => log.SoilMoisture),
                Humidity = group.Average(log => log.Humidity),
                Mood = (int) group.Average(log => log.Mood),
                ConditionsId = Guid.Empty, // This is okay, we won't interact with the grouped values, just display them in the UI
                PlantId = plantId
            })
            .OrderBy(log => log.TimeStamp)
            .ToList();

        return groupedLogs;
    }
    
    
}