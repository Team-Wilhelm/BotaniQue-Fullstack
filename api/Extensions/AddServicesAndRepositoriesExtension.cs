using Core.Services;
using Infrastructure.Repositories;

namespace api.Extensions;

public static class AddServicesAndRepositoriesExtension
{
    public static void AddServicesAndRepositories(this IServiceCollection services)
    {
        // Services
        services.AddSingleton<WebSocketConnectionService>();
        services.AddSingleton<JwtService>();
        services.AddSingleton<UserService>();
        services.AddSingleton<PlantService>();
        services.AddSingleton<RequirementService>();
        services.AddSingleton<MqttSubscriberService>();
        
        // Repositories
        services.AddSingleton<UserRepository>();
        services.AddSingleton<PlantRepository>();
        services.AddSingleton<RequirementsRepository>();
    }
}