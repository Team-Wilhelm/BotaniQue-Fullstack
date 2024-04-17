using System.Reflection;
using api.Options;
using Core.Options;
using Core.Services;
using Fleck;
using Infrastructure;
using Infrastructure.Repositories;
using lib;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Shared.Dtos;
using Shared.Models.Identity;

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
        var builder = WebApplication.CreateBuilder(args);
        
        var connectionString = builder.Configuration.GetConnectionString("BotaniqueDb");
        builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
        {
            connectionString ??= Environment.GetEnvironmentVariable("BotaniqueDb");
            options.UseNpgsql(connectionString ?? throw new Exception("Connection string cannot be null"));
        });

        builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JWT"));
        builder.Services.Configure<MqttOptions>(builder.Configuration.GetSection("MQTT"));
        builder.Services.AddSingleton<UserRepository>();
        builder.Services.AddSingleton<MqttSubscriberService>();
        // TODO: add repositories
        
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
        
        builder.WebHost.UseUrls("http://*:9999");
        
        var port = Environment.GetEnvironmentVariable("PORT") ?? "8181";
        var wsServer = new WebSocketServer($"ws://0.0.0.0:{port}");
        
        wsServer.Start(socket =>
        {
            socket.OnOpen = () => Console.WriteLine("Open!");
            socket.OnClose = () => Console.WriteLine("Close!");
            socket.OnMessage =  async message =>
            {
                try
                {
                    await app.InvokeClientEventHandler(services, socket, message);
                }
                catch (Exception e)
                {
                    // e.Handle(socket);
                }
            };
        });

        return app;
    }
}