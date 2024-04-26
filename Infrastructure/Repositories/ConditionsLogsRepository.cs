using Microsoft.EntityFrameworkCore;
using Shared.Models.Information;

namespace Infrastructure.Repositories;

public class ConditionsLogsRepository (IDbContextFactory<ApplicationDbContext> dbContextFactory)
{
    public async Task CreateConditionsLogAsync(ConditionsLog conditionsLog)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        await context.ConditionsLogs.AddAsync(conditionsLog);
        await context.SaveChangesAsync();
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
}