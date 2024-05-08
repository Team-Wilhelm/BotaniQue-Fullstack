using System.Reflection;
using System.Text.Json;
using api.Events.Auth.Client;
using api.Extensions;
using Core.Options;
using Core.Services;
using Fleck;
using Infrastructure;
using Infrastructure.Repositories;
using lib;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Shared.Dtos.FromClient;
using Shared.Dtos.FromClient.Identity;
using Shared.Exceptions;
using Shared.Models;
using Testcontainers.PostgreSql;

namespace api;

public static class Startup
{
    private static readonly List<string> PublicEvents =
    [
        nameof(ClientWantsToLogIn),
        nameof(ClientWantsToLogOut),
        nameof(ClientWantsToSignUp)
    ];
    
    public static async Task Main(string[] args)
    {
        var app = await StartApi(args);
        await app.RunAsync();
    }

    public static async Task<WebApplication> StartApi(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo
            .Console(outputTemplate: "\n{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}\n")
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);

        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing")
        {
            var dbContainer = 
                new PostgreSqlBuilder()
                .WithDatabase("botanique")
                .WithUsername("root")
                .WithPassword("password")
                .Build();

            await dbContainer.StartAsync();
            
            var connectionString = dbContainer.GetConnectionString() + ";Include Error Detail=true;";
            builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString ?? throw new Exception("Connection string cannot be null"));
            });
        }

        else
        {
            var connectionString = builder.Configuration.GetConnectionString("BotaniqueDb");
            builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
            {
                connectionString ??= Environment.GetEnvironmentVariable("BotaniqueDb");
                options.UseNpgsql(connectionString ?? throw new Exception("Connection string cannot be null"));
            });
        }

        builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JWT"));
        builder.Services.Configure<MqttOptions>(builder.Configuration.GetSection("MQTT"));
        builder.Services.Configure<AzureVisionOptions>(builder.Configuration.GetSection("AzureVision"));
        builder.Services.Configure<AzureBlobStorageOptions>(builder.Configuration.GetSection("AzureBlob"));
        
        
        // On ci options are stored as repository secrets
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing" && Environment.GetEnvironmentVariable("CI") is not null)
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
            
            builder.Services.Configure<AzureVisionOptions>(options =>
            {
                options.BaseUrl = Environment.GetEnvironmentVariable("AZURE_VISION_BASE_URL") ?? throw new Exception("Azure vision endpoint is missing");
                options.Key = Environment.GetEnvironmentVariable("AZURE_VISION_KEY") ?? throw new Exception("Azure vision key is missing");
                options.RemoveBackgroundEndpoint = Environment.GetEnvironmentVariable("AZURE_VISION_REMOVE_BACKGROUND_ENDPOINT") ?? throw new Exception("Azure vision remove background endpoint is missing");
            });
            
            builder.Services.Configure<AzureBlobStorageOptions>(options =>
            {
                options.ConnectionString = Environment.GetEnvironmentVariable("AZURE_BLOB_CONNECTION_STRING") ?? throw new Exception("Azure blob connection string is missing");
                options.PlantImagesContainer = Environment.GetEnvironmentVariable("AZURE_BLOB_PLANT_IMAGES_CONTAINERE") ?? throw new Exception("Azure blob container name is missing");
                options.UserProfileImagesContainer = Environment.GetEnvironmentVariable("AZURE_BLOB_USER_PROFILE_IMAGES_CONTAINER") ?? throw new Exception("Azure blob container name is missing");
            });
        }
        
        builder.Services.AddServicesAndRepositories();
        
        var services = builder.FindAndInjectClientEventHandlers(Assembly.GetExecutingAssembly());
        
        var app = builder.Build();

        if (args.Contains("--db-init"))
        {
            var scope = app.Services.CreateScope();
            var db = app.Services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
            await db.Database.MigrateAsync();

            var userRepository = scope.ServiceProvider.GetRequiredService<UserRepository>();
            await userRepository.CreateUser(new RegisterUserDto
            {
                Email = "bob@app.com",
                Password = "password",
                Username = "bob"
            });
        }

        var port = Environment.GetEnvironmentVariable("PORT") ?? "8181";
        var wsServer = new WebSocketServer($"ws://0.0.0.0:{port}");
        // builder.WebHost.UseUrls("http://*:9999");

        wsServer.Start(socket =>
        {
            socket.OnOpen = () => app.Services.GetRequiredService<WebSocketConnectionService>().AddConnection(socket);
            socket.OnClose = () => app.Services.GetRequiredService<WebSocketConnectionService>().RemoveConnection(socket);
            socket.OnMessage = async message =>
            {
                Log.Information("Received message: {message}", message);
                try
                {
                    // Check if the message contains a JWT token and if it is valid
                    var dto = JsonSerializer.Deserialize<BaseDtoWithJwt>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (dto is not null && PublicEvents.Contains(dto.eventType) == false)
                    {
                        if (dto.Jwt is null)
                        {
                            throw new NotAuthenticatedException("JWT token is missing. Please log in.");
                        }
                        
                        var jwtService = app.Services.GetRequiredService<JwtService>();
                        var jwtValid = jwtService.IsJwtTokenValid(dto.Jwt);
                        if (!jwtValid)
                        {
                            throw new NotAuthenticatedException("JWT token is not valid. Please log in.");
                        }
                    }
                    
                    await app.InvokeClientEventHandler(services, socket, message);
                }
                catch (Exception e)
                {
                    e.Handle(socket, message);
                }
            };
        });
        
        // Connect and subscribe to MQTT
        var mqttSubscriberService = app.Services.GetRequiredService<MqttSubscriberService>();
        _ = mqttSubscriberService.SubscribeAsync();

        return app;
    }
}