using Infrastructure.Repositories;
using Shared.Dtos;
using Shared.Models;

namespace api.Core.Services;

public class StatsService(PlantRepository plantRepository)
{
    public async Task<Stats> GetStats(string email)
    {
        return await plantRepository.GetStats(email);
    }
}
