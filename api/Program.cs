using System.Reflection;
using System.Text.Json;
using AsyncApi.Net.Generator;
using AsyncApi.Net.Generator.AsyncApiSchema.v2;
using Core.Options;
using Core.Services;
using Fleck;
using Infrastructure;
using Infrastructure.Repositories;
using lib;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Shared.Dtos.FromClient;
using Shared.Models;
using Shared.Models.Exceptions;

namespace api;

public static class Startup
{
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

        var connectionString = builder.Configuration.GetConnectionString("BotaniqueDb");
        builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
        {
            connectionString ??= Environment.GetEnvironmentVariable("BotaniqueDb");
            options.UseNpgsql(connectionString ?? throw new Exception("Connection string cannot be null"));
        });

        builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JWT"));
        builder.Services.Configure<MqttOptions>(builder.Configuration.GetSection("MQTT"));
        builder.Services.AddSingleton<WebSocketConnectionService>();
        builder.Services.AddSingleton<JwtService>();
        builder.Services.AddSingleton<UserRepository>();
        builder.Services.AddSingleton<PlantRepository>();
        builder.Services.AddSingleton<RequirementsRepository>();
        builder.Services.AddSingleton<ConditionsLogsRepository>();
        builder.Services.AddSingleton<UserService>();
        builder.Services.AddSingleton<PlantService>();
        builder.Services.AddSingleton<RequirementService>();
        builder.Services.AddSingleton<ConditionsLogsService>();
        builder.Services.AddSingleton<MqttSubscriberService>();
        // TODO: add repositories

        builder.Services.AddAsyncApiSchemaGeneration(o =>
        {
            o.AssemblyMarkerTypes = new[] { typeof(BaseDto) }; // add assembly marker
            o.AsyncApi = new AsyncApiDocument { Info = new Info { Title = "BotaniQue" } };
        });

        var services = builder.FindAndInjectClientEventHandlers(Assembly.GetExecutingAssembly());
        var app = builder.Build();

        app.MapAsyncApiDocuments();
        app.MapAsyncApiUi();

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

        // builder.WebHost.UseUrls("http://*:9999");

        var port = Environment.GetEnvironmentVariable("PORT") ?? "8181";
        var wsServer = new WebSocketServer($"ws://0.0.0.0:{port}");

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
                    BaseDtoWithJwt? dto = JsonSerializer.Deserialize<BaseDtoWithJwt>(message, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                    if (dto?.Jwt != null)
                    {
                        var jwtService = app.Services.GetRequiredService<JwtService>();
                        var jwtValid = jwtService.IsJwtTokenValid(dto.Jwt);
                        if (!jwtValid)
                        {
                            app.Services.GetRequiredService<WebSocketConnectionService>().RevertAuthentication(socket);
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