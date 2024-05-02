using Core.Services;
using Infrastructure.Repositories;

namespace api.Extensions;

public static class AddServicesAndRepositoriesExtension
{
    public static void AddServicesAndRepositories(this IServiceCollection services)
    {
        // Repositories
        services.AddSingleton<UserRepository>();
        services.AddSingleton<PlantRepository>();
        services.AddSingleton<RequirementsRepository>();
        services.AddSingleton<ConditionsLogsRepository>();
        services.AddSingleton<CollectionsRepository>();
        
        // Services
        services.AddSingleton<WebSocketConnectionService>();
        services.AddSingleton<JwtService>();
        services.AddSingleton<UserService>();
        services.AddSingleton<PlantService>();
        services.AddSingleton<ConditionsLogsService>();
        services.AddSingleton<RequirementService>();
        services.AddSingleton<MqttSubscriberService>();
        services.AddSingleton<MqttPublisherService>();
        services.AddSingleton<ImageBackgroundRemoverService>();
        services.AddSingleton<BlobStorageService>();
    }
}