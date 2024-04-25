using System.Reflection;
using System.Text.Json;
using api.Events.Auth.Client;
using api.Extensions;
using api.Options;
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
using Testcontainers.PostgreSql;

namespace api;

public static class Startup
{
    private static readonly List<string> _publicEvents =
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

        if (args.Contains("Testing=true"))
        {
            var dbContainer = 
                new PostgreSqlBuilder()
                .WithDatabase("botanique")
                .WithUsername("root")
                .WithPassword("password")
                .Build();

            await dbContainer.StartAsync();
            
            var connectionString = dbContainer.GetConnectionString();
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
                    if (dto is not null && _publicEvents.Contains(dto.eventType) == false)
                    {
                        Console.WriteLine("Checking JWT token");
                        if (dto.Jwt is null)
                        {
                            throw new NotAuthenticatedException("JWT token is missing. Please log in.");
                        }
                        
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

        return app;
    }
}