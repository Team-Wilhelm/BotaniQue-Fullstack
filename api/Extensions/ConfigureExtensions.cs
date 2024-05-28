using api.Core.Options;

namespace api.Extensions;

public static class ConfigureExtensions
{
    public static void ConfigureOptions(this WebApplicationBuilder builder)
    {
        if (EnvironmentHelper.IsCi())
        {
            builder.Services.Configure<JwtOptions>(options =>
            {
                options.Key = Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new Exception("JWT key is missing");
                options.Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? throw new Exception("JWT issuer is missing");
                options.Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? throw new Exception("JWT audience is missing");
                options.ExpirationMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRY") ?? throw new Exception("JWT expiration minutes is missing"));
            });

            builder.Services.Configure<MqttOptions>(options =>
            {
                options.Server = Environment.GetEnvironmentVariable("MQTT_BROKER") ?? throw new Exception("MQTT broker is missing");
                options.Port = int.Parse(Environment.GetEnvironmentVariable("MQTT_PORT") ?? throw new Exception("MQTT port is missing"));
                options.ClientId = Environment.GetEnvironmentVariable("MQTT_CLIENT_ID") ?? throw new Exception("MQTT client id is missing");
                options.Username = Environment.GetEnvironmentVariable("MQTT_USERNAME") ?? throw new Exception("MQTT username is missing");
                options.SubscribeTopic = Environment.GetEnvironmentVariable("MQTT_SUBSCRIBE_TOPIC") ?? throw new Exception("MQTT subscribe topic is missing");
                options.PublishTopic = Environment.GetEnvironmentVariable("MQTT_PUBLISH_TOPIC") ?? throw new Exception("MQTT publish topic is missing");
            });

            if (EnvironmentHelper.IsTesting()) return;
          
            builder.Services.Configure<AzureVisionOptions>(options =>
            {
                options.RemoveBackgroundEndpoint = Environment.GetEnvironmentVariable("AZURE_VISION_REMOVE_BACKGROUND_ENDPOINT") ?? throw new Exception("Azure Vision endpoint is missing");
                options.Key = Environment.GetEnvironmentVariable("AZURE_VISION_KEY") ?? throw new Exception("Azure Vision key is missing");
                options.BaseUrl = Environment.GetEnvironmentVariable("AZURE_VISION_BASE_URL") ?? throw new Exception("Azure Vision base url is missing");
            });
            
            builder.Services.Configure<AzureBlobStorageOptions>(options =>
            {
                options.ConnectionString = Environment.GetEnvironmentVariable("AZURE_BLOB_CONNECTION_STRING") ?? throw new Exception("Azure Blob connection string is missing");
                options.PlantImagesContainer = Environment.GetEnvironmentVariable("AZURE_BLOB_PLANT_IMAGES_CONTAINER") ?? throw new Exception("Azure Blob plant images container name is missing");
                options.UserProfileImagesContainer = Environment.GetEnvironmentVariable("AZURE_BLOB_USER_PROFILE_IMAGES_CONTAINER") ?? throw new Exception("Azure Blob user profile images container name is missing");
                options.DefaultPlantImageUrl = Environment.GetEnvironmentVariable("AZURE_BLOB_DEFAULT_PLANT_IMAGE_URL") ?? throw new Exception("Azure Blob default plant image url is missing");
            });
        }
        else
        {
            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JWT"));
            builder.Services.Configure<MqttOptions>(builder.Configuration.GetSection("MQTT"));
            builder.Services.Configure<AzureVisionOptions>(builder.Configuration.GetSection("AzureVision"));
            builder.Services.Configure<AzureBlobStorageOptions>(builder.Configuration.GetSection("AzureBlob"));
        }
    }
}