using api.Core.Services;
using api.Core.Services.External.BackgroundRemoval;
using api.Core.Services.External.BlobStorage;
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
        services.AddSingleton<CollectionsService>();
        services.AddSingleton<ConditionsLogsService>();
        services.AddSingleton<RequirementService>();
        services.AddSingleton<MqttSubscriberService>();
        services.AddSingleton<MqttPublisherService>();
        
        // External services
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing")
        {
            services.AddSingleton<IImageBackgroundRemoverService, MockImageBackgroundRemoverService>();
            services.AddSingleton<IBlobStorageService, MockBlobStorageService>();
        }
        else
        {
            services.AddSingleton<IImageBackgroundRemoverService, ImageBackgroundRemoverService>();
            services.AddSingleton<IBlobStorageService, BlobStorageService>();
        }
    }
}