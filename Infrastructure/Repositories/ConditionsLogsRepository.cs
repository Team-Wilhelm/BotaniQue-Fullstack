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
}