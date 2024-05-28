using System.Reflection;
using System.Text.Json;
using api.Core.Options;
using api.Core.Services;
using api.Events.Auth.Client;
using api.Extensions;
using Fleck;
using Infrastructure;
using lib;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Shared.Dtos;
using Shared.Exceptions;
using Shared.Models;
using Testcontainers.PostgreSql;
using Timer = System.Timers.Timer;

namespace api;

public static class Startup
{
    private static readonly List<string> PublicEvents =
    [
        nameof(ClientWantsToLogIn),
        nameof(ClientWantsToLogOut),
        nameof(ClientWantsToSignUp)
    ];
    
    private static readonly List<string?> NonProdEnvironments = ["Development", "Testing"];
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    
    public static async Task Main(string[] args)
    {
        var app = await StartApi(args);
        await app.RunAsync();
    }

    public static async Task<WebApplication> StartApi(string[] args)
    {
        if (args.Contains("--prod"))
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        }
        
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
                connectionString ??= Environment.GetEnvironmentVariable("DbConnection");
                options.UseNpgsql(connectionString ?? throw new Exception("Connection string cannot be null"));
            });
        }

        builder.ConfigureOptions();
        builder.Services.AddServicesAndRepositories();
        
        var services = builder.FindAndInjectClientEventHandlers(Assembly.GetExecutingAssembly());
        
        var app = builder.Build();

        if (args.Contains("--db-init"))
        {
            var scope = app.Services.CreateScope();
            var db = await app.Services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContextAsync();
             
            if (NonProdEnvironments.Contains(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")))
            {
                await db.Database.EnsureDeletedAsync();
            }
            
            await db.Database.EnsureCreatedAsync();
            await db.Database.MigrateAsync();
            
            if (NonProdEnvironments.Contains(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")))
            {
                await db.SeedDevelopmentDataAsync(scope, app.Configuration["AzureBlob:DefaultPlantImageUrl"] ?? "https://example.com");
            }
        }

        builder.WebHost.UseUrls("http://*:9999");
        
        var port = Environment.GetEnvironmentVariable("PORT") ?? "8181";
        var wsServer = new WebSocketServer($"ws://0.0.0.0:{port}");
        
        wsServer.Start(socket =>
        {
            var keepAliveInterval = TimeSpan.FromSeconds(30);
            var keepAliveTimer = new Timer(keepAliveInterval.TotalMilliseconds)
            {
                AutoReset = true,
                Enabled = true
            };
            
            keepAliveTimer.Elapsed += (sender, args) =>
            {
                if (socket.IsAvailable)
                {
                    socket.Send("Keep alive");
                }
                else
                {
                    keepAliveTimer.Stop();
                    keepAliveTimer.Dispose();
                }
            };
            
            socket.OnOpen = () => app.Services.GetRequiredService<WebSocketConnectionService>().AddConnection(socket);
            socket.OnClose = () => app.Services.GetRequiredService<WebSocketConnectionService>().RemoveConnection(socket);
            socket.OnMessage = async message =>
            {
                Log.Information("Received message: {message}", message);
                try
                {
                    // Check if the message contains a JWT token and if it is valid
                    var dto = JsonSerializer.Deserialize<BaseDtoWithJwt>(message, JsonSerializerOptions);
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
        
        return app;
    }
}